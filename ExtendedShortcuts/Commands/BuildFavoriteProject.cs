namespace ExtendedShortcuts
{
    [Command(PackageIds.BuildFavoriteProject)]
    internal sealed class BuildFavoriteProject : BaseCommand<BuildFavoriteProject>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await VS.MessageBox.ShowWarningAsync("ExtendedShortcuts", "Button clicked");
        }
    }
}
