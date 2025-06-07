public static class ThemeManager
{
    public static bool IsDarkMode { get; private set; } = false;

    public static void ToggleTheme(Form form)
    {
        IsDarkMode = !IsDarkMode;
        ApplyTheme(form);
    }

    public static void ApplyTheme(Form form)
    {
        var backColor = IsDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;
        var foreColor = IsDarkMode ? Color.White : Color.Black;

        form.BackColor = backColor;
        form.ForeColor = foreColor;

        foreach (Control control in form.Controls)
        {
            ApplyThemeToControl(control, backColor, foreColor);
        }
    }

    private static void ApplyThemeToControl(Control control, Color backColor, Color foreColor)
    {
        control.BackColor = backColor;
        control.ForeColor = foreColor;

       
        foreach (Control child in control.Controls)
        {
            ApplyThemeToControl(child, backColor, foreColor);
        }
    }
}
