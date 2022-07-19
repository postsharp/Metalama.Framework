// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Project;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    /// <summary>
    /// The implementation of <see cref="IMetaApi"/>.
    /// </summary>
    internal class MetaApi : SyntaxBuilderImpl, IMetaApi, IMetaTarget
    {
        private readonly IAdvisedFieldOrProperty? _fieldOrProperty;
        private readonly IAdvisedMethod? _method;
        private readonly IAdvisedConstructor? _constructor;
        private readonly IAdvisedEvent? _event;
        private readonly INamedType? _type;
        private readonly MetaApiProperties _common;
        private readonly IAdvisedParameter? _parameter;
        private readonly ContractDirection? _contractDirection;

        private Exception CreateInvalidOperationException( string memberName, string? description = null )
            => TemplatingDiagnosticDescriptors.MemberMemberNotAvailable.CreateException(
                (this._common.Template.Declaration!, "meta." + memberName, this.Declaration, this.Declaration.DeclarationKind,
                 description ?? "I" + memberName) );

        public IConstructor Constructor => this._constructor ?? throw this.CreateInvalidOperationException( nameof(this.Constructor) );

        public IMethodBase MethodBase => (IMethodBase?) this._method ?? throw this.CreateInvalidOperationException( nameof(this.MethodBase) );

        public IAdvisedField Field => this._fieldOrProperty as IAdvisedField ?? throw this.CreateInvalidOperationException( nameof(this.Field) );

        public IAdvisedFieldOrProperty FieldOrProperty => this._fieldOrProperty ?? throw this.CreateInvalidOperationException( nameof(this.FieldOrProperty) );

        public IDeclaration Declaration { get; }

        public IMember Member => this.Declaration as IMember ?? throw this.CreateInvalidOperationException( nameof(this.Member) );

        public IAdvisedMethod Method => this._method ?? throw this.CreateInvalidOperationException( nameof(this.Method) );

        public IAdvisedProperty Property => this._fieldOrProperty as IAdvisedProperty ?? throw this.CreateInvalidOperationException( nameof(this.Property) );

        public IAdvisedEvent Event => this._event ?? throw this.CreateInvalidOperationException( nameof(this.Event) );

        public IAdvisedParameterList Parameters => this._method?.Parameters ?? throw this.CreateInvalidOperationException( nameof(this.Parameters) );

        public IAdvisedParameter Parameter => this._parameter ?? throw this.CreateInvalidOperationException( nameof(this.Parameter) );

        public IIndexer Indexer => this.Member as IIndexer ?? throw this.CreateInvalidOperationException( nameof(this.Indexer) );

        public INamedType Type => this._type ?? throw this.CreateInvalidOperationException( nameof(this.Type) );

        public ContractDirection ContractDirection => this._contractDirection ?? throw this.CreateInvalidOperationException( nameof(this.ContractDirection) );

        private ThisInstanceUserReceiver GetThisOrBase( string expressionName, AspectReferenceSpecification linkerAnnotation )
            => (this._common.Staticity, this._type, this.Declaration) switch
            {
                (_, null, _) => throw this.CreateInvalidOperationException( expressionName ),

                (MetaApiStaticity.AlwaysInstance, _, _)
                    => new ThisInstanceUserReceiver(
                        this.Type,
                        linkerAnnotation ),

                (MetaApiStaticity.AlwaysStatic, _, _)
                    => throw TemplatingDiagnosticDescriptors.CannotUseThisInStaticContext.CreateException(
                        (this._common.Template.Declaration!, expressionName, this.Declaration, this.Declaration.DeclarationKind) ),

                (MetaApiStaticity.Default, { IsStatic: false }, IMemberOrNamedType { IsStatic: false })
                    => new ThisInstanceUserReceiver(
                        this.Type,
                        linkerAnnotation ),

                _ => throw TemplatingDiagnosticDescriptors.CannotUseThisInStaticContext.CreateException(
                    (this._common.Template.Declaration!, expressionName, this.Declaration, this.Declaration.DeclarationKind) )
            };

        public IMetaTarget Target => this;

        IAspectInstance IMetaApi.AspectInstance
            => this._common.AspectInstance ?? throw new InvalidOperationException( "IAspectInstance has not been provided." );

        public IAspectInstanceInternal? AspectInstance => this._common.AspectInstance;

        public object This => this.GetThisOrBase( "meta.This", new AspectReferenceSpecification( this._common.AspectLayerId, AspectReferenceOrder.Final ) );

        public object Base => this.GetThisOrBase( "meta.Base", new AspectReferenceSpecification( this._common.AspectLayerId, AspectReferenceOrder.Base ) );

        public object ThisStatic
            => new ThisTypeUserReceiver( this.Type, new AspectReferenceSpecification( this._common.AspectLayerId, AspectReferenceOrder.Final ) );

        public object BaseStatic
            => new ThisTypeUserReceiver( this.Type, new AspectReferenceSpecification( this._common.AspectLayerId, AspectReferenceOrder.Base ) );

        public IObjectReader Tags => this._common.Tags;

        IDiagnosticSink IMetaApi.Diagnostics => this._common.Diagnostics;

        [ExcludeFromCodeCoverage]
        public void DebugBreak()
        {
            var trustOptions = this._common.ServiceProvider.GetRequiredService<IProjectOptions>();

            if ( !trustOptions.IsUserCodeTrusted )
            {
                return;
            }

            if ( Debugger.IsAttached )
            {
                Debugger.Break();
            }
        }

        public IExecutionScenario ExecutionScenario => this._common.PipelineDescription.ExecutionScenario;

        public UserDiagnosticSink Diagnostics => this._common.Diagnostics;

        private MetaApi( IDeclaration declaration, MetaApiProperties common ) : base( declaration.GetCompilationModel(), common.SyntaxGenerationContext )
        {
            this.Declaration = declaration;
            this._common = common;
        }

        private MetaApi( IMethod method, MetaApiProperties common ) : this(
            (IDeclaration) method,
            common )
        {
            this._method = new AdvisedMethod( method );
            this._type = method.DeclaringType;
        }

        private MetaApi( IConstructor constructor, MetaApiProperties common ) : this( (IDeclaration) constructor, common )
        {
            this._constructor = new AdvisedConstructor( constructor );
            this._type = constructor.DeclaringType;
        }

        private MetaApi( IParameter parameter, MetaApiProperties common, ContractDirection? contractDirection ) : this( parameter, common )
        {
            switch ( parameter.DeclaringMember )
            {
                case IConstructor constructor:
                    this._constructor = new AdvisedConstructor( constructor );

                    break;

                case IMethod method:
                    this._method = new AdvisedMethod( method );

                    break;
            }

            this._type = parameter.DeclaringMember.DeclaringType;
            this._parameter = new AdvisedParameter( parameter );
            this._contractDirection = contractDirection;
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

        private MetaApi( IFieldOrProperty fieldOrProperty, MetaApiProperties common, ContractDirection? contractDirection ) : this( fieldOrProperty, common )
        {
            this._fieldOrProperty = fieldOrProperty switch
            {
                IField field => new AdvisedField( field ),
                IProperty property => new AdvisedProperty( property ),
                _ => throw new AssertionFailedException()
            };

            this._type = fieldOrProperty.DeclaringType;
            this._contractDirection = contractDirection;
        }

        private MetaApi( IEvent eventField, MetaApiProperties common ) : this( (IDeclaration) eventField, common )
        {
            this._event = new AdvisedEvent( eventField );
            this._type = eventField.DeclaringType;
        }

        private MetaApi( IEvent @event, IMethod accessor, MetaApiProperties common ) : this( accessor, common )
        {
            this._event = new AdvisedEvent( @event );
            this._type = @event.DeclaringType;
            this._method = new AdvisedMethod( accessor );
        }

        private MetaApi( INamedType type, MetaApiProperties common ) : this( (IDeclaration) type, common )
        {
            this._type = type;
        }

        public static MetaApi ForDeclaration( IDeclaration declaration, MetaApiProperties common, ContractDirection? contractDirection = null )
            => declaration switch
            {
                INamedType type => new MetaApi( type, common ),
                IMethod method => new MetaApi( method, common ),
                IFieldOrProperty fieldOrProperty => new MetaApi( fieldOrProperty, common, contractDirection ),
                IEvent @event => new MetaApi( @event, common ),
                IConstructor constructor => new MetaApi( constructor, common ),
                IParameter parameter => new MetaApi( parameter, common, contractDirection ),
                _ => throw new AssertionFailedException()
            };

        public static MetaApi ForConstructor( IConstructor constructor, MetaApiProperties common ) => new( common.Translate( constructor ), common );

        public static MetaApi ForMethod( IMethod method, MetaApiProperties common ) => new( common.Translate( method ), common );

        public static MetaApi ForFieldOrProperty( IFieldOrProperty fieldOrProperty, IMethod accessor, MetaApiProperties common )
            => new( common.Translate( fieldOrProperty ), common.Translate( accessor ), common );

        public static MetaApi ForInitializer( IMember initializedDeclaration, MetaApiProperties common )
            => initializedDeclaration switch
            {
                IFieldOrProperty fieldOrProperty => new MetaApi( common.Translate( fieldOrProperty ), common ),
                IEvent eventField => new MetaApi( common.Translate( eventField ), common ),
                _ => throw new AssertionFailedException()
            };

        public static MetaApi ForEvent( IEvent @event, IMethod accessor, MetaApiProperties common )
            => new( common.Translate( @event ), common.Translate( accessor ), common );
    }
}