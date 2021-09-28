// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace Caravela.Framework.Impl.Fabrics
{
    internal class ProjectFabricDriver : FabricDriver
    {
        public ProjectFabricDriver( FabricContext context, IFabric fabric, Compilation runTimeCompilation ) :
            base( context, fabric, runTimeCompilation )
        {
            var attribute = this.Fabric.GetType().GetCustomAttribute<FabricAttribute>();

            var depth = 0;

            if ( attribute != null )
            {
                // Compute the distance of the path to the root.
                for ( var path = attribute.Path; !string.IsNullOrEmpty( path ); path = Path.GetDirectoryName( path ) )
                {
                    depth++;
                }

                this.OrderingKey = $"{depth:D3}:{this.Fabric.GetType().FullName}";
            }
            else
            {
                this.OrderingKey = $"XXX:{this.Fabric.GetType().FullName}";
            }
        }

        public override void Execute( IAspectBuilderInternal aspectBuilder )
        {
            var builder = new Builder( (ICompilation) aspectBuilder.Target, this.Context, aspectBuilder );
            ((IProjectFabric) this.Fabric).BuildFabric( builder );
        }

        public override FabricKind Kind => this.Fabric is ITransitiveProjectFabric ? FabricKind.Transitive : FabricKind.Compilation;

        public override string OrderingKey { get; }

        public override IDeclaration GetTarget( CompilationModel compilation ) => compilation;

        private class Builder : BaseBuilder<ICompilation>, IProjectFabricBuilder
        {
            public Builder( ICompilation compilation, FabricContext context, IAspectBuilderInternal aspectBuilder ) : base(
                compilation,
                context,
                aspectBuilder ) { }
        }
    }
}