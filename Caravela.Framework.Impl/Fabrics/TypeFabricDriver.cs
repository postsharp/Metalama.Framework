// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// Implementation of <see cref="FabricAspect{T}"/> for type-level fabrics.
    /// </summary>
    internal class TypeFabricDriver : FabricDriver
    {
        public TypeFabricDriver( FabricManager fabricManager, Fabric fabric, Compilation runTimeCompilation ) : base(
            fabricManager,
            fabric,
            runTimeCompilation ) { }

        private ISymbol TargetSymbol => this.FabricSymbol.ContainingType;

        public void Execute( IAspectBuilderInternal aspectBuilder, FabricTemplateClass templateClass, FabricInstance fabricInstance )
        {
            // Type fabrics execute as aspects, called from FabricAspectClass.
            var templateInstance = new TemplateClassInstance( this.Fabric, templateClass );
            var builder = new Amender( (INamedType) aspectBuilder.Target, this.FabricManager, aspectBuilder, templateInstance, fabricInstance );
            ((TypeFabric) this.Fabric).AmendType( builder );
        }

        public override FabricKind Kind => FabricKind.Type;

        public IDeclaration GetTarget( CompilationModel compilation ) => compilation.Factory.GetNamedType( (INamedTypeSymbol) this.TargetSymbol );

        public override FormattableString FormatPredecessor() => $"type fabric on '{this.TargetSymbol}'";

        private class Amender : BaseAmender<INamedType>, ITypeAmender
        {
            private readonly IAspectBuilderInternal _aspectBuilder;

            public Amender(
                INamedType namedType,
                FabricManager fabricManager,
                IAspectBuilderInternal aspectBuilder,
                TemplateClassInstance templateClassInstance,
                FabricInstance fabricInstance ) : base( namedType.Compilation.Project, fabricManager, fabricInstance, fabricInstance.TargetDeclaration.As<INamedType>() )
            {
                this._aspectBuilder = aspectBuilder;
                this.Type = namedType;
                this.Advices = aspectBuilder.AdviceFactory.WithTemplateClassInstance( templateClassInstance );
            }

            public INamedType Type { get; }

            public IAdviceFactory Advices { get; }

            protected override void AddAspectSource( IAspectSource aspectSource ) => this._aspectBuilder.AddAspectSource( aspectSource );
        }
    }
}