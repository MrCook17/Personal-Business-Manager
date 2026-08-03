using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

internal static class DropDownGlyphPainter
{
    public static void Draw(
        Graphics graphics,
        Rectangle bounds,
        Color color,
        int deviceDpi)
    {
        ArgumentNullException.ThrowIfNull(graphics);

        int halfWidth = DpiScaler.Scale(
            UiSpacing.Space4,
            deviceDpi);
        int verticalOffset = Math.Max(1, halfWidth / 2);
        Point centre = new(
            bounds.Left + (bounds.Width / 2),
            bounds.Top + (bounds.Height / 2));
        using var pen = new Pen(
            color,
            DpiScaler.Scale(
                UiDimensions.StandardBorderWidth,
                deviceDpi));
        graphics.DrawLines(
            pen,
            [
                new Point(centre.X - halfWidth, centre.Y - verticalOffset),
                new Point(centre.X, centre.Y + verticalOffset),
                new Point(centre.X + halfWidth, centre.Y - verticalOffset),
            ]);
    }
}
