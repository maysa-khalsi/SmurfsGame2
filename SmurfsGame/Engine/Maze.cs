namespace SmurfsGame.Engine;

/// <summary>
/// Maze generated with Recursive Backtracking (DFS).
/// Grid: true = wall, false = walkable path.
/// Odd-indexed cells [row,col] are rooms; even-indexed are walls or connectors.
/// </summary>
public class Maze
{
    public int Width    { get; }
    public int Height   { get; }
    public int CellSize { get; }

    public (int x, int y) StartPoint { get; private set; }
    public (int x, int y) EndPoint   { get; private set; }

    private readonly bool[,] _grid;
    private readonly Random  _rng = new();

    // Cached list of all walkable cell pixel positions (top-left of cell)
    private readonly List<(int x, int y)> _freeCells = new();

    public Maze(int width, int height, int cellSize = 50)
    {
        Width    = width;
        Height   = height;
        CellSize = cellSize;

        // Force odd row/col count so the DFS algorithm works correctly
        int rows = ((height / cellSize) | 1);
        int cols = ((width  / cellSize) | 1);

        _grid = new bool[rows, cols];
        GenerateMaze(rows, cols);
        BuildFreeCellList(rows, cols);

        // Start = top-left free cell, End = bottom-right free cell
        StartPoint = SnapToFreeCellPixel(CellSize, CellSize);
        EndPoint   = SnapToFreeCellPixel((cols - 2) * CellSize, (rows - 2) * CellSize);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// True if the sprite rectangle (x,y,size,size) overlaps any wall cell.
    /// Checks all 4 corners with a 2-pixel inset for smoother navigation.
    /// </summary>
    public bool HasWallCollision(int x, int y, int size)
    {
        const int m = 2; // inset margin
        return IsWall(x + m,        y + m)        ||
               IsWall(x + size - m, y + m)        ||
               IsWall(x + m,        y + size - m) ||
               IsWall(x + size - m, y + size - m);
    }

    public bool[,] GetGrid() => _grid;

    /// <summary>
    /// Returns a random walkable cell pixel position (top-left corner, centered in cell).
    /// </summary>
    public (int x, int y) GetRandomFreeCell()
    {
        if (_freeCells.Count == 0) return (CellSize, CellSize);
        return _freeCells[_rng.Next(_freeCells.Count)];
    }

    /// <summary>
    /// Returns a random free cell that is at least minPixelDist away from a given point.
    /// Falls back to any free cell after maxAttempts.
    /// </summary>
    public (int x, int y) GetRandomFreeCellFar(int fromX, int fromY,
                                                int minPixelDist, int maxAttempts = 200)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            var c = GetRandomFreeCell();
            double d = Math.Sqrt(Math.Pow(c.x - fromX, 2) + Math.Pow(c.y - fromY, 2));
            if (d >= minPixelDist) return c;
        }
        return GetRandomFreeCell();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private bool IsWall(int px, int py)
    {
        int col = px / CellSize;
        int row = py / CellSize;
        if (row < 0 || row >= _grid.GetLength(0) ||
            col < 0 || col >= _grid.GetLength(1))
            return true; // out-of-bounds = wall
        return _grid[row, col];
    }

    private void GenerateMaze(int rows, int cols)
    {
        // Fill all cells as walls
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                _grid[r, c] = true;

        // Carve passages from cell (1,1)
        _grid[1, 1] = false;
        CarveFrom(1, 1, rows, cols);

        // Always open the exit cell
        _grid[rows - 2, cols - 2] = false;

        // Open a border entrance/exit for better playability
        _grid[1, 0] = false; // left opening near start
    }

    private void CarveFrom(int row, int col, int rows, int cols)
    {
        // Directions: up, down, left, right (step of 2 to skip over walls)
        var dirs = new[] { (-2, 0), (2, 0), (0, -2), (0, 2) };

        // Fisher-Yates shuffle
        for (int i = dirs.Length - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (dirs[i], dirs[j]) = (dirs[j], dirs[i]);
        }

        foreach (var (dr, dc) in dirs)
        {
            int nr = row + dr;
            int nc = col + dc;

            if (nr >= 1 && nr < rows - 1 && nc >= 1 && nc < cols - 1 && _grid[nr, nc])
            {
                _grid[row + dr / 2, col + dc / 2] = false; // knock out wall between
                _grid[nr, nc] = false;                       // open destination cell
                CarveFrom(nr, nc, rows, cols);
            }
        }
    }

    private void BuildFreeCellList(int rows, int cols)
    {
        _freeCells.Clear();
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (!_grid[r, c])
                    // Pixel = top-left of cell, centered so sprite sits in corridor
                    _freeCells.Add((c * CellSize, r * CellSize));
    }

    private (int x, int y) SnapToFreeCellPixel(int preferredPx, int preferredPy)
    {
        (int x, int y) best  = _freeCells.Count > 0 ? _freeCells[0] : (CellSize, CellSize);
        double        bestD  = double.MaxValue;

        foreach (var c in _freeCells)
        {
            double d = Math.Pow(c.x - preferredPx, 2) + Math.Pow(c.y - preferredPy, 2);
            if (d < bestD) { bestD = d; best = c; }
        }
        return best;
    }
}
