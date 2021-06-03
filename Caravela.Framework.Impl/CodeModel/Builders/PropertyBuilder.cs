// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Caravela.Framework.Code.MethodKind;
using RefKind = Caravela.Framework.Code.RefKind;
using TypedConstant = Caravela.Framework.Code.TypedConstant;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class PropertyBuilder : MemberBuilder, IPropertyBuilder
    {
        // TODO: How to set this from user code? Now it's only possible to do through template.
        private readonly bool _isAutoProperty;
        private readonly bool _hasInitOnlySetter;

        RefKind IProperty.RefKind => this.RefKind;

        public RefKind RefKind { get; set; }

        public bool IsByRef => this.RefKind != RefKind.None;

        public bool IsRef => this.RefKind == RefKind.Ref;

        public bool IsRefReadonly => this.RefKind == RefKind.RefReadOnly;

        public ParameterBuilderList Parameters { get; } = new();

        IParameterList IHasParameters.Parameters => this.Parameters;

        IType IFieldOrProperty.Type => this.Type;

        public IType Type { get; set; }

        public IMethodBuilder? Getter { get; }

        IMethod? IFieldOrProperty.Getter => this.Getter;

        public IMethodBuilder? Setter { get; }

        IMethod? IFieldOrProperty.Setter => this.Setter;

        public IFieldOrPropertyInvoker? BaseInvoker => throw new NotImplementedException();

        IPropertyInvoker IProperty.Invoker => throw new NotImplementedException();

        public IFieldOrPropertyInvoker Invoker => throw new NotImplementedException();

        public AspectLinkerOptions? LinkerOptions { get; }

        public override MemberDeclarationSyntax InsertPositionNode
            => ((NamedType) this.DeclaringType).Symbol.DeclaringSyntaxReferences.Select( x => (TypeDeclarationSyntax) x.GetSyntax() ).FirstOrDefault();

        public override DeclarationKind DeclarationKind => throw new NotImplementedException();

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations => Array.Empty<IProperty>();

        public bool IsIndexer => this.Name == "Items";

        public PropertyBuilder(
            Advice parentAdvice,
            INamedType targetType,
            string name,
            bool hasGetter,
            bool hasSetter,
            bool isAutoProperty,
            bool hasInitOnlySetter,
            AspectLinkerOptions? linkerOptions )
            : base( parentAdvice, targetType, name )
        {
            // TODO: Sanity checks.

            this.LinkerOptions = linkerOptions;
            this.Type = targetType.Compilation.TypeFactory.GetTypeByReflectionType( typeof(object) );

            if ( hasGetter )
            {
                this.Getter = new AccessorBuilder( this, MethodKind.PropertyGet );
            }

            if ( hasSetter )
            {
                this.Setter = new AccessorBuilder( this, MethodKind.PropertySet );
            }

            this._isAutoProperty = isAutoProperty;
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
                    GenerateModifierList(),
                    (TypeSyntax) syntaxGenerator.TypeExpression( this.Type.GetSymbol() ),
                    null,
                    Identifier( this.Name ),
                    GenerateAccessorList(),
                    null,
                    null );

            return new[]
            {
                new IntroducedMember( this, property, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this.LinkerOptions, this )
            };

            SyntaxTokenList GenerateModifierList()
            {
                // Modifiers for property.
                var tokens = new List<SyntaxToken>();

                this.Accessibility.AddTokens( tokens );

                if ( this.IsAbstract )
                {
                    tokens.Add( Token( SyntaxKind.AbstractKeyword ) );
                }

                if ( this.IsSealed )
                {
                    tokens.Add( Token( SyntaxKind.SealedKeyword ) );
                }

                if ( this.IsOverride )
                {
                    tokens.Add( Token( SyntaxKind.OverrideKeyword ) );
                }

                this.RefKind.AddReturnValueTokens( tokens );

                return TokenList( tokens );
            }

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
                        this._isAutoProperty
                            ? null
                            : Block( ReturnStatement( DefaultExpression( (TypeSyntax) syntaxGenerator!.TypeExpression( this.Type.GetSymbol() ) ) ) ),
                        null );
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
                        this._isAutoProperty
                            ? null
                            : Block(),
                        null );
            }
        }

        [return: RunTimeOnly]
        public PropertyInfo ToPropertyInfo()
        {
            throw new NotImplementedException();
        }

        IPropertyInvoker? IProperty.BaseInvoker => throw new NotImplementedException();

        [return: RunTimeOnly]
        public FieldOrPropertyInfo ToFieldOrPropertyInfo()
        {
            throw new NotImplementedException();
        }
    }
}