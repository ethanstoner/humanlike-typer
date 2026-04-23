using System.Drawing.Drawing2D;

namespace HumanType.Native;

public sealed class TrayMenuRenderer : ToolStripProfessionalRenderer
{
    private static readonly Color MenuBackground = Color.FromArgb(14, 18, 28);
    private static readonly Color MenuBorder = Color.FromArgb(34, 40, 59);
    private static readonly Color ItemHover = Color.FromArgb(31, 38, 56);
    private static readonly Color ItemPressed = Color.FromArgb(51, 61, 89);
    private static readonly Color Separator = Color.FromArgb(40, 47, 70);

    public TrayMenuRenderer() : base(new TrayColorTable())
    {
        RoundedEdges = false;
    }

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        using var brush = new SolidBrush(MenuBackground);
        e.Graphics.FillRectangle(brush, e.AffectedBounds);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        using var pen = new Pen(MenuBorder);
        var rect = new Rectangle(Point.Empty, e.ToolStrip.Size - new Size(1, 1));
        e.Graphics.DrawRectangle(pen, rect);
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        var bounds = new Rectangle(Point.Empty, e.Item.Size);
        var fill = e.Item.Pressed ? ItemPressed : e.Item.Selected ? ItemHover : MenuBackground;
        using var brush = new SolidBrush(fill);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.FillRectangle(brush, bounds);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        using var pen = new Pen(Separator);
        var y = e.Item.Bounds.Height / 2;
        e.Graphics.DrawLine(pen, 14, y, e.Item.Bounds.Width - 14, y);
    }

    private sealed class TrayColorTable : ProfessionalColorTable
    {
        public override Color MenuBorder => TrayMenuRenderer.MenuBorder;
        public override Color ToolStripDropDownBackground => TrayMenuRenderer.MenuBackground;
        public override Color ImageMarginGradientBegin => TrayMenuRenderer.MenuBackground;
        public override Color ImageMarginGradientMiddle => TrayMenuRenderer.MenuBackground;
        public override Color ImageMarginGradientEnd => TrayMenuRenderer.MenuBackground;
        public override Color MenuItemSelected => TrayMenuRenderer.ItemHover;
        public override Color MenuItemSelectedGradientBegin => TrayMenuRenderer.ItemHover;
        public override Color MenuItemSelectedGradientEnd => TrayMenuRenderer.ItemHover;
        public override Color MenuItemPressedGradientBegin => TrayMenuRenderer.ItemPressed;
        public override Color MenuItemPressedGradientMiddle => TrayMenuRenderer.ItemPressed;
        public override Color MenuItemPressedGradientEnd => TrayMenuRenderer.ItemPressed;
    }
}
