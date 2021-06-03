// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Aspects.AdvisedCode;
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
        private readonly IAdviceFieldOrProperty? _fieldOrProperty;
        private readonly IAdviceMethod? _method;
        private readonly IAdviceEvent? _event;
        private readonly INamedType? _type;
        private readonly MetaApiProperties _common;

        private Exception CreateInvalidOperationException( string memberName, string? description = null )
            => TemplatingDiagnosticDescriptors.MemberMemberNotAvailable.CreateException(
                (this._common.TemplateSymbol, "meta." + memberName, this.Declaration, this.Declaration.DeclarationKind, description ?? "I" + memberName) );

        public IConstructor Constructor => throw new NotImplementedException();

        public IMethodBase MethodBase => this._method ?? throw this.CreateInvalidOperationException( nameof(this.MethodBase) );

        public IAdviceField Field => this._fieldOrProperty as IAdviceField ?? throw this.CreateInvalidOperationException( nameof(this.Field) );

        public IAdviceFieldOrProperty FieldOrProperty => this._fieldOrProperty ?? throw this.CreateInvalidOperationException( nameof(this.FieldOrProperty) );

        public IDeclaration Declaration { get; }

        public IMemberOrNamedType Member => this.Declaration as IMemberOrNamedType ?? throw this.CreateInvalidOperationException( nameof(this.Member) );

        public IAdviceMethod Method => this._method ?? throw this.CreateInvalidOperationException( nameof(this.Method) );

        public IAdviceProperty Property => this._fieldOrProperty as IAdviceProperty ?? throw this.CreateInvalidOperationException( nameof(this.Property) );

        public IAdviceEvent Event => this._event ?? throw this.CreateInvalidOperationException( nameof(this.Event) );

        public IAdviceParameterList Parameters => this._method?.Parameters ?? throw this.CreateInvalidOperationException( nameof(this.Parameters) );

        public INamedType Type => this._type ?? throw this.CreateInvalidOperationException( nameof(this.Type) );

        public ICompilation Compilation { get; }

        private ThisInstanceDynamicReceiver GetThisOrBase( string expressionName, LinkerAnnotation linkerAnnotation )
        {
            return this._type switch
            {
                null => throw this.CreateInvalidOperationException( expressionName ),
                { IsStatic: false } when this.Declaration is IMemberOrNamedType { IsStatic: false }
                    => new ThisInstanceDynamicReceiver(
                        this.Type,
                        linkerAnnotation ),

                _ => throw TemplatingDiagnosticDescriptors.CannotUseThisInStaticContext.CreateException(
                    (this._common.TemplateSymbol, expressionName, this.Declaration, this.Declaration.DeclarationKind) )
            };
        }

        public dynamic This => this.GetThisOrBase( "meta.This", new LinkerAnnotation( this._common.AspectLayerId, LinkingOrder.Default ) );

        public dynamic Base => this.GetThisOrBase( "meta.Base", new LinkerAnnotation( this._common.AspectLayerId, LinkingOrder.Base ) );

        public dynamic ThisStatic => new ThisTypeDynamicReceiver( this.Type, new LinkerAnnotation( this._common.AspectLayerId, LinkingOrder.Default ) );

        public dynamic BaseStatic => new ThisTypeDynamicReceiver( this.Type, new LinkerAnnotation( this._common.AspectLayerId, LinkingOrder.Base ) );

        public IReadOnlyDictionary<string, object?> Tags => this._common.Tags;

        IDiagnosticSink IMetaApi.Diagnostics => this._common.Diagnostics;

        public UserDiagnosticSink Diagnostics => this._common.Diagnostics;

        private MetaApi( IDeclaration declaration, MetaApiProperties common )
        {
            this.Declaration = declaration;
            this.Compilation = declaration.Compilation;
            this._common = common;
        }

        private MetaApi( IMethod method, MetaApiProperties common ) : this(
            (IDeclaration) method,
            common )
        {
            this._method = new AdviceMethod( method );
            this._type = method.DeclaringType;
        }

        private MetaApi( IFieldOrProperty fieldOrProperty, IMethod accessor, MetaApiProperties common ) : this( accessor, common )
        {
            this._method = new AdviceMethod( accessor );

            this._fieldOrProperty = fieldOrProperty switch
            {
                IField field => new AdviceField( field ),
                IProperty property => new AdviceProperty( property ),
                _ => throw new AssertionFailedException()
            };

            this._type = fieldOrProperty.DeclaringType;
        }

        private MetaApi( IEvent @event, IMethod accessor, MetaApiProperties common ) : this( accessor, common )
        {
            this._event = new AdviceEvent( @event );
            this._type = @event.DeclaringType;
            this._method = new AdviceMethod( accessor );
        }

        public static MetaApi ForMethod( IMethodBase methodBase, MetaApiProperties common ) => new( (IMethod) methodBase, common );

        public static MetaApi ForFieldOrProperty( IFieldOrProperty fieldOrProperty, IMethod accessor, MetaApiProperties common )
            => new( fieldOrProperty, accessor, common );

        public static MetaApi ForEvent( IEvent @event, IMethod accessor, MetaApiProperties common ) => new( @event, accessor, common );
    }
}