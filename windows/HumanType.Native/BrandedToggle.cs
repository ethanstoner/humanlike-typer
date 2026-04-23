namespace HumanType.Native;

public sealed class BrandedToggle : CheckBox
{
    public BrandedToggle()
    {
        AutoSize = false;
        Size = new Size(72, 30);
        MinimumSize = Size;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        e.Graphics.Clear(Parent?.BackColor ?? Color.FromArgb(12, 16, 25));

        var trackRect = new RectangleF(2, 5, 60, 20);
        var knobX = Checked ? trackRect.Right - 18 : trackRect.Left + 2;
        var knobRect = new RectangleF(knobX, 7, 16, 16);

        using var trackBrush = new SolidBrush(Checked ? Color.FromArgb(99, 113, 255) : Color.FromArgb(42, 49, 71));
        using var knobBrush = new SolidBrush(Color.FromArgb(244, 246, 255));

        e.Graphics.FillRoundedRectangle(trackBrush, trackRect, 10);
        e.Graphics.FillEllipse(knobBrush, knobRect);
    }
}

internal static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, RectangleF rect, float radius)
    {
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        var diameter = radius * 2;
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        graphics.FillPath(brush, path);
    }
}
