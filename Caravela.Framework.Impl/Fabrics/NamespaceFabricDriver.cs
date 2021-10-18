// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// Implementation of <see cref="FabricAspect{T}"/> for namespace-level fabrics.
    /// </summary>
    internal class NamespaceFabricDriver : FabricDriver
    {
        public NamespaceFabricDriver( AspectProjectConfiguration configuration, IFabric fabric, Compilation runTimeCompilation ) :
            base( configuration, fabric, runTimeCompilation ) { }

        private ISymbol TargetSymbol => this.FabricSymbol.ContainingNamespace;

        public override void Execute( IAspectBuilderInternal aspectBuilder, FabricTemplateClass fabricTemplateClass )
        {
            var builder = new Builder( this, (INamespace) aspectBuilder.Target, this.Configuration, aspectBuilder );
            ((INamespaceFabric) this.Fabric).AmendNamespace( builder );
        }

        public override FabricKind Kind => FabricKind.Namespace;

        public override IDeclaration GetTarget( CompilationModel compilation ) => compilation.Factory.GetNamespace( (INamespaceSymbol) this.TargetSymbol );

        public override FormattableString FormatPredecessor() => $"namespace fabric '{this.Fabric.GetType()}' on '{this.TargetSymbol}'";
        
        private class Builder : BaseBuilder<INamespace>, INamespaceAmender
        {
            public Builder( FabricDriver parent, INamespace ns, AspectProjectConfiguration context, IAspectBuilderInternal aspectBuilder ) : base(
                parent,
                ns,
                context,
                aspectBuilder ) { }

            public INamespace Namespace => this.Target;
        }
    }
}