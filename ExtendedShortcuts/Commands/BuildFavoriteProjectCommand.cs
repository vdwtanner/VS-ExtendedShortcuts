using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Globalization;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ExtendedShortcuts
{
    internal sealed class FavoriteProjectHelper
    {
        private static FavoriteProjectHelper _instance;
        public static FavoriteProjectHelper Instance { get { if (_instance == null) { _instance = new FavoriteProjectHelper(); } return _instance; }}

        public Project favoriteProject {get; private set;}
        private string _solutionDirectory;

        private string GetSettingsFilePath()
        {
            if (string.IsNullOrEmpty(_solutionDirectory))
                return null;
            
            var solutionName = Path.GetFileNameWithoutExtension(VS.Solutions.GetCurrentSolutionAsync().Result?.FullPath ?? "solution");
            return Path.Combine(_solutionDirectory, $".vs/{solutionName}/extendedshortcuts.user.json");
        }

        public async Task InitializeAsync()
        {
            var solution = await VS.Solutions.GetCurrentSolutionAsync();
            if (solution != null)
            {
                _solutionDirectory = Path.GetDirectoryName(solution.FullPath);
                await LoadFavoriteProjectAsync();
            }
        }

        private async Task LoadFavoriteProjectAsync()
        {
            try
            {
                var settingsFile = GetSettingsFilePath();
                if (settingsFile == null || !File.Exists(settingsFile))
                    return;

                var json = await System.Threading.Tasks.Task.Run(() => File.ReadAllText(settingsFile));
                var settings = JsonConvert.DeserializeObject<FavoriteProjectSettings>(json);
                
                if (settings?.FavoriteProjectPath != null)
                {
                    var projects = await VS.Solutions.GetAllProjectsAsync();
                    var project = projects.FirstOrDefault(p => p.FullPath == settings.FavoriteProjectPath);
                    
                    if (project != null)
                    {
                        favoriteProject = project;
                        await Logger.LogAsync($"Restored favorite project: {project.Name}");
                    }
                    else
                    {
                        await Logger.LogAsync($"Previous favorite project not found: {settings.FavoriteProjectPath}", Logger.Severity.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"Error loading favorite project settings: {ex.Message}", Logger.Severity.Error);
            }
        }

        private async Task SaveFavoriteProjectAsync()
        {
            try
            {
                var settingsFile = GetSettingsFilePath();
                if (settingsFile == null)
                    return;

                var directory = Path.GetDirectoryName(settingsFile);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var settings = new FavoriteProjectSettings
                {
                    FavoriteProjectPath = favoriteProject?.FullPath
                };

                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                await System.Threading.Tasks.Task.Run(() => File.WriteAllText(settingsFile, json));
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"Error saving favorite project settings: {ex.Message}", Logger.Severity.Error);
            }
        }

        public async Task SetFavoriteProjectAsync(Project project)
        {
            favoriteProject = project;
            await SaveFavoriteProjectAsync();
            await Logger.LogAsync($"{project.Name} set as Favorite Project.");
        }

        private class FavoriteProjectSettings
        {
            public string FavoriteProjectPath { get; set; }
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
