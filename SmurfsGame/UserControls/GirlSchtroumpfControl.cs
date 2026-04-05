using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;


namespace SmurfsGame.UserControls
{
    public partial class GirlSchtroumpfControl : UserControl
    {
        public enum Direction { Right, Left, Up, Down }

        private Direction _currentDir = Direction.Right;
        private bool _isGrand;

        private Image? _imgRight;
        private Image? _imgLeft;
        private Image? _imgUp;
        private Image? _imgDown;

        public bool IsGrandSchtroumpf
        {
            get => _isGrand;
            set { _isGrand = value; Invalidate(); }
        }
        public GirlSchtroumpfControl()
        {
            Size = new Size(40, 40);
            BackColor = Color.FromArgb(60, 80, 60);  // Maze path green
            Padding = new Padding(0);
            Margin = new Padding(0);
            DoubleBuffered = true;
            BorderStyle = BorderStyle.None;
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint, true);
            LoadImages();
            StartGifAnimation();
        }
        private void LoadImages()
        {
            string resDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");

            _imgRight = LoadSafe(Path.Combine(resDir, "GirlSmurfRight.png"));
            _imgLeft = LoadSafe(Path.Combine(resDir, "GirlSmurfLeft.png"));
            _imgUp = LoadSafe(Path.Combine(resDir, "GirlSmurfUp.png"));
            _imgDown = LoadSafe(Path.Combine(resDir, "GirlSmurfDown.png"));

            if (_imgRight != null)
                Size = new Size(
                    Math.Min(_imgRight.Width, 40),
                    Math.Min(_imgRight.Height, 40));
        }

        private static Image? LoadSafe(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var img = Image.FromFile(path);
                    return img;
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load image {path}: {ex.Message}");
                return null;
            }
        }

        private void StartGifAnimation()
        {
            var img = CurrentImage();
            if (img != null && ImageAnimator.CanAnimate(img))
                ImageAnimator.Animate(img, OnFrameChanged);
        }

        private void OnFrameChanged(object? sender, EventArgs e)
        {
            if (InvokeRequired) BeginInvoke(new Action(Invalidate));
            else Invalidate();
        }

        public void SetDirection(Direction dir)
        {
            if (_currentDir == dir) return;

            var old = CurrentImage();
            if (old != null && ImageAnimator.CanAnimate(old))
                ImageAnimator.StopAnimate(old, OnFrameChanged);

            _currentDir = dir;

            var next = CurrentImage();
            if (next != null && ImageAnimator.CanAnimate(next))
                ImageAnimator.Animate(next, OnFrameChanged);

            Invalidate();
        }

        public void Step() => Invalidate();

        private Image? CurrentImage() => _currentDir switch
        {
            Direction.Right => _imgRight,
            Direction.Left => _imgLeft,
            Direction.Up => _imgUp,
            Direction.Down => _imgDown,
            _ => _imgRight
        };

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            // Don't clear - let the green background show through
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var img = CurrentImage();
            if (img != null)
            {
                ImageAnimator.UpdateFrames(img);
                g.DrawImage(img, GetScaledRect(img, Width, Height));
            }
            else
            {
                // Fallback: draw a placeholder if image not loaded
                using (var brush = new SolidBrush(Color.FromArgb(200, 100, 150)))
                    g.FillEllipse(brush, 5, 5, Width - 10, Height - 10);
            }
        }

        private static Rectangle GetScaledRect(Image img, int maxW, int maxH)
        {
            float ri = (float)img.Width / img.Height;
            float rc = (float)maxW / maxH;
            int w, h;
            if (ri > rc) { w = maxW; h = (int)(maxW / ri); }
            else { h = maxH; w = (int)(maxH * ri); }
            return new Rectangle((maxW - w) / 2, (maxH - h) / 2, w, h);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Paint the green background to match the maze
            e.Graphics.Clear(BackColor);
        }
    }
}
