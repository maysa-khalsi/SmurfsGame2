using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Smurfs.SmurfsBL.entities;
using SmurfsGame.Engine;
using SmurfsGame.UserControls;

namespace SmurfsGame.Forms;

public class GameForm : Form
{
    private readonly GameEngine _engine;
    private readonly CharacterType _characterType;

    private readonly System.Windows.Forms.Timer _gameTimer;
    private readonly System.Windows.Forms.Timer _animTimer;
    private readonly System.Windows.Forms.Timer _msgTimer;

    private Panel _pnlForest = null!;

    private HealthBarControl _healthBar = null!;
    private Label            _lblStatus = null!;
    private Label            _lblScore  = null!;
    private Button           _btnSave   = null!;
    private Button           _btnMenu   = null!;

    private Control                      _smurfCtrl  = null!;
    private Dictionary<int, ItemControl>  _itemCtrls  = new();
    private Dictionary<int, EnemyControl> _enemyCtrls = new();

    private string _flashMsg   = "";
    private int    _flashAlpha = 0;
    private bool   _left, _right, _up, _down;

    public GameForm(GameEngine engine, CharacterType characterType = CharacterType.Regular)
    {
        _engine = engine;
        _characterType = characterType;

        _engine.ItemCollected += OnItemCollected;
        _engine.EnemyHit      += OnEnemyHit;
        _engine.GameOver      += OnGameOver;
        _engine.ItemSpawned   += OnItemSpawned;
        _engine.GameWon       += OnGameWon;

        InitUI();
        BuildSprites();

        _gameTimer = new System.Windows.Forms.Timer { Interval = 80 };
        _gameTimer.Tick += GameTimer_Tick;
        _gameTimer.Start();

        _animTimer = new System.Windows.Forms.Timer { Interval = 300 };
        _animTimer.Tick += AnimTimer_Tick;
        _animTimer.Start();

        _msgTimer = new System.Windows.Forms.Timer { Interval = 80 };
        _msgTimer.Tick += MsgTimer_Tick;
    }

    private void InitUI()
    {
        Text          = "Smurfs Maze Adventure";
        Size          = new Size(850, 700);
        MinimumSize   = new Size(850, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor     = Color.FromArgb(20, 60, 20);
        KeyPreview    = true;

        KeyDown += GameForm_KeyDown;
        KeyUp   += GameForm_KeyUp;
        Shown   += (_, __) => { ActiveControl = null; Focus(); };

        // HUD
        var pnlHud = new Panel
        {
            Bounds    = new Rectangle(0, 0, 840, 50),
            BackColor = Color.FromArgb(30, 100, 30)
        };

        _healthBar = new HealthBarControl
            { Location = new Point(10, 10), Size = new Size(220, 30),
              Health = GameConstants.InitialHealth };

        _lblScore = new Label
            { Text = "Score: 0", Font = new Font("Segoe UI", 12, FontStyle.Bold),
              ForeColor = Color.LightYellow, Location = new Point(250, 12), AutoSize = true };

        _lblStatus = new Label
            { Text = "Reach the end!", Font = new Font("Segoe UI", 10, FontStyle.Italic),
              ForeColor = Color.LightGreen, Location = new Point(420, 14), AutoSize = true };

        _btnSave = new Button
            { Text = "Save", Location = new Point(680, 8), Size = new Size(70, 34),
              FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(50,110,50),
              ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        _btnSave.Click += async (_, __) =>
        {
            await _engine.SaveAsync();
            ShowFlash("Game saved!");
            ActiveControl = null; Focus();
        };

        _btnMenu = new Button
            { Text = "Menu", Location = new Point(755, 8), Size = new Size(70, 34),
              FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(50,110,50),
              ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        _btnMenu.Click += (_, __) => Close();

        pnlHud.Controls.AddRange(new Control[]
            { _healthBar, _lblScore, _lblStatus, _btnSave, _btnMenu });

        // Maze panel
        _pnlForest = new Panel
        {
            Bounds    = new Rectangle(10, 60, GameConstants.ForestWidth, GameConstants.ForestHeight),
            BackColor = Color.FromArgb(20, 40, 20),
            TabStop   = false
        };
        _pnlForest.Paint     += PnlForest_Paint;
        _pnlForest.Click     += (_, __) => { ActiveControl = null; Focus(); };
        _pnlForest.MouseDown += (_, __) => { ActiveControl = null; Focus(); };

        // Légende
        var legend = new Label
        {
            Text =
                "CONTROLS\n<- -> up dn\n\n" +
                "ITEMS\nBlue Pot +20HP\nRed Pot  +10HP\nBerry     +5HP\n\n" +
                "ENEMIES\nSpider   -20HP\nBzzFly   -10HP\n\n" +
                "GOAL\nReach End!",
            Font      = new Font("Segoe UI", 8),
            ForeColor = Color.LightGreen,
            BackColor = Color.FromArgb(20, 50, 20),
            Bounds    = new Rectangle(820, 60, 150, 280),
            AutoSize  = false,
            TextAlign = System.Drawing.ContentAlignment.TopLeft,
            Padding   = new Padding(6)
        };

        Controls.AddRange(new Control[] { pnlHud, _pnlForest, legend });
    }

    private void ForwardKeys(Control ctrl)
    {
        ctrl.TabStop   = false;
        ctrl.KeyDown  += GameForm_KeyDown;
        ctrl.KeyUp    += GameForm_KeyUp;
        ctrl.MouseDown += (_, __) => { ActiveControl = null; Focus(); };
    }

    private void SetSmurfDirection(int direction)
    {
        // Direction: 0 = Right, 1 = Left, 2 = Up, 3 = Down
        if (_smurfCtrl is SchtroumpfControl smurf)
            smurf.SetDirection((SchtroumpfControl.Direction)direction);
        else if (_smurfCtrl is GirlSchtroumpfControl girl)
            girl.SetDirection((GirlSchtroumpfControl.Direction)direction);
        else if (_smurfCtrl is GrandSchtroumpfControl grand)
            grand.SetDirection((GrandSchtroumpfControl.Direction)direction);
    }

    private void BuildSprites()
    {
        var state = _engine.State;

        _smurfCtrl = _characterType switch
        {
            CharacterType.Girl => new GirlSchtroumpfControl
            {
                IsGrandSchtroumpf = false,
                Location          = new Point(state.Smurf.PositionX, state.Smurf.PositionY)
            },
            CharacterType.Grand => new GrandSchtroumpfControl
            {
                IsGrandSchtroumpf = true,
                Location          = new Point(state.Smurf.PositionX, state.Smurf.PositionY)
            },
            _ => new SchtroumpfControl
            {
                IsGrandSchtroumpf = false,
                Location          = new Point(state.Smurf.PositionX, state.Smurf.PositionY)
            }
        };
        ForwardKeys(_smurfCtrl);
        _pnlForest.Controls.Add(_smurfCtrl);

        foreach (var item in state.Items)
            AddItemSprite(item);

        foreach (var enemy in state.Enemies)
        {
            var kind = enemy is Spider ? EnemyKind.Spider : EnemyKind.BzzFly;
            var ctrl = new EnemyControl
                { Kind = kind, Location = new Point(enemy.PositionX, enemy.PositionY) };
            ForwardKeys(ctrl);
            _enemyCtrls[enemy.Idc] = ctrl;
            _pnlForest.Controls.Add(ctrl);
        }

        _smurfCtrl.BringToFront();
    }

    private void AddItemSprite(Item item)
    {
        var kind = item switch
        {
            BluePotion => ItemKind.BluePotion,
            RedPotion  => ItemKind.RedPotion,
            Berry      => ItemKind.Berry,
            _          => ItemKind.Berry
        };
        var ctrl = new ItemControl
            { Kind = kind, Location = new Point(item.PositionX, item.PositionY) };
        ForwardKeys(ctrl);
        _itemCtrls[item.Idi] = ctrl;
        _pnlForest.Controls.Add(ctrl);
    }

    // ── Timers ────────────────────────────────────────────────────────────────

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        if (_engine.State.IsGameOver || _engine.State.IsGameWon) { _gameTimer.Stop(); return; }

        int dx = 0, dy = 0;
        if (_left)  dx -= GameConstants.SmurfStep;
        if (_right) dx += GameConstants.SmurfStep;
        if (_up)    dy -= GameConstants.SmurfStep;
        if (_down)  dy += GameConstants.SmurfStep;

        if (dx != 0 || dy != 0) _engine.MoveSmurf(dx, dy);
        _engine.Tick();
        UpdateUI();
    }

    private void AnimTimer_Tick(object? sender, EventArgs e)
    {
        if (_smurfCtrl is SchtroumpfControl sc) sc.Step();
        else if (_smurfCtrl is GirlSchtroumpfControl gc) gc.Step();
        else if (_smurfCtrl is GrandSchtroumpfControl grc) grc.Step();

        foreach (var ec in _enemyCtrls.Values) ec.Animate();
    }

    private void MsgTimer_Tick(object? sender, EventArgs e)
    {
        _flashAlpha = Math.Max(0, _flashAlpha - 15);
        _pnlForest.Invalidate();
        if (_flashAlpha == 0) _msgTimer.Stop();
    }

    // ── UI sync ───────────────────────────────────────────────────────────────

    private void UpdateUI()
    {
        var state = _engine.State;
        _smurfCtrl.Location = new Point(state.Smurf.PositionX, state.Smurf.PositionY);

        foreach (var enemy in state.Enemies)
            if (_enemyCtrls.TryGetValue(enemy.Idc, out var ec))
                ec.Location = new Point(enemy.PositionX, enemy.PositionY);

        var collected = _itemCtrls.Keys
            .Except(state.Items.Select(i => i.Idi)).ToList();

        bool itemsRemoved = false;
        foreach (var id in collected)
        {
            if (_itemCtrls.TryGetValue(id, out var ctrl))
            {
                _pnlForest.Controls.Remove(ctrl);
                ctrl.Dispose();
                _itemCtrls.Remove(id);
                itemsRemoved = true;
            }
        }

        // Refresh the panel if items were removed
        if (itemsRemoved)
            _pnlForest.Refresh();

        _healthBar.Health = state.Smurf.Health;
        _lblScore.Text    = $"Score: {state.Score}";
    }

    // ── Paint ─────────────────────────────────────────────────────────────────

    private void PnlForest_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Color.FromArgb(20, 40, 20));

        DrawMaze(g);
        DrawEndPoint(g);

        if (_flashAlpha > 0 && !string.IsNullOrEmpty(_flashMsg))
        {
            using var b    = new SolidBrush(Color.FromArgb(_flashAlpha, Color.Yellow));
            using var font = new Font("Segoe UI", 16, FontStyle.Bold);
            var sz = g.MeasureString(_flashMsg, font);
            g.DrawString(_flashMsg, font, Brushes.Black,
                (_pnlForest.Width - sz.Width)/2+2, _pnlForest.Height/2+2);
            g.DrawString(_flashMsg, font, b,
                (_pnlForest.Width - sz.Width)/2, _pnlForest.Height/2);
        }
    }

    private void DrawMaze(Graphics g)
    {
        if (_engine.State.Maze == null) return;

        var maze = _engine.State.Maze;
        var grid = maze.GetGrid();
        int cellSize = maze.CellSize;

        // Dessiner les murs et les chemins
        using var wallBrush = new SolidBrush(Color.FromArgb(60, 30, 30));
        using var pathBrush = new SolidBrush(Color.FromArgb(60, 80, 60));

        for (int row = 0; row < grid.GetLength(0); row++)
        {
            for (int col = 0; col < grid.GetLength(1); col++)
            {
                int x = col * cellSize;
                int y = row * cellSize;

                if (grid[row, col])
                {
                    // Mur
                    g.FillRectangle(wallBrush, x, y, cellSize, cellSize);
                    using var pen = new Pen(Color.FromArgb(40, 20, 20), 1);
                    g.DrawRectangle(pen, x, y, cellSize, cellSize);
                }
                else
                {
                    // Chemin
                    g.FillRectangle(pathBrush, x, y, cellSize, cellSize);
                    using var pen = new Pen(Color.FromArgb(40, 60, 40), 1);
                    g.DrawRectangle(pen, x, y, cellSize, cellSize);
                }
            }
        }
    }

    private void DrawEndPoint(Graphics g)
    {
        if (_engine.State.Maze == null) return;

        var endPoint = _engine.State.Maze.EndPoint;
        using var endBrush = new SolidBrush(Color.FromArgb(255, 215, 0)); // Or
        using var pen = new Pen(Color.White, 2);

        g.FillRectangle(endBrush, endPoint.x, endPoint.y, GameConstants.CollisionSize, GameConstants.CollisionSize);
        g.DrawRectangle(pen, endPoint.x, endPoint.y, GameConstants.CollisionSize, GameConstants.CollisionSize);

        // Dessiner "END" sur le point de fin
        using var font = new Font("Segoe UI", 10, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.Black);
        var text = "END";
        var sz = g.MeasureString(text, font);
        g.DrawString(text, font, textBrush,
            endPoint.x + (GameConstants.CollisionSize - sz.Width) / 2,
            endPoint.y + (GameConstants.CollisionSize - sz.Height) / 2);
    }

    private void ShowFlash(string msg)
    {
        _flashMsg = msg; _flashAlpha = 255; _msgTimer.Start();
    }

    // ── Events engine ─────────────────────────────────────────────────────────

    private void OnItemCollected(string msg)
        => Invoke(() => { ShowFlash(msg); _lblStatus.Text = msg; _pnlForest.Invalidate(); });

    private void OnItemSpawned(Item item)
    {
        Invoke(() =>
        {
            AddItemSprite(item);
            _smurfCtrl.BringToFront();
        });
    }

    private void OnEnemyHit(string msg)
    {
        Invoke(() =>
        {
            ShowFlash(msg); _lblStatus.Text = msg;
            BackColor = Color.DarkRed;
            var t = new System.Windows.Forms.Timer { Interval = 200 };
            t.Tick += (_, __) => { BackColor = Color.FromArgb(20,60,20); t.Stop(); };
            t.Start();
        });
    }

    private void OnGameOver()
    {
        Invoke(() =>
        {
            _gameTimer.Stop(); _animTimer.Stop();
            MessageBox.Show(
                $"You died!\nFinal Score: {_engine.State.Score}",
                "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            Close();
        });
    }

    private void OnGameWon()
    {
        Invoke(() =>
        {
            _gameTimer.Stop(); _animTimer.Stop();
            MessageBox.Show(
                $"You reached the end! Congratulations!\nFinal Score: {_engine.State.Score}",
                "Victory!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        });
    }

    // ── Clavier ───────────────────────────────────────────────────────────────

    private void GameForm_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Left:
                _left = true;
                SetSmurfDirection(1); // Left
                e.Handled = true; break;
            case Keys.Right:
                _right = true;
                SetSmurfDirection(0); // Right
                e.Handled = true; break;
            case Keys.Up:
                _up = true;
                SetSmurfDirection(2); // Up
                e.Handled = true; break;
            case Keys.Down:
                _down = true;
                SetSmurfDirection(3); // Down
                e.Handled = true; break;
            case Keys.Escape: Close(); break;
        }
    }

    private void GameForm_KeyUp(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Left:  _left  = false; break;
            case Keys.Right: _right = false; break;
            case Keys.Up:    _up    = false; break;
            case Keys.Down:  _down  = false; break;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _gameTimer?.Dispose();
            _animTimer?.Dispose();
            _msgTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
