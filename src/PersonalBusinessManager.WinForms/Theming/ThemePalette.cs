using System.Drawing;

namespace PersonalBusinessManager.WinForms.Theming;

public static class ThemePalette
{
    public static readonly Color ApplicationBackground =
        FromHex("#111318");

    public static readonly Color SidebarBackground =
        FromHex("#171A20");

    public static readonly Color HeaderBackground =
        FromHex("#171A20");

    public static readonly Color PanelBackground =
        FromHex("#1D2128");

    public static readonly Color RaisedPanel =
        FromHex("#242932");

    public static readonly Color InputBackground =
        FromHex("#191D23");

    public static readonly Color InputHoverBackground =
        FromHex("#20252D");

    public static readonly Color InputDisabledBackground =
        FromHex("#20242B");

    public static readonly Color OverlayBackground =
        FromHex("#0B0D11");

    public static readonly Color TooltipBackground =
        FromHex("#2A303A");

    public static readonly Color GridAlternateRow =
        FromHex("#20252C");

    public static readonly Color GridSelectedRow =
        FromHex("#302B55");

    public static readonly Color GridHoverRow =
        FromHex("#282D37");

    public static readonly Color PrimaryText =
        FromHex("#F1F3F5");

    public static readonly Color SecondaryText =
        FromHex("#AAB1BB");

    public static readonly Color MutedText =
        FromHex("#8B94A3");

    public static readonly Color DisabledText =
        FromHex("#7F8896");

    public static readonly Color InverseText =
        FromHex("#111318");

    public static readonly Color LinkText =
        FromHex("#A99FFF");

    public static readonly Color LinkHoverText =
        FromHex("#C1BAFF");

    public static readonly Color PlaceholderText =
        FromHex("#7F8896");

    public static readonly Color BorderSubtle =
        FromHex("#2B313B");

    public static readonly Color BorderDefault =
        FromHex("#343B46");

    public static readonly Color BorderStrong =
        FromHex("#505968");

    public static readonly Color FocusBorder =
        FromHex("#A99FFF");

    public static readonly Color Divider =
        FromHex("#2B313B");

    public static readonly Color SelectionIndicator =
        FromHex("#7C6CF2");

    public static readonly Color Accent =
        FromHex("#7C6CF2");

    public static readonly Color AccentHover =
        FromHex("#9184F7");

    public static readonly Color AccentPressed =
        FromHex("#6959DC");

    public static readonly Color AccentSoft =
        FromHex("#302B55");

    public static readonly Color AccentBorder =
        FromHex("#8F83F5");

    public static readonly Color AccentText =
        FromHex("#A99FFF");

    public static readonly Color Success =
        FromHex("#46B981");

    public static readonly Color SuccessSoft =
        FromHex("#18352B");

    public static readonly Color SuccessBorder =
        FromHex("#2F8F68");

    public static readonly Color SuccessText =
        FromHex("#75D5A8");

    public static readonly Color Warning =
        FromHex("#D6A64A");

    public static readonly Color WarningSoft =
        FromHex("#382D18");

    public static readonly Color WarningBorder =
        FromHex("#A77D2E");

    public static readonly Color WarningText =
        FromHex("#E6C16F");

    public static readonly Color Danger =
        FromHex("#DC5C68");

    public static readonly Color DangerSoft =
        FromHex("#3B2026");

    public static readonly Color DangerBorder =
        FromHex("#B84A56");

    public static readonly Color DangerText =
        FromHex("#F0848E");

    public static readonly Color Information =
        FromHex("#5C9DED");

    public static readonly Color InformationSoft =
        FromHex("#192C43");

    public static readonly Color InformationBorder =
        FromHex("#3E78B9");

    public static readonly Color InformationText =
        FromHex("#82B7F5");

    public static readonly Color Neutral =
        FromHex("#8B94A3");

    public static readonly Color NeutralSoft =
        FromHex("#292E37");

    public static readonly Color NeutralBorder =
        FromHex("#505968");

    public static readonly Color NeutralText =
        FromHex("#C1C7D0");

    private static Color FromHex(string value)
    {
        return ColorTranslator.FromHtml(value);
    }
}
