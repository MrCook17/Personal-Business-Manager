using System.Drawing;

namespace PersonalBusinessManager.WinForms.Theming;

public static class UiFonts
{
    private const string RegularFamily = "Segoe UI Variable Text";
    private const string SemiboldFamily = "Segoe UI Variable Text Semibold";
    private const string MonospaceFamily = "Cascadia Mono";

    public static Font Caption { get; } =
        Create(RegularFamily, 8.5F);

    public static Font Small { get; } =
        Create(RegularFamily, 9F);

    public static Font Body { get; } =
        Create(RegularFamily, 10F);

    public static Font BodyStrong { get; } =
        Create(SemiboldFamily, 10F, FontStyle.Bold);

    public static Font Label { get; } =
        Create(SemiboldFamily, 9.5F, FontStyle.Bold);

    public static Font Button { get; } =
        Create(SemiboldFamily, 9.5F, FontStyle.Bold);

    public static Font SectionHeading { get; } =
        Create(SemiboldFamily, 12F, FontStyle.Bold);

    public static Font DialogHeading { get; } =
        Create(SemiboldFamily, 15F, FontStyle.Bold);

    public static Font PageHeading { get; } =
        Create(SemiboldFamily, 20F, FontStyle.Bold);

    public static Font DashboardValue { get; } =
        Create(SemiboldFamily, 18F, FontStyle.Bold);

    public static Font MonospaceSmall { get; } =
        Create(MonospaceFamily, 9F);

    public static void Dispose()
    {
        Caption.Dispose();
        Small.Dispose();
        Body.Dispose();
        BodyStrong.Dispose();
        Label.Dispose();
        Button.Dispose();
        SectionHeading.Dispose();
        DialogHeading.Dispose();
        PageHeading.Dispose();
        DashboardValue.Dispose();
        MonospaceSmall.Dispose();
    }

    private static Font Create(
        string preferredFamily,
        float size,
        FontStyle fallbackStyle = FontStyle.Regular)
    {
        var preferred = new Font(
            preferredFamily,
            size,
            FontStyle.Regular,
            GraphicsUnit.Point);

        if (string.Equals(
                preferred.Name,
                preferredFamily,
                StringComparison.OrdinalIgnoreCase))
        {
            return preferred;
        }

        preferred.Dispose();

        return new Font(
            "Segoe UI",
            size,
            fallbackStyle,
            GraphicsUnit.Point);
    }
}
