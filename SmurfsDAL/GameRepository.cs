using Microsoft.EntityFrameworkCore;
using Smurfs.SmurfsBL.entities;
using Smurfs.SmurfsDAL.Data;

namespace Smurfs.SmurfsDAL;

public class GameRepository
{
    private readonly SmurfsDbContext _ctx;
    public GameRepository(SmurfsDbContext context) => _ctx = context;

    // ── Forest ────────────────────────────────────────────────────────────────
    public async Task<Forest?> GetForestWithAllAsync(int forestId) =>
        await _ctx.Forests
            .Include(f => f.Creatures)
            .Include(f => f.Items)
            .FirstOrDefaultAsync(f => f.Idf == forestId);

    public async Task<Forest> CreateDefaultForestAsync()
    {
        var forest = new Forest
        {
            MinX = 0, MaxX = GameConstants.ForestWidth,
            MinY = 0, MaxY = GameConstants.ForestHeight,
            Creatures = new List<Creature>(),
            Items     = new List<Item>()
        };
        _ctx.Forests.Add(forest);
        await _ctx.SaveChangesAsync();
        return forest;
    }

    // ── Smurf ─────────────────────────────────────────────────────────────────
    public async Task<Schtroumpf> SaveSmurfAsync(Schtroumpf s)
    {
        if (s.Idc == 0) _ctx.Schtroumpfs.Add(s);
        else            _ctx.Schtroumpfs.Update(s);
        await _ctx.SaveChangesAsync();
        return s;
    }

    // ── Items ─────────────────────────────────────────────────────────────────
    public async Task RemoveItemAsync(Item item)
    {
        _ctx.Items.Remove(item);
        await _ctx.SaveChangesAsync();
    }

    public async Task<Item> AddItemAsync(Item item)
    {
        _ctx.Items.Add(item);
        await _ctx.SaveChangesAsync();
        return item;
    }

    // ── Save session ──────────────────────────────────────────────────────────
    public async Task SaveGameStateAsync(
        Schtroumpf smurf,
        IEnumerable<Creature> enemies,
        IEnumerable<Item> remainingItems)
    {
        _ctx.Schtroumpfs.Update(smurf);
        foreach (var e in enemies)
            _ctx.Creatures.Update(e);
        await _ctx.SaveChangesAsync();
    }

    // ── Seed nouvelle partie ──────────────────────────────────────────────────
    public async Task<Forest> SeedNewGameAsync()
    {
        var rng    = new Random();
        var forest = await CreateDefaultForestAsync();

        _ctx.Schtroumpfs.Add(new Schtroumpf
        {
            Name              = "Grand Schtroumpf",
            Health            = GameConstants.InitialHealth,
            PositionX         = 50,
            PositionY         = 250,
            IsGrandSchtroumpf = true,
            Forest            = forest
        });

        for (int i = 0; i < 3; i++)
            _ctx.Spiders.Add(new Spider
            {
                Name      = $"Spider {i + 1}",
                Health    = 30,
                PositionX = rng.Next(200, GameConstants.ForestWidth  - 50),
                PositionY = rng.Next(50,  GameConstants.ForestHeight - 50),
                Forest    = forest
            });

        for (int i = 0; i < 3; i++)
            _ctx.BzzFlies.Add(new BzzFly
            {
                Name      = $"BzzFly {i + 1}",
                Health    = 20,
                PositionX = rng.Next(200, GameConstants.ForestWidth  - 50),
                PositionY = rng.Next(50,  GameConstants.ForestHeight - 50),
                Forest    = forest
            });

        int[] xs = { 150, 300, 450, 600, 200, 500 };
        int[] ys = { 100, 200, 350, 150, 450, 400 };

        _ctx.BluePotions.Add(new BluePotion { PositionX = xs[0], PositionY = ys[0], Forest = forest });
        _ctx.BluePotions.Add(new BluePotion { PositionX = xs[1], PositionY = ys[1], Forest = forest });
        _ctx.RedPotions .Add(new RedPotion  { PositionX = xs[2], PositionY = ys[2], Forest = forest });
        _ctx.RedPotions .Add(new RedPotion  { PositionX = xs[3], PositionY = ys[3], Forest = forest });
        _ctx.Berries    .Add(new Berry      { PositionX = xs[4], PositionY = ys[4], Forest = forest });
        _ctx.Berries    .Add(new Berry      { PositionX = xs[5], PositionY = ys[5], Forest = forest });

        await _ctx.SaveChangesAsync();
        return forest;
    }
}
