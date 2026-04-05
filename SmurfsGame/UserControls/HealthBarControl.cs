using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Smurfs.SmurfsBL.entities;

namespace SmurfsGame.UserControls;

public class HealthBarControl : UserControl
{
    private int _health    = GameConstants.MaxHealth;
    private int _maxHealth = GameConstants.MaxHealth;

    public int Health
    {
        get => _health;
        set { _health = Math.Clamp(value, 0, _maxHealth); Invalidate(); }
    }

    public HealthBarControl()
    {
        Size           = new Size(200, 30);
        BackColor      = Color.Transparent;
        DoubleBuffered = true;
        SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var   g   = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        float pct = (float)_health / _maxHealth;

        g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(50,50,50)), 0, 4, Width, 20, 6);

        if (pct > 0)
        {
            int   fw  = (int)((Width - 2) * pct);
            Color clr = pct > 0.5f ? Color.FromArgb(40,200,60)
                      : pct > 0.25f ? Color.FromArgb(220,180,0)
                      : Color.FromArgb(220,50,50);

            using var br = new LinearGradientBrush(
                new Rectangle(1, 5, Math.Max(fw,1), 18),
                ControlPaint.Light(clr, 0.3f), clr,
                LinearGradientMode.Vertical);
            g.FillRoundedRectangle(br, 1, 5, fw, 18, 5);
            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(60,255,255,255)), 2, 6, Math.Max(fw-2,1), 6, 3);
        }

        using var borderPen = new Pen(Color.FromArgb(180,255,255,255), 1.5f);
        g.DrawRoundedRectangle(borderPen, 0, 4, Width-1, 20, 6);

        string txt = $"HP  {_health} / {_maxHealth}";
        using var font = new Font("Segoe UI", 8f, FontStyle.Bold);
        var sz = g.MeasureString(txt, font);
        g.DrawString(txt, font, Brushes.Black, (Width-sz.Width)/2+1, (Height-sz.Height)/2+1);
        g.DrawString(txt, font, Brushes.White, (Width-sz.Width)/2,   (Height-sz.Height)/2);
    }
}

internal static class GraphicsExt
{
    public static void FillRoundedRectangle(this Graphics g, Brush b, int x, int y, int w, int h, int r)
    {
        if (w <= 0 || h <= 0) return;
        using var path = RR(x, y, w, h, r);
        g.FillPath(b, path);
    }
    public static void DrawRoundedRectangle(this Graphics g, Pen p, int x, int y, int w, int h, int r)
    {
        using var path = RR(x, y, w, h, r);
        g.DrawPath(p, path);
    }
    private static System.Drawing.Drawing2D.GraphicsPath RR(int x, int y, int w, int h, int r)
    {
        r = Math.Min(r, Math.Min(w, h) / 2);
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(x, y, r*2, r*2, 180, 90);
        path.AddArc(x+w-r*2, y, r*2, r*2, 270, 90);
        path.AddArc(x+w-r*2, y+h-r*2, r*2, r*2, 0, 90);
        path.AddArc(x, y+h-r*2, r*2, r*2, 90, 90);
        path.CloseFigure();
        return path;
    }
}
