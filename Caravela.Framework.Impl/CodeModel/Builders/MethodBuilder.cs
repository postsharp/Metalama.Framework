using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Caravela.Framework.Code.MethodKind;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal sealed class MethodBuilder : MemberBuilder, IMethodBuilder, IMethodInternal, IMemberLink<IMethod>
    {
        public ParameterBuilderList Parameters { get; } = new();

        public GenericParameterBuilderList GenericParameters { get; } = new();

        public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, OptionalValue defaultValue = default )
        {
            var parameter = new ParameterBuilder( this, this.Parameters.Count, name, type, refKind );
            parameter.DefaultValue = defaultValue;
            this.Parameters.Add( parameter );
            return parameter;
        }

        public IGenericParameterBuilder AddGenericParameter( string name ) => throw new NotImplementedException();

        IParameterBuilder IMethodBuilder.ReturnParameter => this.ReturnParameter;

        IType IMethodBuilder.ReturnType
        {
            get => this.ReturnParameter.ParameterType;
            set
            {
                if ( this.ReturnParameter == null )
                {
                    throw new InvalidOperationException();
                }
                else if ( value == null )
                {
                    throw new ArgumentNullException( nameof( value ) );
                }

                this.ReturnParameter.ParameterType = value;
            }
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

        public MethodBuilder( Advice parentAdvice, INamedType targetType, string name )
            : base( parentAdvice, targetType, name )
        {
            this.ReturnParameter =
                new ParameterBuilder(
                    this,
                    -1,
                    null,
                    this.Compilation.Factory.GetTypeByReflectionType( typeof( void ) ).AssertNotNull(),
                    RefKind.None );
        }

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        // TODO: Implement compilation-consistent model.
        protected override ICodeElement GetForCompilation( CompilationModel compilation ) => this;

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
                    !this.ReturnParameter.ParameterType.Is( typeof( void ) )
                    ? new[]
                    {
                        ReturnStatement(
                            LiteralExpression(
                                SyntaxKind.DefaultLiteralExpression,
                                Token (SyntaxKind.DefaultKeyword)))
                    }
                    : null
                    );
            
            return new[] { new IntroducedMember( this, method, this.ParentAdvice.AspectPartId, IntroducedMemberSemantic.Introduction ) };
        }

        // TODO: Temporary
        public override MemberDeclarationSyntax InsertPositionNode => ((NamedType) this.DeclaringType).Symbol.DeclaringSyntaxReferences.SelectMany( x => ((TypeDeclarationSyntax) x.GetSyntax()).Members ).First();

        dynamic IMethodInvocation.Invoke( dynamic? instance, params dynamic[] args ) => throw new NotImplementedException();

        public IReadOnlyList<ISymbol> LookupSymbols()
        {
            // TODO: implement.
            return Array.Empty<ISymbol>();
        }

        IMethod ICodeElementLink<IMethod>.GetForCompilation( CompilationModel compilation ) => throw new NotImplementedException();
    }
}