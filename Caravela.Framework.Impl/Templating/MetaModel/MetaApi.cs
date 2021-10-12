﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Advised;
using Caravela.Framework.Code.SyntaxBuilders;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using SpecialType = Caravela.Framework.Code.SpecialType;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// The implementation of <see cref="IMetaApi"/>.
    /// </summary>
    internal class MetaApi : IMetaApi, IMetaTarget, IMetaCodeBuilder
    {
        private readonly IAdvisedFieldOrProperty? _fieldOrProperty;
        private readonly IAdvisedMethod? _method;
        private readonly IAdvisedEvent? _event;
        private readonly INamedType? _type;
        private readonly MetaApiProperties _common;

        private Exception CreateInvalidOperationException( string memberName, string? description = null )
            => TemplatingDiagnosticDescriptors.MemberMemberNotAvailable.CreateException(
                (this._common.Template.Declaration!, "meta." + memberName, this.Declaration, this.Declaration.DeclarationKind,
                 description ?? "I" + memberName) );

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

        public IProject Project => this.Compilation.Project;

        private ThisInstanceUserReceiver GetThisOrBase( string expressionName, AspectReferenceSpecification linkerAnnotation )
            => this._type switch
            {
                null => throw this.CreateInvalidOperationException( expressionName ),
                { IsStatic: false } when this.Declaration is IMemberOrNamedType { IsStatic: false }
                    => new ThisInstanceUserReceiver(
                        this.Type,
                        linkerAnnotation ),

                _ => throw TemplatingDiagnosticDescriptors.CannotUseThisInStaticContext.CreateException(
                    (this._common.Template.Declaration!, expressionName, this.Declaration, this.Declaration.DeclarationKind) )
            };

        public IMetaTarget Target => this;

        public IMetaCodeBuilder CodeBuilder => this;

        public IAspectInstance AspectInstance => this._common.AspectInstance;

        public object This => this.GetThisOrBase( "meta.This", new AspectReferenceSpecification( this._common.AspectLayerId, AspectReferenceOrder.Final ) );

        public object Base => this.GetThisOrBase( "meta.Base", new AspectReferenceSpecification( this._common.AspectLayerId, AspectReferenceOrder.Base ) );

        public object ThisStatic
            => new ThisTypeUserReceiver( this.Type, new AspectReferenceSpecification( this._common.AspectLayerId, AspectReferenceOrder.Final ) );

        public object BaseStatic
            => new ThisTypeUserReceiver( this.Type, new AspectReferenceSpecification( this._common.AspectLayerId, AspectReferenceOrder.Base ) );

        public IReadOnlyDictionary<string, object?> Tags => this._common.Tags;

        IDiagnosticSink IMetaApi.Diagnostics => this._common.Diagnostics;

        [ExcludeFromCodeCoverage]
        public void DebugBreak()
        {
            var trustOptions = this._common.ServiceProvider.GetService<IProjectOptions>();

            if ( !trustOptions.IsUserCodeTrusted )
            {
                return;
            }

            if ( Debugger.IsAttached )
            {
                Debugger.Break();
            }
        }

        public IExpression Expression( object? expression )
            => RuntimeExpression.FromValue( expression, this.Compilation, TemplateExpansionContext.CurrentSyntaxGenerationContext )
                .ToUserExpression( this.Compilation );

        public IExpression BuildArray( ArrayBuilder arrayBuilder ) => new ArrayUserExpression( arrayBuilder );

        public IExpression BuildInterpolatedString( InterpolatedStringBuilder interpolatedStringBuilder )
            => new InterpolatedStringUserExpression( interpolatedStringBuilder, this.Compilation );

        public IExpression ParseExpression( string code )
        {
            var expression = SyntaxFactory.ParseExpression( code ).WithAdditionalAnnotations( Formatter.Annotation );

            return new RuntimeExpression( expression, this.Compilation, this.Project.ServiceProvider ).ToUserExpression( this.Compilation );
        }

        public IStatement ParseStatement( string code )
        {
            var statement = SyntaxFactory.ParseStatement( code );

            return new UserStatement( statement );
        }

        public void AppendLiteral( object? value, StringBuilder stringBuilder, SpecialType specialType, bool stronglyTyped )
        {
            if ( value == null )
            {
                stringBuilder.Append( stronglyTyped ? "default(string)" : "null" );
            }
            else
            {
                var code = SyntaxFactoryEx.LiteralExpression( value ).Token.Text;

                string suffix = "", prefix = "";

                if ( stronglyTyped )
                {
                    if ( int.TryParse( code, out _ ) && specialType != SpecialType.Int32 )
                    {
                        // Specify the suffix if there is an ambiguity.

                        suffix = specialType switch
                        {
                            SpecialType.UInt32 => "u",
                            SpecialType.Int64 => "l",
                            SpecialType.UInt64 => "ul",
                            SpecialType.Single => "f",
                            SpecialType.Double => "d",
                            SpecialType.Decimal => "m",
                            _ => ""
                        };
                    }

                    prefix = specialType switch
                    {
                        SpecialType.Byte => "(byte) ",
                        SpecialType.SByte => "(sbyte) ",
                        SpecialType.Int16 => "(short) ",
                        SpecialType.UInt16 => "(ushort) ",
                        _ => ""
                    };
                }

                stringBuilder.Append( prefix );
                stringBuilder.Append( code );
                stringBuilder.Append( suffix );
            }
        }

        public void AppendTypeName( IType type, StringBuilder stringBuilder )
        {
            var code = this._common.SyntaxGenerationContext.SyntaxGenerator.Type( type.GetSymbol().AssertNotNull() ).ToString();
            stringBuilder.Append( code );
        }

        public void AppendTypeName( Type type, StringBuilder stringBuilder )
            => this.AppendTypeName(
                this.Compilation.GetCompilationModel().Factory.GetTypeByReflectionType( type ),
                stringBuilder );

        public void AppendExpression( IExpression expression, StringBuilder stringBuilder )
        {
            stringBuilder.Append(
                ((IUserExpression) expression.Value!).ToRunTimeExpression()
                .Syntax
                .NormalizeWhitespace()
                .ToFullString() );
        }

        public void AppendDynamic( object? expression, StringBuilder stringBuilder )
            => stringBuilder.Append(
                expression == null
                    ? "null"
                    : ((RuntimeExpression) expression).Syntax.NormalizeWhitespace().ToFullString() );

        public AspectExecutionScenario ExecutionScenario => this._common.PipelineDescription.ExecutionScenario;

        public UserDiagnosticSink Diagnostics => this._common.Diagnostics;

        private MetaApi( IDeclaration declaration, MetaApiProperties common )
        {
            this.Declaration = declaration;
            this.Compilation = declaration.Compilation;
            this._common = common;

            var serviceProviderMark = this._common.ServiceProvider.GetService<ServiceProviderMark>();

            if ( serviceProviderMark != ServiceProviderMark.Project && serviceProviderMark != ServiceProviderMark.Test )
            {
                // We should get a project-specific service provider here.
                throw new AssertionFailedException();
            }
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