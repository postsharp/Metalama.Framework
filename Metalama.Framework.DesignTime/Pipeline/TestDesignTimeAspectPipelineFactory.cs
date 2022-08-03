// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline
{
    internal class TestDesignTimeAspectPipelineFactory : DesignTimeAspectPipelineFactory
    {
        private readonly IProjectOptions _projectOptions;

        public TestDesignTimeAspectPipelineFactory( CompileTimeDomain domain, IProjectOptions projectOptions ) : base(
            Engine.Pipeline.ServiceProvider.Empty,
            domain,
            true )
        {
            this._projectOptions = projectOptions;
        }

        protected override string GetProjectId( IProjectOptions projectOptions, Compilation compilation ) => compilation.AssemblyName!;

        protected override ValueTask<DesignTimeAspectPipeline?> GetPipelineAndWaitAsync( Compilation compilation, CancellationToken cancellationToken )
        {
            return new ValueTask<DesignTimeAspectPipeline?>( this.GetOrCreatePipeline( this._projectOptions, compilation, CancellationToken.None ) );
        }

        protected override bool HasMetalamaReference( Compilation compilation )
            => compilation.References.OfType<PortableExecutableReference>()
                .Any( x => Path.GetFileNameWithoutExtension( x.FilePath )!.Equals( "Metalama.Framework", StringComparison.OrdinalIgnoreCase ) );
    }
}