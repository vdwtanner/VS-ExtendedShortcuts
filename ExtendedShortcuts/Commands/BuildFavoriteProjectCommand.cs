using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Globalization;
using System.Runtime.InteropServices;

namespace ExtendedShortcuts
{
    internal sealed class FavoriteProjectHelper
    {
        private static FavoriteProjectHelper _instance;
        public static FavoriteProjectHelper Instance { get { if (_instance == null) { _instance = new FavoriteProjectHelper(); } return _instance; }}

        public Project favoriteProject {get; private set;}

        public async Task SetFavoriteProjectAsync(Project project)
        {
            favoriteProject = project;
            await Logger.LogAsync($"{project.Name} set as Favorite Project.");
        }

    }

    [Command(PackageIds.BuildFavoriteProject)]
    internal sealed class BuildFavoriteProjectCommand : BaseCommand<BuildFavoriteProjectCommand>
    {
        private string genericText { get; set; }

        protected override Task InitializeCompletedAsync()
        {
            genericText = "Build Favorite Project";
            return base.InitializeCompletedAsync();
        }

        protected override void BeforeQueryStatus(EventArgs e)
        {
            var proj = FavoriteProjectHelper.Instance.favoriteProject;
            if(proj != null)
            {
                Command.Enabled = true;
                Command.Text = $"{genericText} ({proj.Name})";
            }
            else
            {
                Command.Enabled = false;
                Command.Text = $"{genericText} (NO FAVORITE SET)";
            } 
            base.BeforeQueryStatus(e);
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await Logger.LogAsync("BuildFavoriteProject clicked");
            var proj = FavoriteProjectHelper.Instance.favoriteProject;
            if (proj != null)
            {
                await Logger.LogAsync($"Attempting to build {proj.Name}");
                VS.Build.BuildProjectAsync(FavoriteProjectHelper.Instance.favoriteProject).FileAndForget($"Failed to {Command.Text}");
            }
        }
    }

    [Command(PackageIds.SetFavoriteProject)]
    internal sealed class SetFavoriteProjectCommand : BaseCommand<SetFavoriteProjectCommand>
    {

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await Logger.LogAsync("SetFavoriteProject clicked");

            var solutionItem = await VS.Solutions.GetActiveItemAsync();

            if (solutionItem == null)
            {
                await Logger.LogAsync("Nothing was selected?", Logger.Severity.Error);
                return;
            }

            Project selectedProject = solutionItem as Project;

            if (selectedProject == null)
            {
                await Logger.LogAsync($"selectedProject was null. {solutionItem.Name} may not have been a Project. (was actually a {solutionItem.Type})", Logger.Severity.Error);
                return;
            }

            await FavoriteProjectHelper.Instance.SetFavoriteProjectAsync(selectedProject);
        }
    }
}
