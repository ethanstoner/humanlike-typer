using System.Drawing.Drawing2D;

namespace HumanType.Native;

public sealed class RoundedPanel : Panel
{
    private int cornerRadius = 10;
    private Color borderColor = Color.FromArgb(24, 30, 48);
    private int borderWidth = 1;

    public RoundedPanel()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);
        BackColor = Color.FromArgb(12, 16, 25);
        Padding = new Padding(2);
    }

    public int CornerRadius
    {
        get => cornerRadius;
        set { cornerRadius = value; Invalidate(); }
    }

    public Color PanelBorderColor
    {
        get => borderColor;
        set { borderColor = value; Invalidate(); }
    }

    public int PanelBorderWidth
    {
        get => borderWidth;
        set { borderWidth = value; Invalidate(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var parentBg = Parent?.BackColor ?? Color.FromArgb(10, 13, 21);
        e.Graphics.Clear(parentBg);

        var rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
        using var path = CreateRoundedPath(rect, cornerRadius);
        using var fillBrush = new SolidBrush(BackColor);
        e.Graphics.FillPath(fillBrush, path);

        if (borderWidth > 0)
        {
            using var pen = new Pen(borderColor, borderWidth);
            e.Graphics.DrawPath(pen, path);
        }
    }

    private static GraphicsPath CreateRoundedPath(RectangleF rect, float radius)
    {
        var path = new GraphicsPath();
        radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f);
        var d = radius * 2;
        if (d <= 0) { path.AddRectangle(rect); return path; }
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
