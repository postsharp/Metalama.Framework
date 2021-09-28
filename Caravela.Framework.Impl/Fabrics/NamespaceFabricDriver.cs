// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Fabrics
{
    internal class NamespaceFabricDriver : FabricDriver
    {
        public NamespaceFabricDriver( FabricContext context, IFabric fabric, Compilation runTimeCompilation ) :
            base( context, fabric, runTimeCompilation ) { }

        public override ISymbol TargetSymbol => this.FabricSymbol.ContainingNamespace;

        public override void Execute( IAspectBuilderInternal aspectBuilder )
        {
            var builder = new Builder( (INamespace) aspectBuilder.Target, this.Context, aspectBuilder );
            ((INamespaceFabric) this.Fabric).BuildFabric( builder );
        }

        public override FabricKind Kind => FabricKind.Namespace;

        public override IDeclaration GetTarget( CompilationModel compilation ) => compilation.Factory.GetNamespace( (INamespaceSymbol) this.TargetSymbol );

        private class Builder : BaseBuilder<INamespace>, INamespaceFabricBuilder
        {
            public Builder( INamespace ns, FabricContext context, IAspectBuilderInternal aspectBuilder ) : base( ns, context, aspectBuilder ) { }
        }
    }
}