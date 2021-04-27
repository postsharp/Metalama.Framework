// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal sealed class MethodBuilder : MemberBuilder, IMethodBuilder
    {
        public ParameterBuilderList Parameters { get; } = new();

        public GenericParameterBuilderList GenericParameters { get; } = new();

        public IMethod? OverriddenMethod { get; set; }

        public AspectLinkerOptions? LinkerOptions { get; }

        public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant defaultValue = default )
        {
            var parameter = new ParameterBuilder( this, this.Parameters.Count, name, type, refKind );
            parameter.DefaultValue = defaultValue;
            this.Parameters.Add( parameter );

            return parameter;
        }

        public IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, object? defaultValue = null )
        {
            var iType = this.Compilation.Factory.GetTypeByReflectionType( type );
            var typeConstant = defaultValue != null ? new TypedConstant( iType, defaultValue ) : default;

            return this.AddParameter( name, iType, refKind, typeConstant );
        }

        public IGenericParameterBuilder AddGenericParameter( string name ) => throw new NotImplementedException();

        IParameterBuilder IMethodBuilder.ReturnParameter => this.ReturnParameter;

        IType IMethodBuilder.ReturnType
        {
            get => this.ReturnParameter.ParameterType;
            set => this.ReturnParameter.ParameterType = value ?? throw new ArgumentNullException( nameof(value) );
        }

        IType IMethod.ReturnType => this.ReturnParameter.ParameterType;

        public ParameterBuilder ReturnParameter { get; }

        IParameter IMethod.ReturnParameter => this.ReturnParameter;

        IMethodList IMethodBase.LocalFunctions => MethodList.Empty;

        IParameterList IMethodBase.Parameters => this.Parameters;

        IGenericParameterList IMethod.GenericParameters => this.GenericParameters;

        IReadOnlyList<IType> IMethod.GenericArguments => ImmutableArray<IType>.Empty;

        bool IMethod.IsOpenGeneric => true;

        // We don't currently support adding other methods than default ones.
        public MethodKind MethodKind => MethodKind.Default;

        IMethod IMethod.WithGenericArguments( params IType[] genericArguments ) => throw new NotImplementedException();

        bool IMethod.HasBase => throw new NotImplementedException();

        IMethodInvocation IMethod.Base => throw new NotImplementedException();

        public override CodeElementKind ElementKind => CodeElementKind.Method;

        public MethodBuilder( Advice parentAdvice, INamedType targetType, string name, AspectLinkerOptions? linkerOptions )
            : base( parentAdvice, targetType, name )
        {
            this.LinkerOptions = linkerOptions;

            this.ReturnParameter =
                new ParameterBuilder(
                    this,
                    -1,
                    null,
                    this.Compilation.Factory.GetTypeByReflectionType( typeof(void) ).AssertNotNull(),
                    RefKind.None );
        }

        // TODO: #(28532) Implement properly.
        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            return this.Name;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var syntaxGenerator = this.Compilation.SyntaxGenerator;

            var method = (MethodDeclarationSyntax)
                syntaxGenerator.MethodDeclaration(
                    this.Name,
                    this.Parameters.AsBuilderList.Select( p => p.ToDeclarationSyntax() ),
                    this.GenericParameters.AsBuilderList.Select( p => p.Name ),
                    syntaxGenerator.TypeExpression( this.ReturnParameter.ParameterType.GetSymbol() ),
                    this.Accessibility.ToRoslynAccessibility(),
                    this.ToDeclarationModifiers(),
                    !this.ReturnParameter.ParameterType.Is( typeof(void) )
                        ? new[]
                        {
                            ReturnStatement(
                                LiteralExpression(
                                    SyntaxKind.DefaultLiteralExpression,
                                    Token( SyntaxKind.DefaultKeyword ) ) )
                        }
                        : null );

            return new[]
            {
                new IntroducedMember( this, method, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this.LinkerOptions, this )
            };
        }

        // TODO: Temporary
        public override MemberDeclarationSyntax InsertPositionNode
            => ((NamedType) this.DeclaringType).Symbol.DeclaringSyntaxReferences.Select( x => (TypeDeclarationSyntax) x.GetSyntax() ).FirstOrDefault();

        dynamic IMethodInvocation.Invoke( dynamic? instance, params dynamic[] args ) => throw new NotImplementedException();
    }
}