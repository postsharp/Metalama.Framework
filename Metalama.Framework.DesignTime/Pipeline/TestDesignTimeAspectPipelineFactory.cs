// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline
{
    internal class TestDesignTimeAspectPipelineFactory : DesignTimeAspectPipelineFactory
    {
        private readonly IProjectOptions _projectOptions;

        public TestDesignTimeAspectPipelineFactory( CompileTimeDomain domain, IProjectOptions projectOptions ) : base( domain, true )
        {
            this._projectOptions = projectOptions;
        }

        protected override string GetProjectId( IProjectOptions projectOptions, Compilation compilation ) => compilation.AssemblyName!;

        public override bool TryGetPipeline( Compilation compilation, [NotNullWhen( true )] out DesignTimeAspectPipeline? pipeline )
        {
            pipeline = this.GetOrCreatePipeline( this._projectOptions, compilation, CancellationToken.None );

            return pipeline != null;
        }
    }
}