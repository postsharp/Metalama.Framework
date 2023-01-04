// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.Classification;
using Metalama.Framework.DesignTime.Contracts.CodeLens;
using Metalama.Framework.DesignTime.Contracts.EntryPoint;
using Metalama.Framework.DesignTime.Contracts.Preview;
using Metalama.Framework.DesignTime.Contracts.ServiceHub;
using Metalama.Framework.DesignTime.VersionNeutral;
using Metalama.Framework.DesignTime.VisualStudio.Classification;
using Metalama.Framework.DesignTime.VisualStudio.CodeLens;
using Metalama.Framework.DesignTime.VisualStudio.Preview;

namespace Metalama.Framework.DesignTime.VisualStudio
{
    /// <summary>
    /// The implementation of <see cref="ICompilerServiceProvider"/> for the Visual Studio UI process.
    /// </summary>
    internal sealed class VsUserProcessCompilerServiceProvider : CompilerServiceProvider
    {
        protected override ICompilerService? GetServiceCore( string name )
            => name switch
            {
                nameof(IClassificationService) => new DesignTimeClassificationService(),
                nameof(ITransformationPreviewService) => new UserProcessTransformationPreviewService( this.ServiceProvider ),
                nameof(ICompileTimeEditingStatusService) => new CompileTimeEditingStatusService( this.ServiceProvider ),
                nameof(ICodeLensService) => new CodeLensService( this.ServiceProvider ),
                nameof(IServiceHubLocator) => new ServiceHubLocator( this.ServiceProvider ),
                _ => base.GetServiceCore( name )
            };
    }
}