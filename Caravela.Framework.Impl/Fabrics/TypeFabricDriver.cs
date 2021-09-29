// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Attribute = System.Attribute;

namespace Caravela.Framework.Impl.Fabrics
{
    internal class TypeFabricDriver : FabricDriver
    {
        public TypeFabricDriver( FabricContext context, IFabric fabric, Compilation runTimeCompilation ) : base( context, fabric, runTimeCompilation ) { }

        public override ISymbol TargetSymbol => this.FabricSymbol.ContainingType;

        public override void Execute( IAspectBuilderInternal aspectBuilder, FabricTemplateClass templateClass )
        {
            // Type fabrics execute as aspects, called from FabricAspectClass.
            var templateInstance = new TemplateClassInstance( this.Fabric, templateClass, aspectBuilder.Target );
            var builder = new Builder( (INamedType) aspectBuilder.Target, this.Context, aspectBuilder, templateInstance );
            ((ITypeFabric) this.Fabric).BuildType( builder );
        }

        public override FabricKind Kind => FabricKind.Type;

        public override IDeclaration GetTarget( CompilationModel compilation ) => compilation.Factory.GetNamedType( (INamedTypeSymbol) this.TargetSymbol );

        private class Builder : BaseBuilder<INamedType>, ITypeFabricBuilder
        {
            private readonly NamedTypeSelection _namedTypeSelection;

            public Builder(
                INamedType namedType,
                FabricContext context,
                IAspectBuilderInternal aspectBuilder,
                TemplateClassInstance templateClassInstance ) : base( namedType, context, aspectBuilder )
            {
                this._namedTypeSelection = new NamedTypeSelection(
                    this.RegisterAspectSource,
                    compilation => new[] { compilation.Factory.GetDeclaration( namedType ) },
                    context );

                this.Advices = aspectBuilder.AdviceFactory.WithTemplateClassInstance( templateClassInstance );
            }

            public void AddAspect<TAspect>( Func<INamedType, Expression<Func<TAspect>>> createAspect )
                where TAspect : Attribute, IAspect<INamedType>
                => this._namedTypeSelection.AddAspect( createAspect );

            public void AddAspect<TAspect>( Func<INamedType, TAspect> createAspect )
                where TAspect : Attribute, IAspect<INamedType>
                => this._namedTypeSelection.AddAspect( createAspect );

            public void AddAspect<TAspect>()
                where TAspect : Attribute, IAspect<INamedType>, new()
                => this._namedTypeSelection.AddAspect<TAspect>();

            [Obsolete( "Not implemented." )]
            public void RequireAspect<TTarget, TAspect>( TTarget target )
                where TTarget : class, IDeclaration
                where TAspect : IAspect<TTarget>, new()
                => this._namedTypeSelection.RequireAspect<TTarget, TAspect>( target );

            [Obsolete( "Not implemented." )]
            public void AddAnnotation<TAspect, TAnnotation>( Func<INamedType, TAnnotation> getAnnotation )
                where TAspect : IAspect
                where TAnnotation : IAnnotation<INamedType, TAspect>, IEligible<INamedType>
                => this._namedTypeSelection.AddAnnotation<TAspect, TAnnotation>( getAnnotation );

            public IDeclarationSelection<IMethod> WithMethods( Func<INamedType, IEnumerable<IMethod>> selector )
                => this._namedTypeSelection.WithMethods( selector );

            public IDeclarationSelection<IProperty> WithProperties( Func<INamedType, IEnumerable<IProperty>> selector )
                => this._namedTypeSelection.WithProperties( selector );

            public IDeclarationSelection<IEvent> WithEvents( Func<INamedType, IEnumerable<IEvent>> selector )
                => this._namedTypeSelection.WithEvents( selector );

            public IDeclarationSelection<IField> WithFields( Func<INamedType, IEnumerable<IField>> selector )
                => this._namedTypeSelection.WithFields( selector );

            public IDeclarationSelection<IConstructor> WithConstructors( Func<INamedType, IEnumerable<IConstructor>> selector )
                => this._namedTypeSelection.WithConstructors( selector );

            public IAdviceFactory Advices { get; }
        }
    }
}