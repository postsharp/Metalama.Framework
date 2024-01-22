﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Templating.Expressions;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    /// <summary>
    /// The implementation of <see cref="IMetaApi"/>.
    /// </summary>
    internal sealed class MetaApi : SyntaxBuilderImpl, IMetaApi, IMetaTarget, IDiagnosticSource
    {
        private readonly IFieldOrPropertyOrIndexer? _fieldOrPropertyOrIndexer;
        private readonly IMethod? _method;
        private readonly IConstructor? _constructor;
        private readonly IEvent? _event;
        private readonly INamedType? _type;
        private readonly MetaApiProperties _common;
        private readonly IParameter? _parameter;
        private readonly ContractDirection? _contractDirection;

        private Exception CreateInvalidOperationException( string memberName, string? description = null )
        {
            string? alternativeSuggestion = null;

            if ( memberName is nameof(this.Property) or nameof(this.Field) or nameof(this.FieldOrProperty) && this._fieldOrPropertyOrIndexer != null )
            {
                var alternativeMemberName = this._fieldOrPropertyOrIndexer is IFieldOrProperty
                    ? nameof(this.FieldOrProperty)
                    : nameof(this.FieldOrPropertyOrIndexer);

                alternativeSuggestion = $" Consider using meta.Target.{alternativeMemberName} instead.";
            }

            return TemplatingDiagnosticDescriptors.MetaMemberNotAvailable.CreateException(
                (this._common.Template.Declaration, "meta.Target." + memberName, this.Declaration, this.Declaration.DeclarationKind,
                 description ?? "I" + memberName, alternativeSuggestion) );
        }

        public IConstructor Constructor => this._constructor ?? throw this.CreateInvalidOperationException( nameof(this.Constructor) );

        public IMethodBase MethodBase => (IMethodBase?) this._method ?? throw this.CreateInvalidOperationException( nameof(this.MethodBase) );

        public IField Field => this._fieldOrPropertyOrIndexer as IField ?? throw this.CreateInvalidOperationException( nameof(this.Field) );

        public IFieldOrProperty FieldOrProperty
            => this._fieldOrPropertyOrIndexer as IFieldOrProperty ?? throw this.CreateInvalidOperationException( nameof(this.FieldOrProperty) );

        public IFieldOrPropertyOrIndexer FieldOrPropertyOrIndexer
            => this._fieldOrPropertyOrIndexer ?? throw this.CreateInvalidOperationException( nameof(this.FieldOrPropertyOrIndexer) );

        public IDeclaration Declaration { get; }

        public IMember Member => this.Declaration as IMember ?? throw this.CreateInvalidOperationException( nameof(this.Member) );

        public IMethod Method => this._method ?? throw this.CreateInvalidOperationException( nameof(this.Method) );

        public IProperty Property => this._fieldOrPropertyOrIndexer as IProperty ?? throw this.CreateInvalidOperationException( nameof(this.Property) );

        public IEvent Event => this._event ?? throw this.CreateInvalidOperationException( nameof(this.Event) );

        public IParameterList Parameters => this._method?.Parameters ?? throw this.CreateInvalidOperationException( nameof(this.Parameters), nameof(IMethod) );

        public IParameter Parameter => this._parameter ?? throw this.CreateInvalidOperationException( nameof(this.Parameter) );

        public IIndexer Indexer => this._fieldOrPropertyOrIndexer as IIndexer ?? throw this.CreateInvalidOperationException( nameof(this.Indexer) );

        public INamedType Type => this._type ?? throw this.CreateInvalidOperationException( nameof(this.Type), nameof(INamedType) );

        public ContractDirection ContractDirection => this._contractDirection ?? throw this.CreateInvalidOperationException( nameof(this.ContractDirection), nameof(Framework.Aspects.ContractDirection) );

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
                        (this._common.Template.Declaration, expressionName, this.Declaration, this.Declaration.DeclarationKind) ),

                (MetaApiStaticity.Default, { IsStatic: false }, IMemberOrNamedType { IsStatic: false })
                    => new ThisInstanceUserReceiver(
                        this.Type,
                        linkerAnnotation ),

                _ => throw TemplatingDiagnosticDescriptors.CannotUseThisInStaticContext.CreateException(
                    (this._common.Template.Declaration, expressionName, this.Declaration, this.Declaration.DeclarationKind) )
            };

        public IMetaTarget Target => this;

        IAspectInstance IMetaApi.AspectInstance
            => this._common.AspectInstance ?? throw new InvalidOperationException( "IAspectInstance has not been provided." );

        public IAspectInstanceInternal? AspectInstance => this._common.AspectInstance;

        public object This => this.GetThisOrBase( "meta.This", new AspectReferenceSpecification( null, this._common.AspectLayerId, AspectReferenceOrder.Final ) );

        public object Base => this.GetThisOrBase( "meta.Base", new AspectReferenceSpecification( null, this._common.AspectLayerId, AspectReferenceOrder.Base ) );

        public object ThisType
            => new ThisTypeUserReceiver( this.Type, new AspectReferenceSpecification( null, this._common.AspectLayerId, AspectReferenceOrder.Final ) );

        public object BaseType
            => new ThisTypeUserReceiver( this.Type, new AspectReferenceSpecification( null, this._common.AspectLayerId, AspectReferenceOrder.Base ) );

        public IObjectReader Tags => this._common.Tags;

        ScopedDiagnosticSink IMetaApi.Diagnostics => new( this._common.DiagnosticSink, this, this.Declaration, this.Declaration );

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

        public UserDiagnosticSink Diagnostics => this._common.DiagnosticSink;

        internal TemplateMember<IMemberOrNamedType> Template => this._common.Template;

        private MetaApi( IDeclaration declaration, MetaApiProperties common )
            : base( declaration.GetCompilationModel(), common.SyntaxGenerationContext )
        {
            this.Declaration = declaration;
            this._common = common;
        }

        private MetaApi( IMethod method, MetaApiProperties common )
            : this( (IDeclaration) method, common )
        {
            this._method = method;
            this._type = method.DeclaringType;
        }

        private MetaApi( IConstructor constructor, MetaApiProperties common ) : this( (IDeclaration) constructor, common )
        {
            this._constructor = constructor;
            this._type = constructor.DeclaringType;
        }

        private MetaApi( IParameter parameter, MetaApiProperties common, ContractDirection? contractDirection ) : this( parameter, common )
        {
            switch ( parameter.DeclaringMember )
            {
                case IConstructor constructor:
                    this._constructor = constructor;

                    break;

                case IMethod method:
                    this._method = method;

                    break;
            }

            this._type = parameter.DeclaringMember.DeclaringType;
            this._parameter = parameter;
            this._contractDirection = contractDirection;
        }

        private MetaApi( IFieldOrPropertyOrIndexer fieldOrPropertyOrIndexer, IMethod accessor, MetaApiProperties common ) : this( accessor, common )
        {
            this._method = accessor;

            this._fieldOrPropertyOrIndexer = fieldOrPropertyOrIndexer;
            this._type = fieldOrPropertyOrIndexer.DeclaringType;
        }

        private MetaApi( IFieldOrPropertyOrIndexer fieldOrPropertyOrIndexer, MetaApiProperties common, ContractDirection? contractDirection = null )
            : this( (IDeclaration) fieldOrPropertyOrIndexer, common )
        {
            this._fieldOrPropertyOrIndexer = fieldOrPropertyOrIndexer;

            this._type = fieldOrPropertyOrIndexer.DeclaringType;
            this._contractDirection = contractDirection;
        }

        private MetaApi( IEvent eventField, MetaApiProperties common ) : this( (IDeclaration) eventField, common )
        {
            this._event = eventField;
            this._type = eventField.DeclaringType;
        }

        private MetaApi( IEvent @event, IMethod accessor, MetaApiProperties common ) : this( accessor, common )
        {
            this._event = @event;
            this._type = @event.DeclaringType;
            this._method = accessor;
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
                IFieldOrPropertyOrIndexer fieldOrPropertyOrIndexer => new MetaApi( fieldOrPropertyOrIndexer, common, contractDirection ),
                IEvent @event => new MetaApi( @event, common ),
                IConstructor constructor => new MetaApi( constructor, common ),
                IParameter parameter => new MetaApi( parameter, common, contractDirection ),
                _ => throw new AssertionFailedException( $"Unexpected type: {declaration.GetType()}." )
            };

        public static MetaApi ForConstructor( IConstructor constructor, MetaApiProperties common ) => new( common.Translate( constructor ), common );

        public static MetaApi ForMethod( IMethod method, MetaApiProperties common ) => new( common.Translate( method ), common );

        public static MetaApi ForFieldOrPropertyOrIndexer( IFieldOrPropertyOrIndexer fieldOrPropertyOrIndexer, IMethod accessor, MetaApiProperties common )
            => new( common.Translate( fieldOrPropertyOrIndexer ), common.Translate( accessor ), common );

        public static MetaApi ForInitializer( IMember initializedDeclaration, MetaApiProperties common )
            => initializedDeclaration switch
            {
                IFieldOrProperty fieldOrProperty => new MetaApi( common.Translate( fieldOrProperty ), common ),
                IEvent eventField => new MetaApi( common.Translate( eventField ), common ),
                _ => throw new AssertionFailedException( $"Unexpected type: {initializedDeclaration.GetType()}." )
            };

        public static MetaApi ForEvent( IEvent @event, IMethod accessor, MetaApiProperties common )
            => new( common.Translate( @event ), common.Translate( accessor ), common );

        string IDiagnosticSource.DiagnosticSourceDescription
            => $"aspect '{this.AspectInstance?.AspectClass.ShortName}' applied to '{this.AspectInstance?.TargetDeclaration.GetTarget( this.Compilation ).ToDisplayString()}' while applying a template on '{this.Declaration.ToDisplayString()}'";
    }
}