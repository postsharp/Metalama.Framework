// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.InternalInterfaces;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.RunTime;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Caravela.Framework.Code.MethodKind;
using RefKind = Caravela.Framework.Code.RefKind;
using TypedConstant = Caravela.Framework.Code.TypedConstant;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class PropertyBuilder : MemberBuilder, IPropertyBuilder, IPropertyInternal
    {
        private readonly bool _hasInitOnlySetter;

        RefKind IProperty.RefKind => this.RefKind;

        public RefKind RefKind { get; set; }

        public Writeability Writeability
            => this switch
            {
                { Setter: null, IsAutoPropertyOrField: false } => Writeability.None,
                { Setter: null, IsAutoPropertyOrField: true } => Writeability.ConstructorOnly,
                { _hasInitOnlySetter: true } => Writeability.InitOnly,
                _ => Writeability.All
            };

        public bool IsAutoPropertyOrField { get; }

        public ParameterBuilderList Parameters { get; } = new();

        IParameterList IHasParameters.Parameters => this.Parameters;

        IType IFieldOrProperty.Type => this.Type;

        public IType Type { get; set; }

        public IMethodBuilder? Getter { get; }

        IMethod? IFieldOrProperty.Getter => this.Getter;

        IMethod? IFieldOrProperty.Setter => this.Setter;
        
        public IMethodBuilder? Setter { get; }

        IInvokerFactory<IFieldOrPropertyInvoker> IFieldOrProperty.Invokers => this.Invokers;

        [Memo]
        public IInvokerFactory<IPropertyInvoker> Invokers
            => new InvokerFactory<IPropertyInvoker>( ( order, invokerOperator ) => new PropertyInvoker( this, order, invokerOperator ), false );

        public IProperty? OverriddenProperty { get; set; }

        public override InsertPosition InsertPosition
            => new(
                InsertPositionRelation.Within,
                (MemberDeclarationSyntax) ((NamedType) this.DeclaringType).Symbol.GetPrimaryDeclaration().AssertNotNull() );

        public override DeclarationKind DeclarationKind => throw new NotImplementedException();

        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations { get; set; } = Array.Empty<IProperty>();

        public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

        public bool IsIndexer => string.Equals( this.Name, "Items", StringComparison.Ordinal );

        public ExpressionSyntax? InitializerSyntax { get; set; }

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
            this.Type = targetType.Compilation.TypeFactory.GetTypeByReflectionType( typeof(object) );

            if ( hasGetter )
            {
                this.Getter = new AccessorBuilder( this, MethodKind.PropertyGet );
            }

            if ( hasSetter )
            {
                this.Setter = new AccessorBuilder( this, MethodKind.PropertySet );
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

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var syntaxGenerator = LanguageServiceFactory.CSharpSyntaxGenerator;

            // TODO: Indexers.
            var property =
                PropertyDeclaration(
                    List<AttributeListSyntax>(), // TODO: Attributes.
                    this.GetSyntaxModifierList(),
                    syntaxGenerator.TypeExpression( this.Type.GetSymbol() ),
                    this.ExplicitInterfaceImplementations.Count > 0
                        ? ExplicitInterfaceSpecifier(
                            (NameSyntax) syntaxGenerator.TypeExpression( this.ExplicitInterfaceImplementations[0].DeclaringType.GetSymbol() ) )
                        : null,
                    Identifier( this.Name ),
                    GenerateAccessorList(),
                    null,
                    this.InitializerSyntax != null
                        ? EqualsValueClause( this.InitializerSyntax )
                        : null,
                    this.InitializerSyntax != null
                        ? Token( SyntaxKind.SemicolonToken )
                        : default );

            return new[] { new IntroducedMember( this, property, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ) };

            AccessorListSyntax GenerateAccessorList()
            {
                switch (this.Getter, this.Setter)
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

                if ( this.Getter!.Accessibility != this.Accessibility )
                {
                    this.Getter.Accessibility.AddTokens( tokens );
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
                                        DefaultExpression( syntaxGenerator!.TypeExpression( this.Type.GetSymbol() ) ),
                                        Token( SyntaxKind.SemicolonToken ) ) ),
                            null,
                            this.IsAutoPropertyOrField ? Token( SyntaxKind.SemicolonToken ) : default )
                        .NormalizeWhitespace();
            }

            AccessorDeclarationSyntax GenerateSetAccessor()
            {
                var tokens = new List<SyntaxToken>();

                if ( this.Setter!.Accessibility != this.Accessibility )
                {
                    this.Setter.Accessibility.AddTokens( tokens );
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

        public IMethod? GetAccessor( MethodKind methodKind )
            => methodKind switch
            {
                MethodKind.PropertyGet => this.Getter,
                MethodKind.PropertySet => this.Setter,
                _ => null
            };

        [return: RunTimeOnly]
        public PropertyInfo ToPropertyInfo() => throw new NotImplementedException();

        [return: RunTimeOnly]
        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => throw new NotImplementedException();

        public void SetExplicitInterfaceImplementation( IProperty interfaceProperty ) => this.ExplicitInterfaceImplementations = new[] { interfaceProperty };
    }
}