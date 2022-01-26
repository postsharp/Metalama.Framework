﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class PropertyBuilder : FieldOrPropertyBuilder, IPropertyBuilder, IPropertyImpl
    {
        private readonly bool _hasInitOnlySetter;

        RefKind IProperty.RefKind => this.RefKind;

        public RefKind RefKind { get; set; }

        public override Writeability Writeability
        {
            get => this switch
            {
                { SetMethod: null, IsAutoPropertyOrField: false } => Writeability.None,
                { SetMethod: null, IsAutoPropertyOrField: true } => Writeability.ConstructorOnly,
                { _hasInitOnlySetter: true } => Writeability.InitOnly,
                _ => Writeability.All
            };

            set => throw new NotSupportedException();
        }

        public override bool IsAutoPropertyOrField { get; }

        public ParameterBuilderList Parameters { get; } = new();

        IParameterList IHasParameters.Parameters => this.Parameters;

        public override IType Type { get; set; }

        public override IMethodBuilder? GetMethod { get; }

        IMethod? IFieldOrProperty.GetMethod => this.GetMethod;

        IMethod? IFieldOrProperty.SetMethod => this.SetMethod;

        public override IMethodBuilder? SetMethod { get; }

        protected virtual bool HasBaseInvoker => this.OverriddenProperty != null;

        IInvokerFactory<IFieldOrPropertyInvoker> IFieldOrProperty.Invokers => this.Invokers;

        protected override IInvokerFactory<IFieldOrPropertyInvoker> FieldOrPropertyInvokers => this.Invokers;

        [Memo]
        public IInvokerFactory<IPropertyInvoker> Invokers
            => new InvokerFactory<IPropertyInvoker>( ( order, invokerOperator ) => new PropertyInvoker( this, order, invokerOperator ), this.HasBaseInvoker );

        public IProperty? OverriddenProperty { get; set; }

        public override InsertPosition InsertPosition
            => new(
                InsertPositionRelation.Within,
                (MemberDeclarationSyntax) ((NamedType) this.DeclaringType).Symbol.GetPrimaryDeclaration().AssertNotNull() );

        public override DeclarationKind DeclarationKind => DeclarationKind.Property;

        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations { get; set; } = Array.Empty<IProperty>();

        public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

        public override IMember? OverriddenMember => this.OverriddenProperty;

        public bool IsIndexer => string.Equals( this.Name, "Items", StringComparison.Ordinal );

        public override IExpression? InitializerExpression { get; set; }

        public TemplateMember<IProperty> InitializerTemplate { get; set; }

        public PropertyBuilder(
            Advice parentAdvice,
            INamedType targetType,
            string name,
            bool hasGetter,
            bool hasSetter,
            bool isAutoProperty,
            bool hasInitOnlySetter )
            : base( parentAdvice, targetType, name )
        {
            // TODO: Sanity checks.

            Invariant.Assert( hasGetter || hasSetter );

            this.Type = targetType.Compilation.TypeFactory.GetTypeByReflectionType( typeof(object) );

            if ( hasGetter )
            {
                this.GetMethod = new AccessorBuilder( this, MethodKind.PropertyGet );
            }

            if ( hasSetter )
            {
                this.SetMethod = new AccessorBuilder( this, MethodKind.PropertySet );
            }

            this.IsAutoPropertyOrField = isAutoProperty;
            this._hasInitOnlySetter = hasInitOnlySetter;
        }

        public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant defaultValue = default )
        {
            if ( this.IsIndexer )
            {
                var parameter = new ParameterBuilder( this, this.Parameters.Count, name, type, refKind );
                parameter.DefaultValue = defaultValue;
                this.Parameters.Add( parameter );

                return parameter;
            }
            else
            {
                throw new NotSupportedException( "Adding parameters is only supported on indexers." );
            }
        }

        public IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, object? defaultValue = null )
        {
            if ( this.IsIndexer )
            {
                var itype = this.Compilation.Factory.GetTypeByReflectionType( type );

                var parameter = new ParameterBuilder( this, this.Parameters.Count, name, itype, refKind )
                {
                    DefaultValue = new TypedConstant( itype, defaultValue )
                };

                this.Parameters.Add( parameter );

                return parameter;
            }
            else
            {
                throw new NotSupportedException( "Adding parameters is only supported on indexers." );
            }
        }

        protected virtual bool GetPropertyInitializerExpressionOrMethod(
            in MemberIntroductionContext context,
            out ExpressionSyntax? initializerExpression,
            out MethodDeclarationSyntax? initializerMethod )
        {
            return this.GetInitializerExpressionOrMethod( context, this.InitializerExpression, this.InitializerTemplate, out initializerExpression, out initializerMethod );
        }
        
        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

            // TODO: What if non-auto property has the initializer template?

            // If template fails to expand, we will still generate the field, albeit without the initializer.
            _ = this.GetPropertyInitializerExpressionOrMethod( context, out var initializerExpression, out var initializerMethod );

            // TODO: Indexers.
            var property =
                PropertyDeclaration(
                    List<AttributeListSyntax>(), // TODO: Attributes.
                    this.GetSyntaxModifierList(),
                    syntaxGenerator.Type( this.Type.GetSymbol() ),
                    this.ExplicitInterfaceImplementations.Count > 0
                        ? ExplicitInterfaceSpecifier( (NameSyntax) syntaxGenerator.Type( this.ExplicitInterfaceImplementations[0].DeclaringType.GetSymbol() ) )
                        : null,
                    Identifier( this.Name ),
                    GenerateAccessorList(),
                    null,
                    initializerExpression != null
                        ? EqualsValueClause( initializerExpression )
                        : null );

            var introducedProperty = new IntroducedMember( this, property, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this );
            var introducedInitializerMethod = 
                initializerMethod != null
                ? new IntroducedMember( this, initializerMethod, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.InitializerMethod, this )
                : null;

            if ( introducedInitializerMethod != null )
            {
                return new[] { introducedProperty, introducedInitializerMethod };
            }
            else
            {
                return new[] { introducedProperty };
            }

            AccessorListSyntax GenerateAccessorList()
            {
                switch (Getter: this.GetMethod, Setter: this.SetMethod)
                {
                    case (not null, not null):
                        return AccessorList( List( new[] { GenerateGetAccessor(), GenerateSetAccessor() } ) );

                    case (not null, null):
                        return AccessorList( List( new[] { GenerateGetAccessor() } ) );

                    case (null, not null):
                        return AccessorList( List( new[] { GenerateSetAccessor() } ) );

                    default:
                        throw new AssertionFailedException();
                }
            }

            AccessorDeclarationSyntax GenerateGetAccessor()
            {
                var tokens = new List<SyntaxToken>();

                if ( this.GetMethod!.Accessibility != this.Accessibility )
                {
                    this.GetMethod.Accessibility.AddTokens( tokens );
                }

                // TODO: Attributes.
                return
                    AccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration,
                            List<AttributeListSyntax>(),
                            TokenList( tokens ),
                            Token( SyntaxKind.GetKeyword ),
                            this.IsAutoPropertyOrField
                                ? null
                                : Block(
                                    ReturnStatement(
                                        Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( Whitespace( " " ) ),
                                        DefaultExpression( syntaxGenerator.Type( this.Type.GetSymbol() ) ),
                                        Token( SyntaxKind.SemicolonToken ) ) ),
                            null,
                            this.IsAutoPropertyOrField ? Token( SyntaxKind.SemicolonToken ) : default )
                        .NormalizeWhitespace();
            }

            AccessorDeclarationSyntax GenerateSetAccessor()
            {
                var tokens = new List<SyntaxToken>();

                if ( this.SetMethod!.Accessibility != this.Accessibility )
                {
                    this.SetMethod.Accessibility.AddTokens( tokens );
                }

                return
                    AccessorDeclaration(
                        this._hasInitOnlySetter ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration,
                        List<AttributeListSyntax>(),
                        TokenList( tokens ),
                        this._hasInitOnlySetter ? Token( SyntaxKind.InitKeyword ) : Token( SyntaxKind.SetKeyword ),
                        this.IsAutoPropertyOrField
                            ? null
                            : Block(),
                        null,
                        this.IsAutoPropertyOrField ? Token( SyntaxKind.SemicolonToken ) : default );
            }
        }

        protected virtual bool GetInitializerExpressionOrMethod( in MemberIntroductionContext context, out ExpressionSyntax? initializerExpression, out MethodDeclarationSyntax? initializerMethod )
        {
            BlockSyntax? initializerBlock;

            if ( this.InitializerExpression != null )
            {
                // TODO: Error about the expression type?
                initializerMethod = null;
                initializerExpression = ((IUserExpression) this.InitializerExpression).ToRunTimeExpression().Syntax;
                return true;
            }
            else if ( this.InitializerTemplate.IsNotNull )
            {
                initializerExpression = null;
                if ( !this.TryExpandInitializerTemplate( context, this.InitializerTemplate, out initializerBlock ) )
                {
                    // Template expansion error.
                    initializerMethod = null;
                    initializerExpression = null;
                    return false;
                }

                // If the initializer block contains only a single return statement, 
                if ( initializerBlock.Statements.Count == 1 && initializerBlock.Statements[0] is ReturnStatementSyntax { Expression: not null } returnStatement )
                {
                    initializerMethod = null;
                    initializerExpression = returnStatement.Expression;
                    return true;
                }
            }
            else
            {
                initializerMethod = null;
                initializerExpression = null;
                return true;
            }

            var initializerName = context.IntroductionNameProvider.GetInitializerName( this.DeclaringType, this.ParentAdvice.AspectLayerId, this );

            if ( initializerBlock != null )
            {
                initializerExpression = InvocationExpression( IdentifierName( initializerName ) );
                initializerMethod =
                    MethodDeclaration(
                        List<AttributeListSyntax>(),
                        TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) ),
                        context.SyntaxGenerator.Type( this.Type.GetSymbol() ),
                        null,
                        Identifier( initializerName ),
                        null,
                        ParameterList(),
                        List<TypeParameterConstraintClauseSyntax>(),
                        initializerBlock,
                        null );
                return true;
            }
            else
            {
                initializerMethod = null;
                return true;
            }
        }

        private bool TryExpandInitializerTemplate(
            MemberIntroductionContext context,
            TemplateMember<IProperty> initializerTemplate,
            [NotNullWhen( true )] out BlockSyntax? expression )
        {
            using ( context.DiagnosticSink.WithDefaultScope( this ) )
            {
                var metaApi = MetaApi.ForFieldOrPropertyInitializer(
                    this,
                    new MetaApiProperties(
                        context.DiagnosticSink,
                        initializerTemplate.Cast(),
                        this.ParentAdvice.ReadOnlyTags,
                        this.ParentAdvice.AspectLayerId,
                        context.SyntaxGenerationContext,
                        this.ParentAdvice.Aspect,
                        context.ServiceProvider ) );

                var expansionContext = new TemplateExpansionContext(
                    this.ParentAdvice.Aspect.Aspect,
                    metaApi,
                    this.Compilation,
                    context.LexicalScopeProvider.GetLexicalScope( this ),
                    context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                    context.SyntaxGenerationContext,
                    default,
                    null,
                    this.ParentAdvice.AspectLayerId );

                var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( this.InitializerTemplate.Declaration! );

                return templateDriver.TryExpandDeclaration( expansionContext, context.DiagnosticSink, out expression );
            }
        }

        public override IMethod? GetAccessor( MethodKind methodKind )
            => methodKind switch
            {
                MethodKind.PropertyGet => this.GetMethod,
                MethodKind.PropertySet => this.SetMethod,
                _ => null
            };

        public override IEnumerable<IMethod> Accessors
        {
            get
            {
                if ( this.GetMethod != null )
                {
                    yield return this.GetMethod;
                }

                if ( this.SetMethod != null )
                {
                    yield return this.SetMethod;
                }
            }
        }

        public PropertyInfo ToPropertyInfo() => throw new NotImplementedException();

        public override FieldOrPropertyInfo ToFieldOrPropertyInfo() => throw new NotImplementedException();

        public void SetExplicitInterfaceImplementation( IProperty interfaceProperty ) => this.ExplicitInterfaceImplementations = new[] { interfaceProperty };
    }
}