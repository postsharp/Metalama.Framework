// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace Caravela.Framework.Impl.Serialization
{
    internal class IntrinsicsCaller
    {
        /// <summary>
        /// Returns <c>Caravela.Compiler.Intrinsics.methodName(documentationId)</c>.
        /// </summary>
        /// <param name="methodName">GetRuntimeMethodHandle, GetRuntimeFieldHandle, or GetRuntimeTypeHandle.</param>
        /// <param name="documentationId">The string to pass to the method.</param>
        /// <returns>Roslyn expression that represents the invocation of the method. The type of the expression is a metadata token.</returns>
        public static InvocationExpressionSyntax CreateLdTokenExpression( string methodName, string documentationId )
        {
            // For Instrinsics, we cannot use the SyntaxFactory because we cannot get a runtime Type for it, because
            // we only have a reference assembly, not an implementation assembly.
            return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.AliasQualifiedName(
                                        SyntaxFactory.IdentifierName( SyntaxFactory.Token( SyntaxKind.GlobalKeyword ) ),
                                        SyntaxFactory.IdentifierName( "Caravela" ) ),
                                    SyntaxFactory.IdentifierName( "Compiler" ) ),
                                SyntaxFactory.IdentifierName( "Intrinsics" ) ),
                            SyntaxFactory.IdentifierName( methodName ) )
                        .WithAdditionalAnnotations( Simplifier.Annotation ) )
                .AddArgumentListArguments(
                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal( documentationId ) ) ) );
        }
    }
}