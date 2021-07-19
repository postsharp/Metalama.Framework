// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal sealed class MethodBuilder : MemberBuilder, IMethodBuilder
    {
        public ParameterBuilderList Parameters { get; } = new();

        public GenericParameterBuilderList GenericParameters { get; } = new();

        [Memo]
        public IInvokerFactory<IMethodInvoker> Invokers
            => new InvokerFactory<IMethodInvoker>( ( order, invokerOperator ) => new MethodInvoker( this, order, invokerOperator ), false );

        public IMethod? OverriddenMethod { get; set; }

        public MethodInfo ToMethodInfo() => throw new NotImplementedException();

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

        IParameterList IHasParameters.Parameters => this.Parameters;

        IGenericParameterList IMethod.GenericParameters => this.GenericParameters;

        IReadOnlyList<IType> IMethod.GenericArguments => ImmutableArray<IType>.Empty;

        bool IMethod.IsOpenGeneric => this.GenericParameters.Count > 0;

        // We don't currently support adding other methods than default ones.
        public MethodKind MethodKind => MethodKind.Default;

        public bool IsReadOnly { get; set; }

        System.Reflection.MethodBase IMethodBase.ToMethodBase() => this.ToMethodInfo();

        IMethod IMethod.WithGenericArguments( params IType[] genericArguments ) => throw new NotImplementedException();

        public override DeclarationKind DeclarationKind => DeclarationKind.Method;

        public IReadOnlyList<IMethod> ExplicitInterfaceImplementations { get; private set; } = Array.Empty<IMethod>();

        public MethodBuilder( Advice parentAdvice, INamedType targetType, string name )
            : base( parentAdvice, targetType, name )
        {
            this.ReturnParameter =
                new ParameterBuilder(
                    this,
                    -1,
                    null,
                    this.Compilation.Factory.GetTypeByReflectionType( typeof(void) ).AssertNotNull(),
                    RefKind.None );
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var syntaxGenerator = LanguageServiceFactory.CSharpSyntaxGenerator;

            var method =
                MethodDeclaration(
                    List<AttributeListSyntax>(),
                    this.GetSyntaxModifierList(),
                    this.GetSyntaxReturnType(),
                    this.ExplicitInterfaceImplementations.Count > 0
                        ? ExplicitInterfaceSpecifier(
                            (NameSyntax) syntaxGenerator.TypeExpression( this.ExplicitInterfaceImplementations[0].DeclaringType.GetSymbol() ) )
                        : null,
                    Identifier( this.Name ),
                    this.GetSyntaxTypeParameterList(),
                    this.GetSyntaxParameterList(),
                    this.GetSyntaxConstraintClauses(),
                    Block(
                        List(
                            !this.ReturnParameter.ParameterType.Is( typeof(void) )
                                ? new[]
                                {
                                    ReturnStatement(
                                        Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( Whitespace( " " ) ),
                                        DefaultExpression( syntaxGenerator.TypeExpression( this.ReturnParameter.ParameterType.GetSymbol() ) ),
                                        Token( SyntaxKind.SemicolonToken ) )
                                }
                                : Array.Empty<StatementSyntax>() ) ),
                    null );

            return new[] { new IntroducedMember( this, method, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ) };
        }

        // TODO: Temporary
        public override InsertPosition InsertPosition
            => new(
                InsertPositionRelation.Within,
                (MemberDeclarationSyntax) ((NamedType) this.DeclaringType).Symbol.GetPrimaryDeclaration().AssertNotNull() );

        public void SetExplicitInterfaceImplementation( IMethod interfaceMethod ) => this.ExplicitInterfaceImplementations = new[] { interfaceMethod };

        public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append( this.DeclaringType.ToDisplayString( format, context ) );
            stringBuilder.Append( "." );
            stringBuilder.Append( this.Name );
            stringBuilder.Append( "(" );

            foreach ( var parameter in this.Parameters )
            {
                if ( parameter.Index > 0 )
                {
                    stringBuilder.Append( ", " );
                }

                stringBuilder.Append( parameter.ParameterType.ToDisplayString( format, context ) );
            }

            stringBuilder.Append( ")" );

            return stringBuilder.ToString();
        }
    }
}