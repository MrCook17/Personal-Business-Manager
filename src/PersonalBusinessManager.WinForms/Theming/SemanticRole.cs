namespace PersonalBusinessManager.WinForms.Theming;

public enum SemanticRole
{
    Neutral,
    Information,
    Success,
    Warning,
    Danger,
    Accent,
}

public readonly record struct SemanticColors(
    Color Main,
    Color Background,
    Color Border,
    Color Text);

public static class SemanticTheme
{
    public static SemanticColors GetColors(
        SemanticRole role)
    {
        return role switch
        {
            SemanticRole.Neutral => new SemanticColors(
                ThemePalette.Neutral,
                ThemePalette.NeutralSoft,
                ThemePalette.NeutralBorder,
                ThemePalette.NeutralText),
            SemanticRole.Information => new SemanticColors(
                ThemePalette.Information,
                ThemePalette.InformationSoft,
                ThemePalette.InformationBorder,
                ThemePalette.InformationText),
            SemanticRole.Success => new SemanticColors(
                ThemePalette.Success,
                ThemePalette.SuccessSoft,
                ThemePalette.SuccessBorder,
                ThemePalette.SuccessText),
            SemanticRole.Warning => new SemanticColors(
                ThemePalette.Warning,
                ThemePalette.WarningSoft,
                ThemePalette.WarningBorder,
                ThemePalette.WarningText),
            SemanticRole.Danger => new SemanticColors(
                ThemePalette.Danger,
                ThemePalette.DangerSoft,
                ThemePalette.DangerBorder,
                ThemePalette.DangerText),
            SemanticRole.Accent => new SemanticColors(
                ThemePalette.Accent,
                ThemePalette.AccentSoft,
                ThemePalette.AccentBorder,
                ThemePalette.AccentText),
            _ => throw new ArgumentOutOfRangeException(
                nameof(role)),
        };
    }
}
