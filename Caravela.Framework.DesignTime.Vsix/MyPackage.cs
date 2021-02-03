using System;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace CustomCommandSample
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("PostSharp \"Caravela\"", "", "1.0")]       
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid( "32e6d4ce-74c3-455d-bf30-34974837018e" )] // must match GUID in the .vsct file
    public sealed class MyPackage : AsyncPackage
    {
        public DTE DTE { get; private set; }

        // This method is run automatically the first time the command is being executed
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Query service asynchronously from the UI thread
            this.DTE = (DTE) await this.GetServiceAsync( typeof( EnvDTE.DTE ) );



            await ShowDiffCommand.InitializeAsync(this);
        }


    }
}
