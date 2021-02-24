// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.ComponentModel.Design;
using System.IO;
using Caravela.Framework.DesignTime.Vsix;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace CustomCommandSample
{
    internal sealed class ShowDiffCommand
    {
        private readonly MyPackage _package;

        private ShowDiffCommand(MyPackage package)
        {
            this._package = package;
        }

        public static async Task InitializeAsync( MyPackage package )
        {
            var instance = new ShowDiffCommand( package );

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = await package.GetServiceAsync( typeof( IMenuCommandService ) ) as OleMenuCommandService;
            Assumes.Present( commandService );

            // must match the button GUID and ID specified in the .vsct file
            var cmdId = new CommandID(PackageGuids.guidMyCommandPackageCmdSet, PackageIds.MyCommandId); 
            var cmd = new MenuCommand((s, e) => instance.Execute(), cmdId);
            commandService.AddCommand(cmd);
        }

        private static string? GetIntermediateDirectory( string intermediateDirectory )
        {

            if ( Directory.Exists( Path.Combine( intermediateDirectory, "transformed" ) ) )
            {
                return intermediateDirectory;
            }
            else
            {
                foreach ( var child in Directory.GetDirectories( intermediateDirectory ) )
                {
                    var childIntermediate = GetIntermediateDirectory( child );

                    if ( childIntermediate != null )
                    {
                        return childIntermediate;
                    }
                }

                return null;
            }
        }

        private void Execute()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var activeDocumentPath = this._package.DTE.ActiveDocument.FullName;
            var activeProjectPath = this._package.DTE.ActiveDocument.ProjectItem.ContainingProject.FullName;

            var relativePath = new Uri( activeProjectPath ).MakeRelativeUri( new Uri( activeDocumentPath ) ).ToString().Replace( "/", "\\" );

            // TODO: Implement properly.
            var intermediateDirectory = GetIntermediateDirectory( Path.Combine( Path.GetDirectoryName( activeProjectPath ), "obj\\Debug" ));

            if ( intermediateDirectory != null ) 
            {
                var transformedDocumentPath = Path.Combine( activeProjectPath, intermediateDirectory, "transformed", relativePath );
                if ( File.Exists( transformedDocumentPath ) )
                {
                    this.DiffFilesUsingDefaultTool( this._package, activeDocumentPath, transformedDocumentPath );
                }
            }
        }

        private void DiffFilesUsingDefaultTool( MyPackage package, string file1, string file2 )
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // https://github.com/madskristensen/FileDiffer/blob/master/src/Commands/DiffFilesCommand.cs

            // This is the guid and id for the Tools.DiffFiles command
            var diffFilesCmd = "{5D4C0442-C0A2-4BE8-9B4D-AB1C28450942}";
            var diffFilesId = 256;
            object args = $"\"{file1}\" \"{file2}\"";

            package.DTE.Commands.Raise( diffFilesCmd, diffFilesId, ref args, ref args );
        }
    }
}
