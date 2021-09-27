// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.CodeModel;
using System;
using System.IO;
using System.Reflection;

namespace Caravela.Framework.Impl.Fabrics
{
    internal class CompilationFabricDriver : FabricDriver
    {
        public CompilationFabricDriver( IServiceProvider serviceProvider, AspectClassRegistry aspectClasses, IFabric fabric ) :
            base( serviceProvider, aspectClasses, fabric )
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

        public override FabricResult Execute( IProject project )
        {
            var builder = new Builder( this.ServiceProvider, project, this.AspectClasses );
            ((IProjectFabric) this.Fabric).BuildFabric( builder );

            return new FabricResult( builder );
        }

        public override FabricKind Kind => this.Fabric is ITransitiveProjectFabric ? FabricKind.Transitive : FabricKind.Compilation;

        public override string OrderingKey { get; }

        private class Builder : BaseBuilder<ICompilation>, IProjectFabricBuilder
        {
            public Builder( IServiceProvider serviceProvider, IProject project, AspectClassRegistry aspectClasses ) : base(
                serviceProvider,
                project,
                aspectClasses ) { }

            protected override ICompilation GetTargetDeclaration( CompilationModel compilation ) => compilation;
        }
    }
}