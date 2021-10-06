// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// Implementation of <see cref="FabricAspect{T}"/> for type-level fabrics.
    /// </summary>
    internal class TypeFabricDriver : FabricDriver
    {
        public TypeFabricDriver( AspectProjectConfiguration configuration, IFabric fabric, Compilation runTimeCompilation ) : base(
            configuration,
            fabric,
            runTimeCompilation ) { }

        private ISymbol TargetSymbol => this.FabricSymbol.ContainingType;

        public override void Execute( IAspectBuilderInternal aspectBuilder, FabricTemplateClass templateClass )
        {
            // Type fabrics execute as aspects, called from FabricAspectClass.
            var templateInstance = new TemplateClassInstance( this.Fabric, templateClass );
            var builder = new Builder( (INamedType) aspectBuilder.Target, this.Configuration, aspectBuilder, templateInstance );
            ((ITypeFabric) this.Fabric).AmendType( builder );
        }

        public override FabricKind Kind => FabricKind.Type;

        public override IDeclaration GetTarget( CompilationModel compilation ) => compilation.Factory.GetNamedType( (INamedTypeSymbol) this.TargetSymbol );

        private class Builder : BaseBuilder<INamedType>, ITypeAmender
        {
            public Builder(
                INamedType namedType,
                AspectProjectConfiguration context,
                IAspectBuilderInternal aspectBuilder,
                TemplateClassInstance templateClassInstance ) : base( namedType, context, aspectBuilder )
            {
                this.Advices = aspectBuilder.AdviceFactory.WithTemplateClassInstance( templateClassInstance );
            }

            public INamedType Type => this.Target;

            public IAdviceFactory Advices { get; }
        }
    }
}