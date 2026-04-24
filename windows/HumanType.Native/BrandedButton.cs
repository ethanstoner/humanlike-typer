using System.Drawing.Drawing2D;

namespace HumanType.Native;

public sealed class BrandedButton : Control
{
    private Color baseBackColor = Color.FromArgb(24, 30, 46);
    private Color baseForeColor = Color.FromArgb(233, 237, 255);
    private Color hoverBackColor = Color.FromArgb(34, 42, 62);
    private Color pressBackColor = Color.FromArgb(18, 24, 38);
    private Color borderColor = Color.FromArgb(42, 49, 71);
    private int borderWidth = 1;
    private int cornerRadius = 8;
    private bool isHovering;
    private bool isPressed;

    public event EventHandler? Clicked;

    public BrandedButton()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);
        Size = new Size(144, 40);
        Cursor = Cursors.Hand;
        Font = new Font("Segoe UI Semibold", 9.75f, FontStyle.Bold);
        TabStop = false;
    }

    public Color BaseBackColor
    {
        get => baseBackColor;
        set { baseBackColor = value; ComputeHoverColors(); Invalidate(); }
    }

    public Color BaseForeColor
    {
        get => baseForeColor;
        set { baseForeColor = value; Invalidate(); }
    }

    public Color BorderColor
    {
        get => borderColor;
        set { borderColor = value; Invalidate(); }
    }

    public int BorderWidth
    {
        get => borderWidth;
        set { borderWidth = value; Invalidate(); }
    }

    public int CornerRadius
    {
        get => cornerRadius;
        set { cornerRadius = value; Invalidate(); }
    }

    public bool IsPrimary
    {
        set
        {
            if (value)
            {
                baseBackColor = Color.FromArgb(99, 113, 255);
                baseForeColor = Color.White;
                borderWidth = 0;
                ComputeHoverColors();
                Invalidate();
            }
        }
    }

    private void ComputeHoverColors()
    {
        hoverBackColor = Lighten(baseBackColor, 18);
        pressBackColor = Darken(baseBackColor, 12);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        e.Graphics.Clear(Parent?.BackColor ?? Color.FromArgb(12, 16, 25));

        var rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
        var fill = !Enabled ? Darken(baseBackColor, 20) : isPressed ? pressBackColor : isHovering ? hoverBackColor : baseBackColor;

        using var path = CreateRoundedPath(rect, cornerRadius);
        using var brush = new SolidBrush(fill);
        e.Graphics.FillPath(brush, path);

        if (borderWidth > 0)
        {
            using var pen = new Pen(isHovering ? Lighten(borderColor, 20) : borderColor, borderWidth);
            e.Graphics.DrawPath(pen, path);
        }

        var textSize = e.Graphics.MeasureString(Text, Font);
        var textX = (Width - textSize.Width) / 2f;
        var textY = (Height - textSize.Height) / 2f;
        using var textBrush = new SolidBrush(Enabled ? baseForeColor : Color.FromArgb(90, baseForeColor));
        e.Graphics.DrawString(Text, Font, textBrush, textX, textY);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        if (!Enabled) return;
        base.OnMouseEnter(e);
        isHovering = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        isHovering = false;
        isPressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (!Enabled) return;
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            isPressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (!Enabled) return;
        base.OnMouseUp(e);
        if (isPressed)
        {
            isPressed = false;
            Invalidate();
            if (ClientRectangle.Contains(e.Location))
            {
                Clicked?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        isHovering = false;
        isPressed = false;
        Cursor = Enabled ? Cursors.Hand : Cursors.Default;
        Invalidate();
    }

    private static Color Lighten(Color c, int amount)
    {
        return Color.FromArgb(c.A,
            Math.Min(255, c.R + amount),
            Math.Min(255, c.G + amount),
            Math.Min(255, c.B + amount));
    }

    private static Color Darken(Color c, int amount)
    {
        return Color.FromArgb(c.A,
            Math.Max(0, c.R - amount),
            Math.Max(0, c.G - amount),
            Math.Max(0, c.B - amount));
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
