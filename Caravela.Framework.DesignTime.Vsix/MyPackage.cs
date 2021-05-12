// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using CustomCommandSample;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Caravela.Framework.DesignTime.Vsix
{
    [ProvideAutoLoad( UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad )]
    [PackageRegistration( UseManagedResourcesOnly = true, AllowsBackgroundLoading = true )]
    [InstalledProductRegistration( "PostSharp \"Caravela\"", "", "1.0" )]
    [ProvideMenuResource( "Menus.ctmenu", 1 )]
    [Guid( PackageGuids.guidPackageString )] // must match GUID in the .vsct file
    public sealed class MyPackage : AsyncPackage
    {
        public DTE? DTE { get; private set; }

        // This method is run automatically the first time the command is being executed
        protected override async Task InitializeAsync( CancellationToken cancellationToken, IProgress<ServiceProgressData> progress )
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync( cancellationToken );

            // Query service asynchronously from the UI thread
            this.DTE = (DTE) await this.GetServiceAsync( typeof( DTE ) )!;

            await ShowDiffCommand.InitializeAsync( this );
        }
    }
}
