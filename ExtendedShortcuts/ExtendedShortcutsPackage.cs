global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
using System.Threading;

namespace ExtendedShortcuts
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuids.ExtendedShortcutsString)]
    public sealed class ExtendedShortcutsPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await Logger.InitAsync("ExtendedShortcuts");
            await Logger.LogAsync("Hello world");
            await this.RegisterCommandsAsync();
            await Logger.LogAsync("ExtendedShortcuts initialization completed.");
        }
    }
}