// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using RefKind = Caravela.Framework.Code.RefKind;
using TypedConstant = Caravela.Framework.Code.TypedConstant;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class PropertyBuilder : MemberBuilder, IPropertyBuilder, IProperty
    {
        RefKind IProperty.RefKind => this.RefKind;

        public RefKind RefKind { get; set; }

        public bool IsByRef => throw new NotImplementedException();

        public bool IsRef => throw new NotImplementedException();

        public bool IsRefReadonly => throw new NotImplementedException();

        public ParameterBuilderList Parameters { get; } = new();

        IParameterList IProperty.Parameters => this.Parameters;

        public IPropertyInvocation Base => throw new NotImplementedException();

        IType IFieldOrProperty.Type => this.Type;

        public IType Type { get; set; }

        [Memo]
        public IMethod? Getter => new PseudoAccessor( this, PseudoAccessorSemantic.Get );

        [Memo]
        public IMethod? Setter => new PseudoAccessor( this, PseudoAccessorSemantic.Set );

        public bool HasBase => throw new NotImplementedException();

        public dynamic Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IFieldOrPropertyInvocation IFieldOrProperty.Base => throw new NotImplementedException();

        public AspectLinkerOptions? LinkerOptions { get; }

        public override MemberDeclarationSyntax InsertPositionNode
            => ((NamedType) this.DeclaringType).Symbol.DeclaringSyntaxReferences.Select( x => (TypeDeclarationSyntax) x.GetSyntax() ).FirstOrDefault();

        public override CodeElementKind ElementKind => throw new NotImplementedException();

        public PropertyBuilder( Advice parentAdvice, INamedType targetType, string name, AspectLinkerOptions? linkerOptions )
            : base( parentAdvice, targetType, name )
        {
            this.LinkerOptions = linkerOptions;
            this.Type = targetType.Compilation.TypeFactory.GetTypeByReflectionType( typeof( object ) );
        }

        public dynamic GetIndexerValue( dynamic? instance, params dynamic[] args )
        {
            throw new NotImplementedException();
        }

        public dynamic GetValue( dynamic? instance )
        {
            throw new NotImplementedException();
        }

        public dynamic SetIndexerValue( dynamic? instance, dynamic value, params dynamic[] args )
        {
            throw new NotImplementedException();
        }

        public dynamic SetValue( dynamic? instance, dynamic value )
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var syntaxGenerator = this.Compilation.SyntaxGenerator;
            var reflectionMapper = ReflectionMapper.GetInstance( this.Compilation.RoslynCompilation );

            // TODO: Indexers.
            var property = (PropertyDeclarationSyntax)
                syntaxGenerator.PropertyDeclaration(
                    this.Name,
                    syntaxGenerator.TypeExpression( this.Type.GetSymbol() ),
                    this.Accessibility.ToRoslynAccessibility(),
                    this.ToDeclarationModifiers(),
                    new[]
                    {
                        ReturnStatement( DefaultExpression( (TypeSyntax) syntaxGenerator.TypeExpression( this.Type.GetSymbol() ) ) )
                    },
                    Array.Empty<SyntaxNode>() );

            return new[]
            {
                new IntroducedMember( this, property, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this.LinkerOptions, this )
            };
        }

        public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant defaultValue = default )
        {
            throw new NotImplementedException();
        }

        public IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, object? defaultValue = null )
        {
            throw new NotImplementedException();
        }
    }
}