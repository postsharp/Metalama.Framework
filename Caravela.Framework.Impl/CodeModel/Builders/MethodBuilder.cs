// unset

using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Transformations
{
    internal sealed class MethodBuilder : MemberBuilder, IMethodBuilder
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

        IParameterBuilder? IMethodBuilder.ReturnParameter => this.ReturnParameter;

        IType? IMethodBuilder.ReturnType
        {
            get => this.ReturnParameter?.Type;
            set
            {
                if ( this.ReturnParameter == null )
                {
                    throw new InvalidOperationException();
                }
                else if ( value == null )
                {
                    throw new ArgumentNullException( nameof(value) );
                }
                
                this.ReturnParameter.Type = value;
            }
        }

        IType IMethod.ReturnType => this.ReturnParameter?.Type;

        [Memo]
        public ParameterBuilder? ReturnParameter { get;}


        IParameter? IMethod.ReturnParameter => this.ReturnParameter;
        
        IReadOnlyList<IMethod> IMethod.LocalFunctions => this.LocalFunctions;
        IReadOnlyList<IParameter> IMethod.Parameters => this._parameters;

        IReadOnlyList<IGenericParameter> IMethod.GenericParameters => this._genericParameters;

     
        public IReadOnlyList<IMethod> LocalFunctions => Array.Empty<IMethod>();


        // We don't currently support adding other methods than default ones.
        public MethodKind MethodKind => MethodKind.Default;

        public override CodeElementKind ElementKind => CodeElementKind.Method;

        public MethodBuilder( IAdvice advice, INamedType targetType, IMethod templateMethod, string name )
            : base( advice, targetType )
        {
            this.Name = name;
        }

        // TODO: Memo for methods?
        private MemberDeclarationSyntax? _declaration;

        public override MemberDeclarationSyntax GetDeclaration()
        {
            /*
            if ( this._declaration != null )
            {
                return this._declaration;
            }

            var templateSyntax = (MethodDeclarationSyntax) this.TemplateMethod!.GetSyntaxNode()!;

            this._declaration = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.List<AttributeListSyntax>(), // TODO: Copy some attributes?
                templateSyntax.Modifiers,
                templateSyntax.ReturnType,
                templateSyntax.ExplicitInterfaceSpecifier!,
                templateSyntax.Identifier,
                templateSyntax.TypeParameterList!,
                templateSyntax.ParameterList,
                templateSyntax.ConstraintClauses,
                SyntaxFactory.Block( SyntaxFactory.ThrowStatement( SyntaxFactory.ObjectCreationExpression( SyntaxFactory.ParseTypeName( "System.NotImplementedException" ) ) ) ),
                null,
                templateSyntax.SemicolonToken );
*/
            return this._declaration;
        }

        public override CSharpSyntaxNode GetSyntaxNode() => this.GetDeclaration();

        public override IEnumerable<CSharpSyntaxNode> GetSyntaxNodes() => new[] { (CSharpSyntaxNode) this.GetDeclaration() };

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            throw new NotImplementedException();
        }

        public override bool Equals( ICodeElement other ) => throw new NotImplementedException();
        protected override void ForEachChild( Action<CodeElementBuilder> action ) => throw new NotImplementedException();
    }
}