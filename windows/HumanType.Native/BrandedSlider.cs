using System.Drawing.Drawing2D;

namespace HumanType.Native;

public sealed class BrandedSlider : Control
{
    private const float TrackInset = HandleRadius + 2f;
    private const float HandleRadius = 9f;
    private const int ValueBoxWidth = 112;

    private double minimum;
    private double maximum = 100;
    private double value;
    private double step = 1;
    private bool dragging;
    private bool syncingText;
    private readonly TextBox valueBox = new();
    private readonly Label suffixLabel = new();

    public event EventHandler? ValueChanged;

    public string Title { get; set; } = string.Empty;
    public string Suffix
    {
        get => suffix;
        set
        {
            suffix = value;
            suffixLabel.Text = suffix;
            UpdateValueText();
            LayoutValueBox();
            Invalidate();
        }
    }

    private string suffix = string.Empty;

    public double Minimum
    {
        get => minimum;
        set
        {
            minimum = value;
            if (this.value < minimum)
            {
                Value = minimum;
            }
            Invalidate();
        }
    }

    public double Maximum
    {
        get => maximum;
        set
        {
            maximum = Math.Max(value, minimum + 0.001);
            if (this.value > maximum)
            {
                Value = maximum;
            }
            Invalidate();
        }
    }

    public double Step
    {
        get => step;
        set => step = Math.Max(value, 0.0001);
    }

    public double Value
    {
        get => this.value;
        set
        {
            var snapped = Snap(value);
            if (Math.Abs(this.value - snapped) < 0.0001)
            {
                return;
            }

            this.value = snapped;
            UpdateValueText();
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public BrandedSlider()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        DoubleBuffered = true;
        Size = new Size(360, 82);
        Cursor = Cursors.Hand;
        Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
        ForeColor = Color.FromArgb(242, 245, 255);
        BackColor = Color.FromArgb(12, 16, 25);

        valueBox.BorderStyle = BorderStyle.None;
        valueBox.BackColor = Color.FromArgb(22, 29, 44);
        valueBox.ForeColor = Color.FromArgb(243, 246, 255);
        valueBox.Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold, GraphicsUnit.Point);
        valueBox.TextAlign = HorizontalAlignment.Center;
        valueBox.TabStop = true;
        valueBox.ShortcutsEnabled = true;
        valueBox.TextChanged += (_, _) => HandleValueTextChanged();
        valueBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                CommitValueText();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                UpdateValueText();
                Select();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        };
        valueBox.Leave += (_, _) => CommitValueText();
        Controls.Add(valueBox);

        suffixLabel.AutoSize = false;
        suffixLabel.BackColor = Color.FromArgb(22, 29, 44);
        suffixLabel.ForeColor = Color.FromArgb(142, 152, 184);
        suffixLabel.Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold, GraphicsUnit.Point);
        suffixLabel.TextAlign = ContentAlignment.MiddleLeft;
        Controls.Add(suffixLabel);

        UpdateValueText();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var titleBrush = new SolidBrush(Color.FromArgb(132, 142, 176));
        using var valueBackBrush = new SolidBrush(Color.FromArgb(22, 29, 44));
        using var smallFont = new Font("Segoe UI", 9f, FontStyle.Regular);

        e.Graphics.DrawString(Title, smallFont, titleBrush, new PointF(0, 2));

        var pillRect = GetValuePillRect();
        FillRoundedRect(e.Graphics, valueBackBrush, pillRect, 14);

        var trackRect = GetTrackRect();
        using var trackBrush = new SolidBrush(Color.FromArgb(28, 34, 49));
        using var fillBrush = new LinearGradientBrush(
            new PointF(trackRect.Left, trackRect.Top),
            new PointF(trackRect.Right, trackRect.Top),
            Color.FromArgb(110, 122, 255),
            Color.FromArgb(145, 154, 255));

        using var trackPath = CreateRoundedPath(trackRect, trackRect.Height / 2f);
        e.Graphics.FillPath(trackBrush, trackPath);

        var fillWidth = (float)((Value - Minimum) / (Maximum - Minimum) * trackRect.Width);
        if (fillWidth > 0)
        {
            var state = e.Graphics.Save();
            e.Graphics.SetClip(trackPath);
            e.Graphics.FillRectangle(fillBrush, trackRect.Left, trackRect.Top, fillWidth, trackRect.Height);
            e.Graphics.Restore(state);
        }

        var handleX = trackRect.Left + fillWidth;
        handleX = Math.Clamp(handleX, trackRect.Left, trackRect.Right);
        var handleRect = new RectangleF(handleX - HandleRadius, trackRect.Top - 6, HandleRadius * 2, HandleRadius * 2);
        using var handleBrush = new SolidBrush(Color.FromArgb(244, 246, 255));
        using var outlinePen = new Pen(Color.FromArgb(74, 84, 112), 2);
        e.Graphics.FillEllipse(handleBrush, handleRect);
        e.Graphics.DrawEllipse(outlinePen, handleRect);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutValueBox();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (valueBox.Bounds.Contains(e.Location))
        {
            return;
        }

        dragging = true;
        UpdateFromMouse(e.X);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (dragging)
        {
            UpdateFromMouse(e.X);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        dragging = false;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (!MouseButtons.HasFlag(MouseButtons.Left))
        {
            dragging = false;
        }
    }

    private void UpdateFromMouse(int x)
    {
        var trackRect = GetTrackRect();
        var usableWidth = Math.Max(1d, trackRect.Width);
        var ratio = Math.Clamp((x - trackRect.Left) / usableWidth, 0, 1);
        Value = Minimum + ratio * (Maximum - Minimum);
    }

    private RectangleF GetTrackRect()
    {
        return new RectangleF(TrackInset, 50, Math.Max(0, Width - TrackInset * 2), 8);
    }

    private RectangleF GetValuePillRect()
    {
        return new RectangleF(Math.Max(0, Width - ValueBoxWidth), 0, ValueBoxWidth, 30);
    }

    private void LayoutValueBox()
    {
        var pillRect = GetValuePillRect();
        using var graphics = CreateGraphics();
        using var suffixFont = new Font("Segoe UI Semibold", 9f, FontStyle.Bold, GraphicsUnit.Point);
        var suffixWidth = string.IsNullOrEmpty(Suffix)
            ? 0
            : Math.Min(38, (int)Math.Ceiling(graphics.MeasureString(Suffix, suffixFont).Width) + 4);
        var innerWidth = Math.Max(20, (int)Math.Round(pillRect.Width - 16));
        var valueWidth = Math.Max(32, innerWidth - suffixWidth);

        valueBox.SetBounds(
            (int)Math.Round(pillRect.X + 8),
            (int)Math.Round(pillRect.Y + 5),
            valueWidth,
            20);
        suffixLabel.SetBounds(
            valueBox.Right,
            (int)Math.Round(pillRect.Y),
            suffixWidth,
            (int)Math.Round(pillRect.Height));
    }

    private void HandleValueTextChanged()
    {
        if (syncingText || valueBox.Focused)
        {
            return;
        }

        CommitValueText();
    }

    private void CommitValueText()
    {
        if (syncingText)
        {
            return;
        }

        var raw = valueBox.Text.Trim();
        if (Suffix.Length > 0 && raw.EndsWith(Suffix, StringComparison.OrdinalIgnoreCase))
        {
            raw = raw[..^Suffix.Length].Trim();
        }

        if (double.TryParse(raw, out var parsed))
        {
            Value = parsed;
        }

        UpdateValueText();
    }

    private void UpdateValueText()
    {
        syncingText = true;
        valueBox.Text = Suffix == "%"
            ? $"{Value:0.00}"
            : $"{Value:0}";
        syncingText = false;
    }

    private double Snap(double raw)
    {
        var clamped = Math.Clamp(raw, Minimum, Maximum);
        var snapped = Math.Round((clamped - Minimum) / Step) * Step + Minimum;
        return Math.Clamp(snapped, Minimum, Maximum);
    }

    private static void FillRoundedRect(Graphics graphics, Brush brush, RectangleF rect, int radius)
    {
        using var path = CreateRoundedPath(rect, radius);
        graphics.FillPath(brush, path);
    }

    private static GraphicsPath CreateRoundedPath(RectangleF rect, float radius)
    {
        var path = new GraphicsPath();
        radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f);
        var diameter = radius * 2;
        if (diameter <= 0)
        {
            path.AddRectangle(rect);
            return path;
        }

        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
