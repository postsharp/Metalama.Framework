using System;
using System.ComponentModel.Design;
using System.IO;
using Caravela.Framework.DesignTime.Vsix;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace CustomCommandSample
{
    internal sealed class ShowDiffCommand
    {
        public static async Task InitializeAsync( MyPackage package )
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
           
            var commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Assumes.Present( commandService );

            // must match the button GUID and ID specified in the .vsct file
            var cmdId = new CommandID(PackageGuids.guidMyCommandPackageCmdSet, PackageIds.MyCommandId); 
            var cmd = new MenuCommand((s, e) => Execute(package), cmdId);
            commandService.AddCommand(cmd);
        }

        private static void Execute(MyPackage package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();


            var activeFile = package.DTE.ActiveDocument.FullName;
            var activeProject = package.DTE.ActiveDocument.ProjectItem.ContainingProject.FullName;

            var relativePath = new Uri( activeFile ).MakeRelative( new Uri( activeProject ) );
            var transformedPath = Path.Combine( activeProject, "obj\\Debug", relativePath );

        }
    }
}
