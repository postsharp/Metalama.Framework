// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal sealed class MethodBuilder : MemberBuilder, IMethodBuilder
    {
        public ParameterBuilderList Parameters { get; } = new();

        public GenericParameterBuilderList GenericParameters { get; } = new();

        IInvokerFactory<IMethodInvoker> IMethod.Invoker
            => throw new NotSupportedException( "Invokers are supported in build declarations but not in builders." );

        public IMethod? OverriddenMethod { get; set; }

        public MethodInfo ToMethodInfo() => throw new NotImplementedException();

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

        IParameterList IHasParameters.Parameters => this.Parameters;

        IGenericParameterList IMethod.GenericParameters => this.GenericParameters;

        IReadOnlyList<IType> IMethod.GenericArguments => ImmutableArray<IType>.Empty;

        bool IMethod.IsOpenGeneric => this.GenericParameters.Count > 0;

        // We don't currently support adding other methods than default ones.
        public MethodKind MethodKind => MethodKind.Default;

        System.Reflection.MethodBase IMethodBase.ToMethodBase() => this.ToMethodInfo();

        IMethod IMethod.WithGenericArguments( params IType[] genericArguments ) => throw new NotImplementedException();

        public override DeclarationKind DeclarationKind => DeclarationKind.Method;

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IMethod> ExplicitInterfaceImplementations => Array.Empty<IMethod>();

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
            var syntaxGenerator = LanguageServiceFactory.CSharpSyntaxGenerator;

            var method = (MethodDeclarationSyntax)
                syntaxGenerator.MethodDeclaration(
                    this.Name,
                    this.Parameters.AsBuilderList.Select( p => ((ParameterBuilder) p).ToDeclarationSyntax() ),
                    this.GenericParameters.AsBuilderList.Select( p => p.Name ),
                    syntaxGenerator.TypeExpression( this.ReturnParameter.ParameterType.GetSymbol() ),
                    this.Accessibility.ToRoslynAccessibility(),
                    this.ToDeclarationModifiers(),
                    !this.ReturnParameter.ParameterType.Is( typeof(void) )
                        ? new[]
                        {
                            ReturnStatement(
                                DefaultExpression( (TypeSyntax) syntaxGenerator.TypeExpression( this.ReturnParameter.ParameterType.GetSymbol() ) ) )
                        }
                        : null );

            return new[]
            {
                new IntroducedMember( this, method, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this.LinkerOptions, this )
            };
        }

        // TODO: Temporary
        public override MemberDeclarationSyntax InsertPositionNode
            => ((NamedType) this.DeclaringType).Symbol.DeclaringSyntaxReferences.Select( x => (TypeDeclarationSyntax) x.GetSyntax() ).First();
    }
}