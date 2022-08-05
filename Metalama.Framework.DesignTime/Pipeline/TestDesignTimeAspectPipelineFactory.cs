// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline
{
    internal class TestDesignTimeAspectPipelineFactory : DesignTimeAspectPipelineFactory
    {
        private readonly IProjectOptions _projectOptions;

        public TestDesignTimeAspectPipelineFactory( CompileTimeDomain domain, ServiceProvider serviceProvider ) : base(
            serviceProvider,
            domain,
            true )
        {
            this._projectOptions = serviceProvider.GetRequiredService<IProjectOptions>();
        }

        protected override ValueTask<DesignTimeAspectPipeline?> GetPipelineAndWaitAsync( Compilation compilation, CancellationToken cancellationToken )
        {
            return new ValueTask<DesignTimeAspectPipeline?>( this.GetOrCreatePipeline( this._projectOptions, compilation, CancellationToken.None ) );
        }

        protected override bool IsMetalamaEnabled( Compilation compilation )
            => compilation.References.OfType<PortableExecutableReference>()
                .Any( x => Path.GetFileNameWithoutExtension( x.FilePath )!.Equals( "Metalama.Framework", StringComparison.OrdinalIgnoreCase ) );
    }
}