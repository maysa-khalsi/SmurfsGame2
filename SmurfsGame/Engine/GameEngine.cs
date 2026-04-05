using Smurfs.SmurfsBL.entities;
using Smurfs.SmurfsDAL;

namespace SmurfsGame.Engine;

public class GameEngine
{
    private readonly GameRepository _repo;
    private readonly Random         _rng = new();

    // Per-enemy: current cardinal direction
    private readonly Dictionary<int, (int dx, int dy)> _dirs = new();

    // Per-enemy: timestamp of last hit → prevents continuous damage every 80 ms
    private readonly Dictionary<int, DateTime> _lastHitTime = new();

    // Four cardinal directions only — no diagonal in a maze
    private static readonly (int dx, int dy)[] Cardinals =
        { (1,0), (-1,0), (0,1), (0,-1) };

    public GameState State { get; private set; } = new();

    public event Action<string>? ItemCollected;
    public event Action<string>? EnemyHit;
    public event Action?         GameOver;
    public event Action<Item>?   ItemSpawned;
    public event Action?         GameWon;

    public GameEngine(GameRepository repo) => _repo = repo;

    // ── Initialisation ────────────────────────────────────────────────────────

    public async Task StartNewGameAsync()
    {
        var forest = await _repo.SeedNewGameAsync();
        await LoadForestAsync(forest.Idf);
    }

    public async Task LoadForestAsync(int forestId)
    {
        var forest = await _repo.GetForestWithAllAsync(forestId)
                     ?? throw new Exception($"Forest {forestId} not found.");

        var maze = new Maze(GameConstants.ForestWidth, GameConstants.ForestHeight,
                            GameConstants.MazeCellSize);

        State = new GameState
        {
            Forest  = forest,
            Maze    = maze,
            Smurf   = forest.Creatures!.OfType<Schtroumpf>().First(),
            Enemies = forest.Creatures!.Where(c => c is not Schtroumpf).ToList(),
            Items   = new List<Item>()
        };

        // Smurf starts at maze entrance
        State.Smurf.PositionX = maze.StartPoint.x;
        State.Smurf.PositionY = maze.StartPoint.y;

        PlaceEnemiesInMaze();
        PlaceItemsInMaze();

        // Give each enemy a starting cardinal direction
        foreach (var e in State.Enemies)
        {
            _dirs[e.Idc]       = RandomCardinal();
            _lastHitTime[e.Idc] = DateTime.MinValue;
        }
    }

    // ── Enemy placement ───────────────────────────────────────────────────────

    private void PlaceEnemiesInMaze()
    {
        int minDist = 180; // keep enemies away from start

        foreach (var enemy in State.Enemies)
        {
            var cell = State.Maze.GetRandomFreeCellFar(
                State.Smurf.PositionX, State.Smurf.PositionY,
                minDist);

            enemy.PositionX = cell.x;
            enemy.PositionY = cell.y;
        }
    }

    // ── Item placement ────────────────────────────────────────────────────────

    private void PlaceItemsInMaze()
    {
        // 8 items total: mix of the three types, spread across the maze
        var itemTypes = new[] { 0, 1, 2, 0, 1, 2, 0, 1 }; // 0=Blue, 1=Red, 2=Berry

        var placed = new List<(int x, int y)>();

        foreach (int t in itemTypes)
        {
            (int x, int y) cell;
            int tries = 0;

            do
            {
                cell = State.Maze.GetRandomFreeCell();
                tries++;
            }
            // Keep items apart from each other and from start
            while (tries < 150 && (
                DistSq(cell.x, cell.y, State.Smurf.PositionX, State.Smurf.PositionY) < 80 * 80 ||
                placed.Any(p => DistSq(cell.x, cell.y, p.x, p.y) < 70 * 70)));

            Item item = t switch
            {
                0 => new BluePotion { PositionX = cell.x, PositionY = cell.y, Forest = State.Forest },
                1 => new RedPotion  { PositionX = cell.x, PositionY = cell.y, Forest = State.Forest },
                _ => new Berry      { PositionX = cell.x, PositionY = cell.y, Forest = State.Forest }
            };

            State.Items.Add(item);
            placed.Add(cell);
        }
    }

    // ── Smurf movement ────────────────────────────────────────────────────────

    public void MoveSmurf(int dx, int dy)
    {
        if (State.IsGameOver || State.IsGameWon) return;

        var s = State.Smurf;

        // Try full movement first
        int nx = s.PositionX + dx;
        int ny = s.PositionY + dy;

        if (!State.Maze.HasWallCollision(nx, ny, GameConstants.CollisionSize))
        {
            s.PositionX = nx;
            s.PositionY = ny;
        }
        else
        {
            // Wall-slide: try horizontal only
            if (!State.Maze.HasWallCollision(nx, s.PositionY, GameConstants.CollisionSize))
                s.PositionX = nx;
            // Wall-slide: try vertical only
            else if (!State.Maze.HasWallCollision(s.PositionX, ny, GameConstants.CollisionSize))
                s.PositionY = ny;
        }

        // Clamp strictly inside forest
        s.PositionX = Math.Clamp(s.PositionX, 0, GameConstants.ForestWidth  - GameConstants.CollisionSize);
        s.PositionY = Math.Clamp(s.PositionY, 0, GameConstants.ForestHeight - GameConstants.CollisionSize);

        CheckItemCollisions();
        CheckEnemyCollisions();
        CheckWinCondition();
    }

    // ── Tick ─────────────────────────────────────────────────────────────────

    public void Tick()
    {
        if (State.IsGameOver || State.IsGameWon) return;
        MoveEnemies();
        CheckEnemyCollisions();
        CheckWinCondition();
    }

    // ── Enemy AI ─────────────────────────────────────────────────────────────

    private void MoveEnemies()
    {
        var smurf = State.Smurf;

        foreach (var e in State.Enemies)
        {
            if (!_dirs.TryGetValue(e.Idc, out var dir))
                dir = _dirs[e.Idc] = RandomCardinal();

            int step = GameConstants.EnemyStep;
            int nx   = e.PositionX + dir.dx * step;
            int ny   = e.PositionY + dir.dy * step;

            if (!State.Maze.HasWallCollision(nx, ny, GameConstants.CollisionSize))
            {
                // Clear path — continue, but occasionally chase the smurf
                e.PositionX = nx;
                e.PositionY = ny;

                // 20 % chance to re-orient toward smurf if nearby
                double dist = Math.Sqrt(DistSq(e.PositionX, e.PositionY,
                                                smurf.PositionX, smurf.PositionY));
                if (dist < 250 && _rng.Next(5) == 0)
                    _dirs[e.Idc] = BestCardinalToward(e, smurf);
            }
            else
            {
                // Blocked — pick best valid cardinal toward smurf or random
                var newDir = ChooseNewDirection(e, smurf);
                _dirs[e.Idc] = newDir;

                // Try to move in the new direction immediately
                int nnx = e.PositionX + newDir.dx * step;
                int nny = e.PositionY + newDir.dy * step;
                if (!State.Maze.HasWallCollision(nnx, nny, GameConstants.CollisionSize))
                {
                    e.PositionX = nnx;
                    e.PositionY = nny;
                }
            }

            // Clamp within forest
            e.PositionX = Math.Clamp(e.PositionX, 0, GameConstants.ForestWidth  - GameConstants.CollisionSize);
            e.PositionY = Math.Clamp(e.PositionY, 0, GameConstants.ForestHeight - GameConstants.CollisionSize);
        }
    }

    /// <summary>
    /// Returns the cardinal direction that best moves enemy toward smurf,
    /// from the set of directions that don't immediately hit a wall.
    /// Falls back to any valid direction, then a random cardinal.
    /// </summary>
    private (int dx, int dy) ChooseNewDirection(Creature enemy, Creature target)
    {
        int step = GameConstants.EnemyStep;

        // Preferred: move toward target
        var pref = BestCardinalToward(enemy, target);

        // Test all cardinals, rank: preferred first, then others
        var ordered = Cardinals
            .OrderBy(d => d == pref ? 0 : 1)
            .ThenBy(_  => _rng.Next());

        foreach (var d in ordered)
        {
            int tx = enemy.PositionX + d.dx * step;
            int ty = enemy.PositionY + d.dy * step;
            if (!State.Maze.HasWallCollision(tx, ty, GameConstants.CollisionSize))
                return d;
        }

        return RandomCardinal(); // complete fallback
    }

    /// <summary>
    /// Cardinal direction that moves enemy closest to target (no wall check).
    /// </summary>
    private static (int dx, int dy) BestCardinalToward(Creature enemy, Creature target)
    {
        int dx = target.PositionX - enemy.PositionX;
        int dy = target.PositionY - enemy.PositionY;

        // Choose the axis with the greater distance
        if (Math.Abs(dx) >= Math.Abs(dy))
            return (Math.Sign(dx), 0);
        else
            return (0, Math.Sign(dy));
    }

    // ── Collision checks ─────────────────────────────────────────────────────

    private bool Overlaps(int ax, int ay, int bx, int by)
    {
        int sz = GameConstants.CollisionSize;
        return ax < bx + sz && ax + sz > bx &&
               ay < by + sz && ay + sz > by;
    }

    private void CheckItemCollisions()
    {
        var s    = State.Smurf;
        var hits = State.Items
                        .Where(i => Overlaps(s.PositionX, s.PositionY, i.PositionX, i.PositionY))
                        .ToList();

        foreach (var item in hits)
        {
            int gain = item.HealthValue;
            s.Health      = Math.Min(s.Health + gain, GameConstants.MaxHealth);
            State.Score  += gain * 10;
            State.Items.Remove(item);

            string name = item switch
            {
                BluePotion => "Blue Potion",
                RedPotion  => "Red Potion",
                Berry      => "Berry",
                _          => "Item"
            };
            ItemCollected?.Invoke($"+{gain} HP  {name}!");
            _ = _repo.RemoveItemAsync(item);

            // Spawn a replacement elsewhere in the maze
            SpawnSingleItem();
        }

        // Bonus batch when all items are collected
        if (!State.Items.Any())
        {
            State.Score += 500;
            ItemCollected?.Invoke("Bravo ! Nouveau lot ! +500 pts");
            SpawnBatch(6);
        }
    }

    private void CheckEnemyCollisions()
    {
        var s   = State.Smurf;
        var now = DateTime.UtcNow;

        foreach (var e in State.Enemies)
        {
            if (!Overlaps(s.PositionX, s.PositionY, e.PositionX, e.PositionY)) continue;

            // Cooldown: don't damage more than once per EnemyHitCooldown ms per enemy
            if (_lastHitTime.TryGetValue(e.Idc, out var last) &&
                (now - last).TotalMilliseconds < GameConstants.EnemyHitCooldown)
                continue;

            _lastHitTime[e.Idc] = now;

            int dmg = e switch
            {
                Spider => GameConstants.SpiderDamage,
                BzzFly => GameConstants.BzzFlyDamage,
                _      => 5
            };

            s.Health -= dmg;
            EnemyHit?.Invoke($"-{dmg} HP  {e.Name}!");

            if (s.Health <= 0)
            {
                s.Health         = 0;
                State.IsGameOver = true;
                GameOver?.Invoke();
                return;
            }
        }
    }

    private void CheckWinCondition()
    {
        var ep = State.Maze.EndPoint;
        if (Overlaps(State.Smurf.PositionX, State.Smurf.PositionY, ep.x, ep.y))
        {
            State.IsGameWon  = true;
            State.Score     += 1000;
            GameWon?.Invoke();
        }
    }

    // ── Spawn helpers ─────────────────────────────────────────────────────────

    private void SpawnSingleItem()
    {
        (int x, int y) cell;
        int tries = 0;

        do
        {
            cell = State.Maze.GetRandomFreeCell();
            tries++;
        }
        while (tries < 100 && (
            DistSq(cell.x, cell.y, State.Smurf.PositionX, State.Smurf.PositionY) < 60 * 60 ||
            State.Items.Any(i => DistSq(cell.x, cell.y, i.PositionX, i.PositionY) < 60 * 60)));

        Item newItem = _rng.Next(3) switch
        {
            0 => new BluePotion { PositionX = cell.x, PositionY = cell.y, Forest = State.Forest },
            1 => new RedPotion  { PositionX = cell.x, PositionY = cell.y, Forest = State.Forest },
            _ => new Berry      { PositionX = cell.x, PositionY = cell.y, Forest = State.Forest }
        };

        State.Items.Add(newItem);
        ItemSpawned?.Invoke(newItem);
    }

    private void SpawnBatch(int count)
    {
        for (int i = 0; i < count; i++)
        {
            (int x, int y) cell;
            int tries = 0;
            do
            {
                cell = State.Maze.GetRandomFreeCellFar(
                    State.Smurf.PositionX, State.Smurf.PositionY, 80);
                tries++;
            }
            while (tries < 100 &&
                   State.Items.Any(it => DistSq(cell.x, cell.y, it.PositionX, it.PositionY) < 60 * 60));

            Item newItem = (i % 3) switch
            {
                0 => new BluePotion { PositionX = cell.x, PositionY = cell.y, Forest = State.Forest },
                1 => new RedPotion  { PositionX = cell.x, PositionY = cell.y, Forest = State.Forest },
                _ => new Berry      { PositionX = cell.x, PositionY = cell.y, Forest = State.Forest }
            };

            State.Items.Add(newItem);
            _ = _repo.AddItemAsync(newItem);
            ItemSpawned?.Invoke(newItem);
        }
    }

    // ── Save ──────────────────────────────────────────────────────────────────

    public async Task SaveAsync() =>
        await _repo.SaveGameStateAsync(State.Smurf, State.Enemies, State.Items);

    // ── Utilities ─────────────────────────────────────────────────────────────

    /// <summary>Returns a random cardinal direction (no diagonal).</summary>
    private (int dx, int dy) RandomCardinal() =>
        Cardinals[_rng.Next(Cardinals.Length)];

    private static long DistSq(int ax, int ay, int bx, int by) =>
        (long)(ax - bx) * (ax - bx) + (long)(ay - by) * (ay - by);
}
