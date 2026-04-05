using System.Drawing;
using System.Windows.Forms;
using Smurfs.SmurfsDAL;
using Smurfs.SmurfsDAL.Data;
using SmurfsGame.Engine;
using SmurfsGame.UserControls;

namespace SmurfsGame.Forms;

public class MainMenuForm : Form
{
    private Button              _btnNew;
    private Button              _btnLoad;
    private Button              _btnExit;
    private Label               _lblSelected;
    private CharacterType       _selectedCharacter = CharacterType.Regular;
    private System.Windows.Forms.Timer _timer;

    public MainMenuForm()
    {
        Text            = "Smurfs Forest Adventure";
        Size            = new Size(500, 580);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = Color.FromArgb(30, 80, 30);

        var lblTitle = new Label
        {
            Text      = "Smurfs Forest Adventure",
            Font      = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.LightCyan,
            AutoSize  = false,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(10, 20, 460, 50)
        };

        var lblSub = new Label
        {
            Text      = "Traverse la forêt, collecte les items, évite les ennemis !",
            Font      = new Font("Segoe UI", 10),
            ForeColor = Color.LightGreen,
            AutoSize  = false,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(10, 70, 460, 30)
        };

        var lblSelectChar = new Label
        {
            Text      = "Select Your Character:",
            Font      = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.LightCyan,
            AutoSize  = false,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(10, 105, 460, 25)
        };

        // Regular Smurf Button
        var btnRegular = CreateCharacterButton(
            "Regular Smurf",
            new Point(40, 135),
            CharacterType.Regular,
            Color.FromArgb(50, 100, 150)
        );

        // Girl Smurf Button
        var btnGirl = CreateCharacterButton(
            "Girl Smurf",
            new Point(190, 135),
            CharacterType.Girl,
            Color.FromArgb(200, 100, 150)
        );

        // Grand Smurf Button
        var btnGrand = CreateCharacterButton(
            "Grand Smurf",
            new Point(340, 135),
            CharacterType.Grand,
            Color.FromArgb(150, 120, 80)
        );

        // Selection indicator
        _lblSelected = new Label
        {
            Text      = "Selected: Regular Smurf",
            Font      = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.Yellow,
            AutoSize  = false,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(10, 285, 460, 30)
        };

        var updateSelection = (CharacterType type, string name) =>
        {
            _selectedCharacter = type;
            _lblSelected.Text = $"Selected: {name}";
        };

        btnRegular.Click += (_, __) => updateSelection(CharacterType.Regular, "Regular Smurf");
        btnGirl.Click    += (_, __) => updateSelection(CharacterType.Girl, "Girl Smurf");
        btnGrand.Click   += (_, __) => updateSelection(CharacterType.Grand, "Grand Smurf");

        _btnNew  = CreateBtn("New Game",  new Point(150, 340));
        _btnLoad = CreateBtn("Load Game", new Point(150, 410));
        _btnExit = CreateBtn("Exit",      new Point(150, 480));

        _btnNew.Click  += BtnNew_Click;
        _btnLoad.Click += BtnLoad_Click;
        _btnExit.Click += (_, __) => Application.Exit();

        Controls.AddRange(new Control[]
            { lblTitle, lblSub, lblSelectChar, btnRegular, btnGirl, btnGrand, _lblSelected, _btnNew, _btnLoad, _btnExit });

        _timer = new System.Windows.Forms.Timer { Interval = 400 };
        _timer.Start();
    }

    private Button CreateCharacterButton(string name, Point location, CharacterType type, Color backColor)
    {
        var panel = new Panel
        {
            Location  = location,
            Size      = new Size(120, 140),
            BackColor = backColor,
            Cursor    = Cursors.Hand
        };

        // Load and display character preview image
        var preview = new PictureBox
        {
            Location  = new Point(10, 10),
            Size      = new Size(100, 80),
            SizeMode  = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent,
            BorderStyle = BorderStyle.None
        };

        // Try to load image from Resources folder
        Image? loadedImage = null;

        try
        {
            // Try multiple possible paths
            string[] possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", GetImageFileName(type)),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Resources", GetImageFileName(type)),
                Path.Combine(Directory.GetCurrentDirectory(), "Resources", GetImageFileName(type))
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    // Create a copy of the image to avoid file locking issues
                    using (var original = Image.FromFile(path))
                    {
                        loadedImage = new Bitmap(original);
                    }
                    break;
                }
            }

            if (loadedImage != null)
                preview.Image = loadedImage;
            else
                System.Diagnostics.Debug.WriteLine($"Failed to find image for {type}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading image for {type}: {ex.Message}");
        }

        var label = new Label
        {
            Text      = name,
            Location  = new Point(5, 95),
            Size      = new Size(110, 35),
            Font      = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            TextAlign = System.Drawing.ContentAlignment.TopCenter,
            AutoSize  = false
        };

        panel.Controls.Add(preview);
        panel.Controls.Add(label);

        var btn = new Button
        {
            Location  = location,
            Size      = new Size(120, 140),
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.BringToFront();

        panel.Click += (_, __) => btn.PerformClick();
        preview.Click += (_, __) => btn.PerformClick();
        label.Click += (_, __) => btn.PerformClick();

        Controls.Add(panel);
        Controls.Add(btn);
        return btn;
    }

    private static string GetImageFileName(CharacterType type) => type switch
    {
        CharacterType.Girl => "GirlSmurfRight.png",
        CharacterType.Grand => "GrandSmurfRight.png",
        _ => "smurf_right.gif.gif"
    };

    private Button CreateBtn(string text, System.Drawing.Point loc) => new Button
    {
        Text      = text,
        Location  = loc,
        Size      = new Size(200, 50),
        Font      = new Font("Segoe UI", 12, FontStyle.Bold),
        BackColor = Color.FromArgb(60, 130, 60),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Cursor    = Cursors.Hand
    };

    private async void BtnNew_Click(object? sender, EventArgs e)
    {
        _btnNew.Enabled = false;
        try
        {
            var ctx = new SmurfsDbContext();
            await ctx.Database.EnsureCreatedAsync();
            var repo   = new GameRepository(ctx);
            var engine = new GameEngine(repo);
            await engine.StartNewGameAsync();
            _timer.Stop();
            Hide();
            new GameForm(engine, _selectedCharacter).ShowDialog();
            Show();
            _timer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { _btnNew.Enabled = true; }
    }

    private async void BtnLoad_Click(object? sender, EventArgs e)
    {
        _btnLoad.Enabled = false;
        try
        {
            var ctx = new SmurfsDbContext();
            await ctx.Database.EnsureCreatedAsync();
            var repo   = new GameRepository(ctx);
            var engine = new GameEngine(repo);
            await engine.LoadForestAsync(1);
            _timer.Stop();
            Hide();
            new GameForm(engine, _selectedCharacter).ShowDialog();
            Show();
            _timer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Aucune partie sauvegardée.\n{ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally { _btnLoad.Enabled = true; }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _timer?.Dispose();
        base.Dispose(disposing);
    }
}
