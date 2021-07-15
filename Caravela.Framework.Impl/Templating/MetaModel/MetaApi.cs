// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Advised;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// The implementation of <see cref="IMetaApi"/>.
    /// </summary>
    internal class MetaApi : IMetaApi
    {
        private readonly IAdvisedFieldOrProperty? _fieldOrProperty;
        private readonly IAdvisedMethod? _method;
        private readonly IAdvisedEvent? _event;
        private readonly INamedType? _type;
        private readonly MetaApiProperties _common;

        private Exception CreateInvalidOperationException( string memberName, string? description = null )
            => TemplatingDiagnosticDescriptors.MemberMemberNotAvailable.CreateException(
                (this._common.TemplateSymbol, "meta." + memberName, this.Declaration, this.Declaration.DeclarationKind, description ?? "I" + memberName) );

        public IConstructor Constructor => throw new NotImplementedException();

        public IMethodBase MethodBase => this._method ?? throw this.CreateInvalidOperationException( nameof(this.MethodBase) );

        public IAdvisedField Field => this._fieldOrProperty as IAdvisedField ?? throw this.CreateInvalidOperationException( nameof(this.Field) );

        public IAdvisedFieldOrProperty FieldOrProperty => this._fieldOrProperty ?? throw this.CreateInvalidOperationException( nameof(this.FieldOrProperty) );

        public IDeclaration Declaration { get; }

        public IMember Member => this.Declaration as IMember ?? throw this.CreateInvalidOperationException( nameof(this.Member) );

        public IAdvisedMethod Method => this._method ?? throw this.CreateInvalidOperationException( nameof(this.Method) );

        public IAdvisedProperty Property => this._fieldOrProperty as IAdvisedProperty ?? throw this.CreateInvalidOperationException( nameof(this.Property) );

        public IAdvisedEvent Event => this._event ?? throw this.CreateInvalidOperationException( nameof(this.Event) );

        public IAdvisedParameterList Parameters => this._method?.Parameters ?? throw this.CreateInvalidOperationException( nameof(this.Parameters) );

        public INamedType Type => this._type ?? throw this.CreateInvalidOperationException( nameof(this.Type) );

        public ICompilation Compilation { get; }

        private ThisInstanceDynamicReceiver GetThisOrBase( string expressionName, AspectReferenceSpecification linkerAnnotation )
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

        public object This => this.GetThisOrBase( "meta.This", new AspectReferenceSpecification( this._common.AspectLayerId, AspectReferenceOrder.Final ) );

        public object Base => this.GetThisOrBase( "meta.Base", new AspectReferenceSpecification( this._common.AspectLayerId, AspectReferenceOrder.Base ) );

        public object ThisStatic
            => new ThisTypeDynamicReceiver( this.Type, new AspectReferenceSpecification( this._common.AspectLayerId, AspectReferenceOrder.Final ) );

        public object BaseStatic
            => new ThisTypeDynamicReceiver( this.Type, new AspectReferenceSpecification( this._common.AspectLayerId, AspectReferenceOrder.Base ) );

        public object? Proceed() => this._common.ProceedExpression;

        public IReadOnlyDictionary<string, object?> Tags => this._common.Tags;

        IDiagnosticSink IMetaApi.Diagnostics => this._common.Diagnostics;

        public void DebugBreak()
        {
            if ( Debugger.IsAttached )
            {
                Debugger.Break();
            }
        }

        public AspectExecutionScenario ExecutionScenario => this._common.PipelineDescription.ExecutionScenario;

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
            this._method = new AdvisedMethod( method );
            this._type = method.DeclaringType;
        }

        private MetaApi( IFieldOrProperty fieldOrProperty, IMethod accessor, MetaApiProperties common ) : this( accessor, common )
        {
            this._method = new AdvisedMethod( accessor );

            this._fieldOrProperty = fieldOrProperty switch
            {
                IField field => new AdvisedField( field ),
                IProperty property => new AdvisedProperty( property ),
                _ => throw new AssertionFailedException()
            };

            this._type = fieldOrProperty.DeclaringType;
        }

        private MetaApi( IEvent @event, IMethod accessor, MetaApiProperties common ) : this( accessor, common )
        {
            this._event = new AdvisedEvent( @event );
            this._type = @event.DeclaringType;
            this._method = new AdvisedMethod( accessor );
        }

        public static MetaApi ForMethod( IMethodBase methodBase, MetaApiProperties common ) => new( (IMethod) methodBase, common );

        public static MetaApi ForFieldOrProperty( IFieldOrProperty fieldOrProperty, IMethod accessor, MetaApiProperties common )
            => new( fieldOrProperty, accessor, common );

        public static MetaApi ForEvent( IEvent @event, IMethod accessor, MetaApiProperties common ) => new( @event, accessor, common );
    }
}