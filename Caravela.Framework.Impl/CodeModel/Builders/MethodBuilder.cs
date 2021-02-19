using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Caravela.Framework.Code.MethodKind;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal sealed class MethodBuilder : MemberBuilder, IMethodBuilder, IMethodInternal
    {
        private readonly List<ParameterBuilder> _parameters = new List<ParameterBuilder>();

        private readonly List<GenericParameterBuilder> _genericParameters = new List<GenericParameterBuilder>();

        public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, OptionalValue optionalValue = default )
        {
            var parameter = new ParameterBuilder( this, this._parameters.Count, name, type, refKind );
            parameter.DefaultValue = optionalValue;
            this._parameters.Add( parameter );
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

        IReadOnlyList<IMethod> IMethodBase.LocalFunctions => this.LocalFunctions;

        IReadOnlyList<IParameter> IMethodBase.Parameters => this._parameters;

        IReadOnlyList<IGenericParameter> IMethod.GenericParameters => this._genericParameters;

        IReadOnlyList<IType> IMethod.GenericArguments => ImmutableArray<IType>.Empty;

        bool IMethod.IsOpenGeneric => true;

        public IReadOnlyList<IMethod> LocalFunctions => Array.Empty<IMethod>();

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

        public override bool Equals( ICodeElement other ) => throw new NotImplementedException();

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var syntaxGenerator = this.Compilation.SyntaxGenerator;

            var method = (MethodDeclarationSyntax)
                syntaxGenerator.MethodDeclaration(
                    this.Name,
                    this._parameters.Select( p => p.ToDeclarationSyntax() ),
                    this._genericParameters.Select( p => p.Name ),
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
        public override MemberDeclarationSyntax InsertPositionNode => ((NamedType) this.DeclaringType).Symbol.DeclaringSyntaxReferences.Select( x => (TypeDeclarationSyntax)x.GetSyntax() ).FirstOrDefault();
        
        dynamic IMethodInvocation.Invoke( dynamic? instance, params dynamic[] args ) => throw new NotImplementedException();

        public IReadOnlyList<ISymbol> LookupSymbols()
        {
            // TODO: implement.
            return Array.Empty<ISymbol>();
        }
    }
}