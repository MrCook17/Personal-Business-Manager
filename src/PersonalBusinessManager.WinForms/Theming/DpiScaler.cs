using System.Drawing;

namespace PersonalBusinessManager.WinForms.Theming;

public static class DpiScaler
{
    public const int BaselineDpi = 96;

    public static int Scale(
        int logicalPixels,
        int deviceDpi)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(
            logicalPixels);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            deviceDpi);

        return (int)Math.Round(
            logicalPixels * deviceDpi / (double)BaselineDpi,
            MidpointRounding.AwayFromZero);
    }

    public static Size Scale(
        Size logicalSize,
        int deviceDpi)
    {
        return new Size(
            Scale(logicalSize.Width, deviceDpi),
            Scale(logicalSize.Height, deviceDpi));
    }
}
