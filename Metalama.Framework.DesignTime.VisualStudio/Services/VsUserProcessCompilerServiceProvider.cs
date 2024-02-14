// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.AspectExplorer;
using Metalama.Framework.DesignTime.Contracts.Classification;
using Metalama.Framework.DesignTime.Contracts.CodeLens;
using Metalama.Framework.DesignTime.Contracts.Diagnostics;
using Metalama.Framework.DesignTime.Contracts.EntryPoint;
using Metalama.Framework.DesignTime.Contracts.Preview;
using Metalama.Framework.DesignTime.Contracts.ServiceHub;
using Metalama.Framework.DesignTime.VersionNeutral;
using Metalama.Framework.DesignTime.VisualStudio.AspectExplorer;
using Metalama.Framework.DesignTime.VisualStudio.Classification;
using Metalama.Framework.DesignTime.VisualStudio.CodeLens;
using Metalama.Framework.DesignTime.VisualStudio.Preview;

namespace Metalama.Framework.DesignTime.VisualStudio.Services
{
    /// <summary>
    /// The implementation of <see cref="ICompilerServiceProvider"/> for the Visual Studio UI process.
    /// </summary>
    internal sealed class VsUserProcessCompilerServiceProvider : CompilerServiceProvider
    {
        protected override ICompilerService? GetServiceCore( string name )
            => name switch
            {
                nameof(IClassificationService) => new DesignTimeClassificationService( this.ServiceProvider ),
                nameof(ITransformationPreviewService) => new UserProcessTransformationPreviewService( this.ServiceProvider ),
                nameof(ICompileTimeEditingStatusService) => new CompileTimeEditingStatusService( this.ServiceProvider ),
                nameof(ICodeLensService) => new CodeLensService( this.ServiceProvider ),
                nameof(IServiceHubLocator) => new ServiceHubLocator( this.ServiceProvider ),
                nameof(IAspectDatabaseService) => new AspectDatabase( this.ServiceProvider ),

                // When components implement several services, we ask for the primary interface so we ensure there 
                // is a single instance of the component.
                nameof(ICompileTimeErrorStatusService) => this.GetService( typeof(ICompileTimeEditingStatusService) ),

                _ => base.GetServiceCore( name )
            };
    }
}