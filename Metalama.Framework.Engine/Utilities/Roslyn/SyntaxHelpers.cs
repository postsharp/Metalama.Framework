// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
    internal static class SyntaxHelpers
    {
        public static BracketedParameterListSyntax WithAdditionalParameters(
            this BracketedParameterListSyntax parameterList,
            params (TypeSyntax Type, string Name)[] additionalParameters )
        {
            var additionalParameterSyntax =
                additionalParameters.SelectAsReadOnlyList(
                    p =>
                        Parameter(
                            List<AttributeListSyntax>(),
                            TokenList(),
                            p.Type,
                            Identifier( TriviaList( ElasticSpace ), p.Name, TriviaList( ElasticSpace ) ),
                            default ) );

            if ( parameterList.Parameters.Last().Modifiers.Any( m => m.IsKind( SyntaxKind.ParamsKeyword ) ) )
            {
                // Insert before params.

                return parameterList
                    .WithParameters(
                        parameterList.Parameters.InsertRange(
                            parameterList.Parameters.Count - 1,
                            additionalParameterSyntax ) );
            }
            else
            {
                // Insert last.

                return parameterList
                    .WithParameters( parameterList.Parameters.AddRange( additionalParameterSyntax ) );
            }
        }
    }
}