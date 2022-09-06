// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.Formatting;
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
        private bool _isReadOnly;

        public ParameterBuilderList Parameters { get; } = new();

        public GenericParameterBuilderList TypeParameters { get; } = new();

        public bool IsReadOnly
        {
            get => this._isReadOnly;
            set
            {
                this.CheckNotFrozen();

                this._isReadOnly = value;
            }
        }

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

        public override void Freeze()
        {
            base.Freeze();

            foreach ( var parameter in this.Parameters )
            {
                parameter.Freeze();
            }

            foreach ( var typeParameter in this.TypeParameters )
            {
                typeParameter.Freeze();
            }

            this.ReturnParameter.Freeze();
        }

        public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = null )
        {
            this.CheckNotFrozen();

            var parameter = new ParameterBuilder( this.ParentAdvice, this, this.Parameters.Count, name, type, refKind );
            parameter.DefaultValue = defaultValue;
            this.Parameters.Add( parameter );

            return parameter;
        }

        public IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = null )
        {
            this.CheckNotFrozen();

            var iType = this.Compilation.Factory.GetTypeByReflectionType( type );
            var typeConstant = defaultValue != null ? new TypedConstant( iType, defaultValue ) : default;

            return this.AddParameter( name, iType, refKind, typeConstant );
        }

        public ITypeParameterBuilder AddTypeParameter( string name )
        {
            this.CheckNotFrozen();

            var builder = new TypeParameterBuilder( this, this.TypeParameters.Count, name );
            this.TypeParameters.Add( builder );

            return builder;
        }

        IParameterBuilder IMethodBuilder.ReturnParameter => this.ReturnParameter;

        public IType ReturnType
        {
            get => this.ReturnParameter.Type;
            set
            {
                this.CheckNotFrozen();

                this.ReturnParameter.Type = value ?? throw new ArgumentNullException( nameof(value) );
            }
        }

        IType IMethod.ReturnType => this.ReturnParameter.Type;

        public ParameterBuilder ReturnParameter { get; }

        IParameter IMethod.ReturnParameter => this.ReturnParameter;

        IParameterList IHasParameters.Parameters => this.Parameters;

        IGenericParameterList IGeneric.TypeParameters => this.TypeParameters;

        public bool IsOpenGeneric => this.TypeParameters.Count > 0 || this.DeclaringType.IsOpenGeneric;

        public bool IsGeneric => this.TypeParameters.Count > 0;

        // We don't currently support adding other methods than default ones.
        public MethodKind MethodKind
            => this.DeclarationKind switch
            {
                DeclarationKind.Method => MethodKind.Default,
                DeclarationKind.Operator => MethodKind.Operator,
                DeclarationKind.Finalizer => MethodKind.Finalizer,
                _ => throw new AssertionFailedException()
            };

        System.Reflection.MethodBase IMethodBase.ToMethodBase() => this.ToMethodInfo();

        IGeneric IGenericInternal.ConstructGenericInstance( params IType[] typeArguments ) => throw new NotImplementedException();

        public override DeclarationKind DeclarationKind { get; }

        public OperatorKind OperatorKind { get; }

        public IReadOnlyList<IMethod> ExplicitInterfaceImplementations { get; private set; } = Array.Empty<IMethod>();

        public MethodBuilder(
            Advice parentAdvice,
            INamedType targetType,
            string name,
            DeclarationKind declarationKind = DeclarationKind.Method,
            OperatorKind operatorKind = OperatorKind.None )
            : base( parentAdvice, targetType, name )
        {
            Invariant.Assert(
                declarationKind == DeclarationKind.Operator
                                ==
                                (operatorKind != OperatorKind.None) );

            this.Name = name;
            this.DeclarationKind = declarationKind;
            this.OperatorKind = operatorKind;

            this.ReturnParameter =
                new ParameterBuilder(
                    this.ParentAdvice,
                    this,
                    -1,
                    null,
                    this.Compilation.Factory.GetTypeByReflectionType( typeof(void) ).AssertNotNull(),
                    RefKind.None );
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( MemberIntroductionContext context )
        {
            if ( this.DeclarationKind == DeclarationKind.Finalizer )
            {
                var syntax =
                    DestructorDeclaration(
                        List<AttributeListSyntax>(),
                        TokenList(),
                        ((TypeDeclarationSyntax) this.DeclaringType.GetPrimaryDeclarationSyntax().AssertNotNull()).Identifier,
                        ParameterList(),
                        Block().WithGeneratedCodeAnnotation( this.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation ),
                        null );

                return new[] { new IntroducedMember( this, syntax, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ) };
            }
            else if ( this.DeclarationKind == DeclarationKind.Operator )
            {
                if ( this.OperatorKind.GetCategory() == OperatorCategory.Conversion )
                {
                    Invariant.Assert( this.Parameters.Count == 1 );

                    var syntax =
                        ConversionOperatorDeclaration(
                            this.GetAttributeLists( context )
                                .AddRange( this.ReturnParameter.GetAttributeLists( context ) ),
                            TokenList( Token( SyntaxKind.PublicKeyword ), Token( SyntaxKind.StaticKeyword ) ),
                            this.OperatorKind.ToOperatorKeyword(),
                            context.SyntaxGenerator.Type( this.ReturnType.GetSymbol().AssertNotNull() ),
                            context.SyntaxGenerator.ParameterList( this, context.Compilation ),
                            null,
                            ArrowExpressionClause( context.SyntaxGenerator.DefaultExpression( this.ReturnType.GetSymbol().AssertNotNull() ) ) );

                    return new[] { new IntroducedMember( this, syntax, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ) };
                }
                else
                {
                    Invariant.Assert( this.Parameters.Count is 1 or 2 );

                    var syntax =
                        OperatorDeclaration(
                            this.GetAttributeLists( context )
                                .AddRange( this.ReturnParameter.GetAttributeLists( context ) ),
                            TokenList( Token( SyntaxKind.PublicKeyword ), Token( SyntaxKind.StaticKeyword ) ),
                            context.SyntaxGenerator.Type( this.ReturnType.GetSymbol().AssertNotNull() ),
                            this.OperatorKind.ToOperatorKeyword(),
                            context.SyntaxGenerator.ParameterList( this, context.Compilation ),
                            null,
                            ArrowExpressionClause( context.SyntaxGenerator.DefaultExpression( this.ReturnType.GetSymbol().AssertNotNull() ) ) );

                    return new[] { new IntroducedMember( this, syntax, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ) };
                }
            }
            else
            {
                var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

                var method =
                    MethodDeclaration(
                        this.GetAttributeLists( context )
                            .AddRange( this.ReturnParameter.GetAttributeLists( context ) ),
                        this.GetSyntaxModifierList(),
                        context.SyntaxGenerator.ReturnType( this ),
                        this.ExplicitInterfaceImplementations.Count > 0
                            ? ExplicitInterfaceSpecifier(
                                (NameSyntax) syntaxGenerator.Type( this.ExplicitInterfaceImplementations[0].DeclaringType.GetSymbol() ) )
                            : null,
                        this.GetCleanName(),
                        context.SyntaxGenerator.TypeParameterList( this, context.Compilation ),
                        context.SyntaxGenerator.ParameterList( this, context.Compilation ),
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
        }

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