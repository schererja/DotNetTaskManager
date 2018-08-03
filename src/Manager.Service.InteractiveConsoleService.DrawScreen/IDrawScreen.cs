namespace Manager.Services.InteractiveConsoleService.DrawScreen
{
    /// <summary>
    ///     Interface for the Draw Screen Service
    /// </summary>
    public interface IDrawScreen
    {
        int ScreenHeights { get; set; }
        int ScreenWidths { get; set; }

        void DrawCliScreenNow();
        void DrawLogScreenNow();
        void DrawWholeScreenNow();
    }
}