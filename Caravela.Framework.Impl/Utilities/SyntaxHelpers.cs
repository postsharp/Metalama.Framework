using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Caravela.Framework.Impl.Utilities
{
    internal static class SyntaxHelpers
    {
        public static TypeSyntax CreateSyntaxForEventType( IMethod method )
        {
            return LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( method.ReturnType.GetSymbol() );
        }

        public static TypeSyntax CreateSyntaxForPropertyType( IProperty property )
        {
            return LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( property.Type.GetSymbol() );
        }

        public static TypeSyntax CreateSyntaxForEventType( IEvent property )
        {
            return LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( property.EventType.GetSymbol() );
        }

        public static TypeParameterListSyntax? CreateSyntaxForTypeParameterList( IMethod method )
        {
            // TODO: generics
            return
                method.GenericParameters.Count > 0
                    ? throw new NotImplementedException()
                    : null;
        }

        public static ParameterListSyntax CreateSyntaxForParameterList( IMethodBase method )
        {
            // TODO: generics
            return SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(
                    method.Parameters.Select(
                        p => SyntaxFactory.Parameter(
                            SyntaxFactory.List<AttributeListSyntax>(),
                            DeclarationModifiersHelper.GetSyntaxModifierList( p ),
                            LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( CodeModelExtensions.GetSymbol( (IType) p.ParameterType ) ),
                            SyntaxFactory.Identifier( p.Name! ),
                            null ) ) ) );
        }

        public static SyntaxList<TypeParameterConstraintClauseSyntax> CreateSyntaxForConstraintClauses( IMethod method )
        {
            // TODO: generics
            return SyntaxFactory.List<TypeParameterConstraintClauseSyntax>();
        }
    }
}