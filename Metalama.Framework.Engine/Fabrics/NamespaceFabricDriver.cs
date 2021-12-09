// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Fabrics
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
            this._targetNamespace = Ref.FromSymbol<INamespace>( this.FabricSymbol.ContainingNamespace, runTimeCompilation );
        }

        private ISymbol TargetSymbol => this.FabricSymbol.ContainingNamespace;

        public override FabricKind Kind => FabricKind.Namespace;

        public IDeclaration GetTarget( CompilationModel compilation ) => compilation.Factory.GetNamespace( (INamespaceSymbol) this.TargetSymbol );

        public override FormattableString FormatPredecessor() => $"namespace fabric '{this.Fabric.GetType()}' on '{this.TargetSymbol}'";

        public override bool TryExecute( IProject project, IDiagnosticAdder diagnosticAdder, [NotNullWhen( true )] out StaticFabricResult? result )
        {
            var amender = new Amender(
                project,
                this.FabricManager,
                new FabricInstance( this, this._targetNamespace.As<IDeclaration>() ) );

            var executionContext = new UserCodeExecutionContext(
                this.FabricManager.ServiceProvider,
                diagnosticAdder,
                UserCodeMemberInfo.FromDelegate( new Action<INamespaceAmender>( ((NamespaceFabric) this.Fabric).AmendNamespace ) ) );

            if ( !this.FabricManager.UserCodeInvoker.TryInvoke( () => ((NamespaceFabric) this.Fabric).AmendNamespace( amender ), executionContext ) )
            {
                result = null;

                return false;
            }

            // TODO: Exception handling.

            result = amender.ToResult();

            return true;
        }

        private class Amender : StaticAmender<INamespace>, INamespaceAmender
        {
            public Amender(
                IProject project,
                FabricManager fabricManager,
                FabricInstance fabricInstance ) : base(
                project,
                fabricManager,
                fabricInstance,
                fabricInstance.TargetDeclaration.As<INamespace>() ) { }
        }
    }
}