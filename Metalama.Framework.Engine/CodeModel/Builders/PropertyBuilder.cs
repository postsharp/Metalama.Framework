// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class PropertyBuilder : MemberBuilder, IPropertyBuilder, IPropertyImpl
    {
        private readonly bool _hasInitOnlySetter;
        private IType _type;
        private IExpression? _initializerExpression;
        private TemplateMember<IProperty> _initializerTemplate;

        public RefKind RefKind { get; set; }

        public virtual Writeability Writeability
            => this switch
            {
                { SetMethod: null } => Writeability.None,
                { SetMethod: { IsImplicit: true }, IsAutoPropertyOrField: true } => Writeability.ConstructorOnly,
                { _hasInitOnlySetter: true } => Writeability.InitOnly,
                _ => Writeability.All
            };

        public override bool IsImplicit => false;

        public bool IsAutoPropertyOrField { get; }

        public IObjectReader InitializerTags { get; }

        public IType Type
        {
            get => this._type;
            set
            {
                this.CheckNotFrozen();

                this._type = value;
            }
        }

        public IMethodBuilder? GetMethod { get; }

        IMethod? IFieldOrPropertyOrIndexer.GetMethod => this.GetMethod;

        IMethod? IFieldOrPropertyOrIndexer.SetMethod => this.SetMethod;

        public IMethodBuilder? SetMethod { get; }

        protected virtual bool HasBaseInvoker => this.OverriddenProperty != null;

        IInvokerFactory<IFieldOrPropertyInvoker> IFieldOrProperty.Invokers => this.Invokers;

        [Memo]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers
            => new InvokerFactory<IFieldOrPropertyInvoker>(
                ( order, invokerOperator ) => new FieldOrPropertyInvoker( this, order, invokerOperator ),
                this.HasBaseInvoker );

        public IProperty? OverriddenProperty { get; set; }

        public override DeclarationKind DeclarationKind => DeclarationKind.Property;

        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations { get; set; } = Array.Empty<IProperty>();

        public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

        public override IMember? OverriddenMember => this.OverriddenProperty;

        public IExpression? InitializerExpression
        {
            get => this._initializerExpression;
            set
            {
                this.CheckNotFrozen();

                this._initializerExpression = value;
            }
        }

        public TemplateMember<IProperty> InitializerTemplate
        {
            get => this._initializerTemplate;
            set
            {
                this.CheckNotFrozen();

                this._initializerTemplate = value;
            }
        }

        public PropertyBuilder(
            Advice parentAdvice,
            INamedType targetType,
            string name,
            bool hasGetter,
            bool hasSetter,
            bool isAutoProperty,
            bool hasInitOnlySetter,
            bool hasImplicitGetter,
            bool hasImplicitSetter,
            IObjectReader initializerTags )
            : base( parentAdvice, targetType, name )
        {
            // TODO: Sanity checks.

            Invariant.Assert( hasGetter || hasSetter );
            Invariant.Assert( !(!hasSetter && hasImplicitSetter) );
            Invariant.Assert( !(hasInitOnlySetter && hasImplicitSetter) );
            Invariant.Assert( !(!isAutoProperty && hasImplicitSetter) );

            this._type = targetType.Compilation.GetCompilationModel().Factory.GetTypeByReflectionType( typeof(object) );

            if ( hasGetter )
            {
                this.GetMethod = new AccessorBuilder( this, MethodKind.PropertyGet, hasImplicitGetter );
            }

            if ( hasSetter )
            {
                this.SetMethod = new AccessorBuilder( this, MethodKind.PropertySet, hasImplicitSetter );
            }

            this.IsAutoPropertyOrField = isAutoProperty;
            this.InitializerTags = initializerTags;
            this._hasInitOnlySetter = hasInitOnlySetter;
        }

        protected virtual bool GetPropertyInitializerExpressionOrMethod(
            in MemberIntroductionContext context,
            out ExpressionSyntax? initializerExpression,
            out MethodDeclarationSyntax? initializerMethod )
        {
            return this.GetInitializerExpressionOrMethod(
                context,
                this.Type,
                this.InitializerExpression,
                this.InitializerTemplate,
                this.InitializerTags,
                out initializerExpression,
                out initializerMethod );
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
                    this.GetAttributeLists( context ),
                    this.GetSyntaxModifierList(),
                    syntaxGenerator.Type( this.Type.GetSymbol() ),
                    this.ExplicitInterfaceImplementations.Count > 0
                        ? ExplicitInterfaceSpecifier( (NameSyntax) syntaxGenerator.Type( this.ExplicitInterfaceImplementations[0].DeclaringType.GetSymbol() ) )
                        : null,
                    this.GetCleanName(),
                    GenerateAccessorList(),
                    null,
                    initializerExpression != null
                        ? EqualsValueClause( initializerExpression )
                        : null,
                    initializerExpression != null
                        ? Token( SyntaxKind.SemicolonToken )
                        : default );

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
                switch (this.IsAutoPropertyOrField, this.Writeability, this.GetMethod, this.SetMethod)
                {
                    // Properties with both accessors.
                    case (false, _, not null, not null):
                    // Writeable fields.
                    case (true, Writeability.All, { IsImplicit: true }, { IsImplicit: true }):
                    // Auto-properties with both accessors.
                    case (true, Writeability.All or Writeability.InitOnly, { IsImplicit: false }, { IsImplicit: false }):
                        return AccessorList( List( new[] { GenerateGetAccessor(), GenerateSetAccessor() } ) );

                    // Properties with only get accessor.
                    case (false, _, not null, null):
                    // Read only fields or get-only auto properties.
                    case (true, Writeability.ConstructorOnly, { }, { IsImplicit: true }):
                        return AccessorList( List( new[] { GenerateGetAccessor() } ) );

                    // Properties with only set accessor.
                    case (_, _, null, not null):
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

        protected virtual bool GetInitializerExpressionOrMethod(
            in MemberIntroductionContext context,
            out ExpressionSyntax? initializerExpression,
            out MethodDeclarationSyntax? initializerMethod )
        {
            return this.GetInitializerExpressionOrMethod(
                context,
                this.Type,
                this.InitializerExpression,
                this.InitializerTemplate,
                this.InitializerTags,
                out initializerExpression,
                out initializerMethod );
        }

        public IMethod? GetAccessor( MethodKind methodKind )
            => methodKind switch
            {
                MethodKind.PropertyGet => this.GetMethod,
                MethodKind.PropertySet => this.SetMethod,
                _ => null
            };

        public IEnumerable<IMethod> Accessors
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

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => throw new NotImplementedException();

        public void SetExplicitInterfaceImplementation( IProperty interfaceProperty ) => this.ExplicitInterfaceImplementations = new[] { interfaceProperty };

        public override void Freeze()
        {
            base.Freeze();

            ((DeclarationBuilder?) this.GetMethod)?.Freeze();
            ((DeclarationBuilder?) this.SetMethod)?.Freeze();
        }
    }
}