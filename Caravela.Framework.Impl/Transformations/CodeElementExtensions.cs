using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    public static class CodeElementExtensions
    {
        public static SyntaxTokenList GetSyntaxModifiers( this ICodeElement codeElement)
        {
            if (codeElement is Method method)
            {
                return ((BaseMethodDeclarationSyntax) method.Symbol.DeclaringSyntaxReferences.Single().GetSyntax()).Modifiers;
            }
            else if (codeElement is IMethod imethod)
            {
                return TokenList(
                    new SyntaxToken?[]
                    {
                        imethod.IsStatic ? Token( SyntaxKind.StaticKeyword ) : null
                    }.Where( x => x != null ).Select( x => x.Value )
                    );
            }

            throw new AssertionFailedException();
        }

        public static TypeSyntax GetSyntaxReturnType( this IMethod method )
        {
            return (TypeSyntax) CSharpSyntaxGenerator.Instance.TypeExpression( method.ReturnType.GetSymbol() );
        }

        public static TypeParameterListSyntax? GetSyntaxTypeParameterList( this IMethod method )
        {
            // TODO: generics
            return
                method.GenericParameters.Count > 0
                ? throw new NotImplementedException()
                : null;
        }

        public static ParameterListSyntax GetSyntaxParameterList( this IMethod method )
        {
            // TODO: generics
            return ParameterList(
                SeparatedList(
                    method.Parameters.Select(
                        p => Parameter(
                            List<AttributeListSyntax>(),
                            TokenList(), // TODO: modifiers
                            ParseTypeName( p.Type.ToDisplayString() ),
                            Identifier( p.Name! ),
                            null ) ) )
                );
        }

        public static SyntaxList<TypeParameterConstraintClauseSyntax> GetSyntaxConstraintClauses( this IMethod method )
        {
            // TODO: generics
            return List<TypeParameterConstraintClauseSyntax>();
        }
    }
}
