// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.IO;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// Implementation of <see cref="FabricAspect{T}"/> for project-level fabrics.
    /// </summary>
    internal class ProjectFabricDriver : FabricDriver
    {
        private readonly int _depth;

        public ProjectFabricDriver( AspectProjectConfiguration configuration, IFabric fabric, Compilation runTimeCompilation ) :
            base( configuration, fabric, runTimeCompilation )
        {
            var depth = 0;

            // Compute the distance of the path to the root.
            for ( var path = this.OriginalPath; !string.IsNullOrEmpty( path ); path = Path.GetDirectoryName( path ) )
            {
                depth++;
            }

            this._depth = depth;
        }

        public override void Execute( IAspectBuilderInternal aspectBuilder, FabricTemplateClass fabricTemplateClass, FabricInstance fabricInstance )
        {
            var builder = new Builder( (ICompilation) aspectBuilder.Target, this.Configuration, aspectBuilder, fabricInstance );
            ((IProjectFabric) this.Fabric).AmendProject( builder );
        }

        public override FabricKind Kind => this.Fabric is ITransitiveProjectFabric ? FabricKind.Transitive : FabricKind.Compilation;

        protected override int CompareToCore( FabricDriver other )
        {
            if ( this.Fabric is ITransitiveProjectFabric )
            {
                // For transitive project fabrics, we first consider the depth of the dependency in the project graph.
                var thisReferenceDepth = CompilationReferenceGraph.GetInstance( this.Compilation ).GetDepth( this.FabricSymbol.ContainingAssembly );
                var otherReferenceDepth = CompilationReferenceGraph.GetInstance( other.Compilation ).GetDepth( other.FabricSymbol.ContainingAssembly );

                var referenceDepthComparison = thisReferenceDepth.Max.CompareTo( otherReferenceDepth.Max );

                if ( referenceDepthComparison != 0 )
                {
                    return referenceDepthComparison;
                }

                // Then we sort by assembly name, to make sure all fabrics of the same project run together.
                var nameComparison = string.Compare(
                    this.FabricSymbol.ContainingAssembly.Name,
                    other.FabricSymbol.ContainingAssembly.Name,
                    StringComparison.OrdinalIgnoreCase );

                if ( nameComparison != 0 )
                {
                    return nameComparison;
                }
            }

            var otherProjectFabricDriver = (ProjectFabricDriver) other;

            // Compare by depth from the root directory.
            var depthComparison = this._depth.CompareTo( otherProjectFabricDriver._depth );

            if ( depthComparison != 0 )
            {
                return depthComparison;
            }

            // Finally compare by type name, ignoring the namespace.
            return string.Compare( this.FabricSymbol.Name, other.FabricSymbol.Name, StringComparison.Ordinal );
        }

        public override IDeclaration GetTarget( CompilationModel compilation ) => compilation;

        public override FormattableString FormatPredecessor() => $"project fabric '{this.Fabric.GetType()}'";

        private class Builder : BaseBuilder<ICompilation>, IProjectAmender
        {
            public Builder(
                ICompilation compilation,
                AspectProjectConfiguration context,
                IAspectBuilderInternal aspectBuilder,
                FabricInstance fabricInstance ) : base(
                compilation,
                context,
                aspectBuilder,
                fabricInstance ) { }
        }
    }
}