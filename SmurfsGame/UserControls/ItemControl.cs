using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SmurfsGame.UserControls;

public enum ItemKind { BluePotion, RedPotion, Berry }

public class ItemControl : UserControl
{
    private ItemKind _kind;
    private Image? _img;

    public ItemKind Kind
    {
        get => _kind;
        set { _kind = value; LoadImage(); Invalidate(); }
    }

    public ItemControl()
    {
        Size = new Size(36, 36);
        BackColor = Color.Transparent;
        DoubleBuffered = true;
        SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint |
                 ControlStyles.AllPaintingInWmPaint, true);
    }

    private void LoadImage()
    {
        string resDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        string fileName = _kind switch
        {
            ItemKind.BluePotion => "blueportion.png",
            ItemKind.RedPotion => "redportion.png",
            ItemKind.Berry => "berry.png",
            _ => "berry.png"
        };

        _img?.Dispose();
        _img = LoadSafe(Path.Combine(resDir, fileName));

        if (_img != null && ImageAnimator.CanAnimate(_img))
            ImageAnimator.Animate(_img, OnFrameChanged);

        if (_img != null)
            Size = new Size(Math.Min(_img.Width, 50), Math.Min(_img.Height, 50));
    }

    private void OnFrameChanged(object? sender, EventArgs e)
    {
        if (InvokeRequired) BeginInvoke(new Action(Invalidate));
        else Invalidate();
    }

    private static Image? LoadSafe(string path)
    {
        try { return File.Exists(path) ? Image.FromFile(path) : null; }
        catch { return null; }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (_img != null)
        {
            ImageAnimator.UpdateFrames(_img);
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImage(_img, GetScaledRect(_img, Width, Height));
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_img != null && ImageAnimator.CanAnimate(_img))
                ImageAnimator.StopAnimate(_img, OnFrameChanged);
            _img?.Dispose();
        }
        base.Dispose(disposing);
    }
}