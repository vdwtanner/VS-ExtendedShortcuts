namespace ExtendedShortcuts
{
    [Command(PackageIds.BuildFavoriteProject)]
    internal sealed class BuildFavoriteProjectCommand : BaseCommand<BuildFavoriteProjectCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await Logger.LogAsync("BuildFavoriteProject clicked");
        }
    }

    [Command(PackageIds.SetFavoriteProject)]
    internal sealed class SetFavoriteProjectCommand : BaseCommand<SetFavoriteProjectCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await Logger.LogAsync("SetFavoriteProject clicked");
        }
    }
}
