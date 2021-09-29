// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System.IO;

namespace Caravela.Framework.Impl.Fabrics
{
    internal class ProjectFabricDriver : FabricDriver
    {
        public ProjectFabricDriver( AspectProjectConfiguration configuration, IFabric fabric, Compilation runTimeCompilation ) :
            base( configuration, fabric, runTimeCompilation )
        {
            var depth = 0;

            // Compute the distance of the path to the root.
            for ( var path = this.OriginalPath; !string.IsNullOrEmpty( path ); path = Path.GetDirectoryName( path ) )
            {
                depth++;
            }

            this.OrderingKey = $"{depth:D3}:{this.Fabric.GetType().FullName}";
        }

        public override ISymbol TargetSymbol => this.FabricSymbol.ContainingAssembly;

        public override void Execute( IAspectBuilderInternal aspectBuilder, FabricTemplateClass fabricTemplateClass )
        {
            var builder = new Builder( (ICompilation) aspectBuilder.Target, this.Configuration, aspectBuilder );
            ((IProjectFabric) this.Fabric).BuildProject( builder );
        }

        public override FabricKind Kind => this.Fabric is ITransitiveProjectFabric ? FabricKind.Transitive : FabricKind.Compilation;

        public override string OrderingKey { get; }

        public override IDeclaration GetTarget( CompilationModel compilation ) => compilation;

        private class Builder : BaseBuilder<ICompilation>, IProjectFabricBuilder
        {
            public Builder( ICompilation compilation, AspectProjectConfiguration context, IAspectBuilderInternal aspectBuilder ) : base(
                compilation,
                context,
                aspectBuilder ) { }
        }
    }
}