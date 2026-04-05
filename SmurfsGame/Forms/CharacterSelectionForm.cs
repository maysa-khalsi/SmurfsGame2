using System.Drawing;
using System.Windows.Forms;
using SmurfsGame.UserControls;

namespace SmurfsGame.Forms;

public class CharacterSelectionForm : Form
{
    private CharacterType _selectedCharacter = CharacterType.Regular;
    public CharacterType SelectedCharacter => _selectedCharacter;

    public CharacterSelectionForm()
    {
        Text            = "Choose Your Character";
        Size            = new Size(500, 480);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        BackColor       = Color.FromArgb(30, 80, 30);
        KeyPreview      = true;
        KeyDown        += (_, e) => { if (e.KeyCode == Keys.Escape) DialogResult = DialogResult.Cancel; };

        var lblTitle = new Label
        {
            Text      = "Choose Your Character",
            Font      = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.LightCyan,
            AutoSize  = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(10, 20, 460, 40)
        };

        var lblSub = new Label
        {
            Text      = "Select a character to begin your adventure!",
            Font      = new Font("Segoe UI", 10),
            ForeColor = Color.LightGreen,
            AutoSize  = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(10, 60, 460, 25)
        };

        // Regular Smurf Button
        var btnRegular = CreateCharacterButton(
            "Regular Smurf",
            "The classic blue smurf.\nBalanced stats.",
            new Point(40, 110),
            CharacterType.Regular,
            Color.FromArgb(50, 100, 150)
        );

        // Girl Smurf Button
        var btnGirl = CreateCharacterButton(
            "Girl Smurf",
            "The elegant smurf.\nFast movement.",
            new Point(180, 110),
            CharacterType.Girl,
            Color.FromArgb(200, 100, 150)
        );

        // Grand Smurf Button
        var btnGrand = CreateCharacterButton(
            "Grand Smurf",
            "The wise smurf.\nExtra health.",
            new Point(320, 110),
            CharacterType.Grand,
            Color.FromArgb(150, 120, 80)
        );

        // Selection indicator
        var lblSelected = new Label
        {
            Text      = "Selected: Regular Smurf",
            Font      = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.Yellow,
            AutoSize  = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(10, 300, 460, 30)
        };

        var updateSelection = (CharacterType type, string name) =>
        {
            _selectedCharacter = type;
            lblSelected.Text = $"Selected: {name}";
        };

        btnRegular.Click += (_, __) => updateSelection(CharacterType.Regular, "Regular Smurf");
        btnGirl.Click    += (_, __) => updateSelection(CharacterType.Girl, "Girl Smurf");
        btnGrand.Click   += (_, __) => updateSelection(CharacterType.Grand, "Grand Smurf");

        // Start button
        var btnStart = new Button
        {
            Text      = "Start Game",
            Location  = new Point(150, 360),
            Size      = new Size(200, 50),
            Font      = new Font("Segoe UI", 12, FontStyle.Bold),
            BackColor = Color.FromArgb(60, 150, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        btnStart.Click += (_, __) => DialogResult = DialogResult.OK;

        Controls.AddRange(new Control[]
        {
            lblTitle, lblSub,
            btnRegular, btnGirl, btnGrand,
            lblSelected,
            btnStart
        });
    }

    private Button CreateCharacterButton(string name, string description, Point location, CharacterType type, Color backColor)
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
        string? imagePath = null;
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
                    imagePath = path;
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
            TextAlign = ContentAlignment.TopCenter,
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
            Cursor    = Cursors.Hand,
            Visible   = true,
            Dock      = DockStyle.None
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
}
