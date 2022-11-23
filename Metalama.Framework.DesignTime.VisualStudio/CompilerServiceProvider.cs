// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.Classification;
using Metalama.Framework.DesignTime.Contracts.CodeLens;
using Metalama.Framework.DesignTime.Contracts.EntryPoint;
using Metalama.Framework.DesignTime.Contracts.Preview;
using Metalama.Framework.DesignTime.Contracts.ServiceHub;
using Metalama.Framework.DesignTime.VisualStudio.Classification;
using Metalama.Framework.DesignTime.VisualStudio.CodeLens;
using Metalama.Framework.DesignTime.VisualStudio.Preview;
using Metalama.Framework.Project;
using System.Collections.Concurrent;

namespace Metalama.Framework.DesignTime.VisualStudio
{
    /// <summary>
    /// The implementation of <see cref="ICompilerServiceProvider"/>.
    /// </summary>
    internal class CompilerServiceProvider : ICompilerServiceProvider
    {
        private readonly GlobalServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, ICompilerService?> _services = new( StringComparer.Ordinal );

        public CompilerServiceProvider( GlobalServiceProvider serviceProvider, ContractVersion[] contractVersions )
        {
            this.ContractVersions = contractVersions;
            this._serviceProvider = serviceProvider;
            this.Version = this.GetType().Assembly.GetName().Version!;
        }

        public Version Version { get; }

        public ContractVersion[] ContractVersions { get; }

        public ICompilerService? GetService( Type type ) => this.GetService( type.Name );

        private ICompilerService? GetService( string name ) => this._services.GetOrAdd( name, this.GetServiceCore );

        private ICompilerService? GetServiceCore( string name )
            => name switch
            {
                nameof(IClassificationService) => new DesignTimeClassificationService(),
                nameof(ITransformationPreviewService) => new UserProcessTransformationPreviewService( this._serviceProvider ),
                nameof(ICompileTimeEditingStatusService) => new CompileTimeEditingStatusService( this._serviceProvider ),
                nameof(ICodeLensService) => new CodeLensService( this._serviceProvider ),
                nameof(IServiceHubLocator) => new ServiceHubLocator( this._serviceProvider ),
                _ => null
            };
    }
}