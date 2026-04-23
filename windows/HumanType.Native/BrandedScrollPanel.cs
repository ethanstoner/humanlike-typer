using System.Drawing.Drawing2D;

namespace HumanType.Native;

public sealed class BrandedScrollPanel : Panel
{
    private const int ScrollbarWidth = 10;
    private const int ScrollbarInset = 6;
    private const int ThumbMinHeight = 52;
    private const int WheelStep = 72;

    private Control? content;
    private int scrollOffset;
    private bool draggingThumb;
    private int dragStartY;
    private int dragStartOffset;

    public BrandedScrollPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        BackColor = Color.FromArgb(10, 13, 21);
    }

    public void SetContent(Control control)
    {
        if (content is not null)
        {
            content.SizeChanged -= OnContentLayoutChanged;
            content.ControlAdded -= OnContentChildrenChanged;
            content.ControlRemoved -= OnContentChildrenChanged;
            Controls.Remove(content);
        }

        content = control;
        content.Location = Point.Empty;
        content.SizeChanged += OnContentLayoutChanged;
        content.ControlAdded += OnContentChildrenChanged;
        content.ControlRemoved += OnContentChildrenChanged;
        Controls.Add(content);
        content.BringToFront();
        UpdateLayoutState();
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        UpdateLayoutState();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        if (!NeedsScrollbar)
        {
            return;
        }

        SetScrollOffset(scrollOffset - Math.Sign(e.Delta) * WheelStep);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (!NeedsScrollbar)
        {
            return;
        }

        var thumbRect = GetThumbRect();
        if (thumbRect.Contains(e.Location))
        {
            draggingThumb = true;
            dragStartY = e.Y;
            dragStartOffset = scrollOffset;
            return;
        }

        var trackRect = GetTrackRect();
        if (!trackRect.Contains(e.Location))
        {
            return;
        }

        var pageStep = Math.Max(120, ClientSize.Height - 80);
        if (e.Y < thumbRect.Top)
        {
            SetScrollOffset(scrollOffset - pageStep);
        }
        else if (e.Y > thumbRect.Bottom)
        {
            SetScrollOffset(scrollOffset + pageStep);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!draggingThumb || !NeedsScrollbar)
        {
            Cursor = GetThumbRect().Contains(e.Location) ? Cursors.Hand : Cursors.Default;
            return;
        }

        var trackRect = GetTrackRect();
        var thumbRect = GetThumbRect();
        var availableTrack = Math.Max(1, trackRect.Height - thumbRect.Height);
        var scrollableHeight = Math.Max(1, GetScrollableHeight());
        var deltaY = e.Y - dragStartY;
        var nextOffset = dragStartOffset + (int)Math.Round(deltaY * (scrollableHeight / (double)availableTrack));
        SetScrollOffset(nextOffset);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        draggingThumb = false;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (!draggingThumb)
        {
            Cursor = Cursors.Default;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (!NeedsScrollbar)
        {
            return;
        }

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var trackBrush = new SolidBrush(Color.FromArgb(18, 24, 38));
        using var thumbBrush = new SolidBrush(Color.FromArgb(86, 98, 145));
        using var thumbHighlightBrush = new SolidBrush(Color.FromArgb(112, 126, 182));

        var trackRect = GetTrackRect();
        var thumbRect = GetThumbRect();

        using (var trackPath = CreateRoundedPath(trackRect, trackRect.Width))
        {
            e.Graphics.FillPath(trackBrush, trackPath);
        }

        using (var thumbPath = CreateRoundedPath(thumbRect, thumbRect.Width))
        {
            e.Graphics.FillPath(draggingThumb ? thumbHighlightBrush : thumbBrush, thumbPath);
        }
    }

    private void OnContentLayoutChanged(object? sender, EventArgs e)
    {
        UpdateLayoutState();
    }

    private void OnContentChildrenChanged(object? sender, ControlEventArgs e)
    {
        UpdateLayoutState();
    }

    private void UpdateLayoutState()
    {
        if (content is null)
        {
            return;
        }

        var availableWidth = Math.Max(0, ClientSize.Width - (NeedsScrollbar ? ScrollbarWidth + ScrollbarInset * 2 : 0));
        content.Width = availableWidth;

        if (content is FlowLayoutPanel flowPanel)
        {
            flowPanel.WrapContents = false;
            flowPanel.FlowDirection = FlowDirection.TopDown;
        }

        var preferredHeight = content.GetPreferredSize(new Size(availableWidth, 0)).Height;
        content.Height = Math.Max(preferredHeight, ClientSize.Height);

        scrollOffset = Math.Clamp(scrollOffset, 0, GetScrollableHeight());
        PositionContent();
        Invalidate();
    }

    private void PositionContent()
    {
        if (content is null)
        {
            return;
        }

        var availableWidth = Math.Max(0, ClientSize.Width - (NeedsScrollbar ? ScrollbarWidth + ScrollbarInset * 2 : 0));
        content.Width = availableWidth;
        content.Location = new Point(0, -scrollOffset);
    }

    private void SetScrollOffset(int nextOffset)
    {
        var clamped = Math.Clamp(nextOffset, 0, GetScrollableHeight());
        if (clamped == scrollOffset)
        {
            return;
        }

        scrollOffset = clamped;
        PositionContent();
        Invalidate();
    }

    private int GetScrollableHeight()
    {
        return content is null ? 0 : Math.Max(0, content.Height - ClientSize.Height);
    }

    private bool NeedsScrollbar => GetScrollableHeight() > 0;

    private Rectangle GetTrackRect()
    {
        return new Rectangle(
            ClientSize.Width - ScrollbarWidth - ScrollbarInset,
            ScrollbarInset,
            ScrollbarWidth,
            Math.Max(0, ClientSize.Height - ScrollbarInset * 2));
    }

    private Rectangle GetThumbRect()
    {
        var trackRect = GetTrackRect();
        if (!NeedsScrollbar)
        {
            return trackRect;
        }

        var visibleRatio = ClientSize.Height / (double)Math.Max(ClientSize.Height, content!.Height);
        var thumbHeight = Math.Max(ThumbMinHeight, (int)Math.Round(trackRect.Height * visibleRatio));
        thumbHeight = Math.Min(trackRect.Height, thumbHeight);

        var availableTrack = Math.Max(0, trackRect.Height - thumbHeight);
        var scrollableHeight = Math.Max(1, GetScrollableHeight());
        var thumbTop = trackRect.Top + (int)Math.Round(scrollOffset / (double)scrollableHeight * availableTrack);

        return new Rectangle(trackRect.Left, thumbTop, trackRect.Width, thumbHeight);
    }

    private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));

        if (diameter <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
