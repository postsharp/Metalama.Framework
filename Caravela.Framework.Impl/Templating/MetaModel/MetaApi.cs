// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Linking;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// The implementation of <see cref="IMetaApi"/>.
    /// </summary>
    internal class MetaApi : IMetaApi
    {
        private readonly IFieldOrProperty? _fieldOrProperty;
        private readonly IMethodBase? _methodBase;
        private readonly IEvent? _event;
        private readonly INamedType? _type;
        private readonly AdviceParameterList? _parameters;
        private readonly MetaApiProperties _common;

        private Exception CreateInvalidOperationException( string memberName, string? description = null )
            => TemplatingDiagnosticDescriptors.MemberMemberNotAvailable.CreateException(
                (this._common.TemplateSymbol, "meta." + memberName, this.Declaration, this.Declaration.ElementKind, description ?? "I" + memberName) );

        public IConstructor Constructor => this._methodBase as IConstructor ?? throw this.CreateInvalidOperationException( nameof(this.Constructor) );

        public IMethodBase MethodBase => this._methodBase ?? throw this.CreateInvalidOperationException( nameof(this.MethodBase) );

        public IField Field => this._fieldOrProperty as IField ?? throw this.CreateInvalidOperationException( nameof(this.Field) );

        public IFieldOrProperty FieldOrProperty => this._fieldOrProperty ?? throw this.CreateInvalidOperationException( nameof(this.FieldOrProperty) );

        public IDeclaration Declaration { get; }

        public IMember Member => this.Declaration as IMember ?? throw this.CreateInvalidOperationException( nameof(this.Member) );

        public IMethod Method => this._methodBase as IMethod ?? throw this.CreateInvalidOperationException( nameof(this.Method) );

        public IProperty Property => this._fieldOrProperty as IProperty ?? throw this.CreateInvalidOperationException( nameof(this.Property) );

        public IEvent Event => this._event ?? throw this.CreateInvalidOperationException( nameof(this.Event) );

        public IAdviceParameterList Parameters
            => this._parameters ?? throw this.CreateInvalidOperationException( nameof(this.Parameters), "list of parameters" );

        public INamedType Type => this._type ?? throw this.CreateInvalidOperationException( nameof(this.Type) );

        public ICompilation Compilation { get; }

        private ThisInstanceDynamicReceiver GetThisOrBase( string expressionName, LinkerAnnotation linkerAnnotation )
            => this._type is { IsStatic: false } && this.Declaration is IMember { IsStatic: false }
                ? new ThisInstanceDynamicReceiver( this.Type, linkerAnnotation )
                : throw TemplatingDiagnosticDescriptors.CannotUseThisInStaticContext.CreateException(
                    (this._common.TemplateSymbol, expressionName, this.Declaration, this.Declaration.ElementKind) );

        public dynamic This => this.GetThisOrBase( "meta.This", new LinkerAnnotation( this._common.AspectLayerId, LinkerAnnotationOrder.Default ) );

        public dynamic Base => this.GetThisOrBase( "meta.Base", new LinkerAnnotation( this._common.AspectLayerId, LinkerAnnotationOrder.Original ) );

        public dynamic ThisStatic
            => new ThisTypeDynamicReceiver( this.Type, new LinkerAnnotation( this._common.AspectLayerId, LinkerAnnotationOrder.Default ) );

        public dynamic BaseStatic
            => new ThisTypeDynamicReceiver( this.Type, new LinkerAnnotation( this._common.AspectLayerId, LinkerAnnotationOrder.Original ) );

        public IReadOnlyDictionary<string, object?> Tags => this._common.Tags;

        public IDiagnosticSink Diagnostics => this._common.Diagnostics;

        private MetaApi( IDeclaration declaration, MetaApiProperties common )
        {
            this.Declaration = declaration;
            this.Compilation = declaration.Compilation;
            this._common = common;
        }

        public MetaApi( IMethodBase methodBase, MetaApiProperties common ) : this(
            (IDeclaration) methodBase,
            common )
        {
            // TODO: if the method is a getter/setter/adder/remover, set the event or property.

            this._methodBase = methodBase;
            this._type = methodBase.DeclaringType;
            this._parameters = new AdviceParameterList( methodBase );
        }

        public MetaApi( IFieldOrProperty fieldOrProperty, MetaApiProperties common ) : this(
            (IDeclaration) fieldOrProperty,
            common )
        {
            // TODO: if the method is a getter/setter/adder/remover, set the event or property.

            this._fieldOrProperty = fieldOrProperty;
            this._type = fieldOrProperty.DeclaringType;

            // TODO: indexer parameters
        }

        public MetaApi( IEvent @event, MetaApiProperties common ) : this( (IDeclaration) @event, common )
        {
            // TODO: if the method is a getter/setter/adder/remover, set the event or property.

            this._event = @event;
            this._type = @event.DeclaringType;

            // TODO: event parameters
        }
    }
}