// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal sealed class MethodBuilder : MemberBuilder, IMethodBuilder, IMethodImpl
    {
        public ParameterBuilderList Parameters { get; } = new();

        public GenericParameterBuilderList GenericParameters { get; } = new();

        public override string Name { get; set; }

        // A builder is never accessed directly from user code and never represents a generic type instance,
        // so we don't need an implementation of GenericArguments.
        public IReadOnlyList<IType> TypeArguments => throw new NotSupportedException();

        [Memo]
        public IInvokerFactory<IMethodInvoker> Invokers
            => new InvokerFactory<IMethodInvoker>(
                ( order, invokerOperator ) => new MethodInvoker( this, order, invokerOperator ),
                this.OverriddenMethod != null );

        public IMethod? OverriddenMethod { get; set; }

        public MethodInfo ToMethodInfo() => throw new NotImplementedException();

        IMemberWithAccessors? IMethod.DeclaringMember => null;

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

        public ITypeParameterBuilder AddTypeParameter( string name )
        {
            var builder = new TypeParameterBuilder( this, this.GenericParameters.Count, name );
            this.GenericParameters.Add( builder );

            return builder;
        }

        IParameterBuilder IMethodBuilder.ReturnParameter => this.ReturnParameter;

        IType IMethodBuilder.ReturnType
        {
            get => this.ReturnParameter.Type;
            set => this.ReturnParameter.Type = value ?? throw new ArgumentNullException( nameof(value) );
        }

        IType IMethod.ReturnType => this.ReturnParameter.Type;

        public ParameterBuilder ReturnParameter { get; }

        IParameter IMethod.ReturnParameter => this.ReturnParameter;

        IMethodList IMethodBase.LocalFunctions => MethodList.Empty;

        IParameterList IHasParameters.Parameters => this.Parameters;

        IGenericParameterList IGeneric.TypeParameters => this.GenericParameters;

        public bool IsOpenGeneric => this.GenericParameters.Count > 0 || this.DeclaringType.IsOpenGeneric;

        public bool IsGeneric => this.GenericParameters.Count > 0;

        // We don't currently support adding other methods than default ones.
        public MethodKind MethodKind => MethodKind.Default;

        public bool IsReadOnly { get; set; }

        System.Reflection.MethodBase IMethodBase.ToMethodBase() => this.ToMethodInfo();

        IGeneric IGenericInternal.ConstructGenericInstance( params IType[] typeArguments ) => throw new NotImplementedException();

        public override DeclarationKind DeclarationKind => DeclarationKind.Method;

        public IReadOnlyList<IMethod> ExplicitInterfaceImplementations { get; private set; } = Array.Empty<IMethod>();

        public MethodBuilder( Advice parentAdvice, INamedType targetType, string name, IObjectReader tags )
            : base( parentAdvice, targetType, tags )
        {
            this.Name = name;

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
            var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

            var method =
                MethodDeclaration(
                    List<AttributeListSyntax>(),
                    this.GetSyntaxModifierList(),
                    context.SyntaxGenerator.ReturnType( this ),
                    this.ExplicitInterfaceImplementations.Count > 0
                        ? ExplicitInterfaceSpecifier( (NameSyntax) syntaxGenerator.Type( this.ExplicitInterfaceImplementations[0].DeclaringType.GetSymbol() ) )
                        : null,
                    Identifier( this.Name ),
                    context.SyntaxGenerator.TypeParameterList( this ),
                    context.SyntaxGenerator.ParameterList( this ),
                    context.SyntaxGenerator.ConstraintClauses( this ),
                    Block(
                        List(
                            !this.ReturnParameter.Type.Is( typeof(void) )
                                ? new[]
                                {
                                    ReturnStatement(
                                        Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( Whitespace( " " ) ),
                                        DefaultExpression( syntaxGenerator.Type( this.ReturnParameter.Type.GetSymbol() ) ),
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

                stringBuilder.Append( parameter.Type.ToDisplayString( format, context ) );
            }

            stringBuilder.Append( ")" );

            return stringBuilder.ToString();
        }

        public override IMember? OverriddenMember => (IMemberImpl?) this.OverriddenMethod;
    }
}