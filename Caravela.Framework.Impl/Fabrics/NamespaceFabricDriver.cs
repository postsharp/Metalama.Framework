// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// Implementation of <see cref="FabricAspect{T}"/> for namespace-level fabrics.
    /// </summary>
    internal class NamespaceFabricDriver : StaticFabricDriver
    {
        private readonly Ref<INamespace> _targetNamespace;

        public NamespaceFabricDriver( FabricManager fabricManager, Fabric fabric, Compilation runTimeCompilation ) :
            base( fabricManager, fabric, runTimeCompilation )
        {
            this._targetNamespace = Ref.FromSymbol<INamespace>( this.FabricSymbol.ContainingNamespace );
        }

        private ISymbol TargetSymbol => this.FabricSymbol.ContainingNamespace;

        public override FabricKind Kind => FabricKind.Namespace;

        public IDeclaration GetTarget( CompilationModel compilation ) => compilation.Factory.GetNamespace( (INamespaceSymbol) this.TargetSymbol );

        public override FormattableString FormatPredecessor() => $"namespace fabric '{this.Fabric.GetType()}' on '{this.TargetSymbol}'";

        public override StaticFabricResult Execute( IProject project )
        {
            var amender = new Amender(
                this._targetNamespace,
                project,
                this.FabricManager,
                new FabricInstance( this, this._targetNamespace.As<IDeclaration>() ) );

            ((NamespaceFabric) this.Fabric).AmendNamespace( amender );

            return amender.ToResult();
        }

        private class Amender : StaticAmender<INamespace>, INamespaceAmender
        {
            public Amender(
                ISdkRef<INamespace> ns,
                IProject project,
                FabricManager fabricManager,
                FabricInstance fabricInstance ) : base(
                project,
                fabricManager,
                fabricInstance,
                fabricInstance.TargetDeclaration.As<INamespace>()) { }
        }
    }
}