using MahApps.Metro.Controls;

namespace ZenovaLauncher.Controls
{
    public sealed class LauncherSideMenuSelectionChangedEventArgs
    {
        /// <summary>
        /// Gets the newly selected menu item.
        /// </summary>
        /// <returns>The newly selected menu item.</returns>
        public object SelectedItem { get; internal set; }
    }

    public sealed class LauncherSideMenuDisplayModeChangedEventArgs
    {
        internal LauncherSideMenuDisplayModeChangedEventArgs(SplitViewDisplayMode displayMode)
        {
            DisplayMode = displayMode;
        }

        /// <summary>
        /// Gets the new display mode.
        /// </summary>
        /// <returns>The new display mode.</returns>
        public SplitViewDisplayMode DisplayMode { get; }
    }
}
