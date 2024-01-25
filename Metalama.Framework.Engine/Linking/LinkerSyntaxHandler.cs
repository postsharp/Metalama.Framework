// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed class LinkerSyntaxHandler
    {
        private readonly LinkerInjectionRegistry _injectionRegistry;

        public LinkerSyntaxHandler( LinkerInjectionRegistry injectionRegistry )
        {
            this._injectionRegistry = injectionRegistry;
        }

        public SyntaxNode GetCanonicalRootNode( IMethodSymbol symbol )
        {
            var declaration = symbol.GetPrimaryDeclaration();

            if ( this._injectionRegistry.IsOverrideTarget( symbol ) )
            {
                switch ( declaration )
                {
                    case MethodDeclarationSyntax methodDecl:
                        // Partial methods without declared body have empty implicit body.
                        return methodDecl.Body ?? (SyntaxNode?) methodDecl.ExpressionBody ?? methodDecl;

                    case ConstructorDeclarationSyntax constructorDecl:
                        return (SyntaxNode?) constructorDecl.Body
                               ?? constructorDecl.ExpressionBody ?? throw new AssertionFailedException( $"'{symbol}' has no implementation." );

                    case DestructorDeclarationSyntax destructorDecl:
                        return (SyntaxNode?) destructorDecl.Body
                               ?? destructorDecl.ExpressionBody ?? throw new AssertionFailedException( $"'{symbol}' has no implementation." );

                    case OperatorDeclarationSyntax operatorDecl:
                        return (SyntaxNode?) operatorDecl.Body
                               ?? operatorDecl.ExpressionBody ?? throw new AssertionFailedException( $"'{symbol}' has no implementation." );

                    case ConversionOperatorDeclarationSyntax operatorDecl:
                        return (SyntaxNode?) operatorDecl.Body
                               ?? operatorDecl.ExpressionBody ?? throw new AssertionFailedException( $"'{symbol}' has no implementation." );

                    case AccessorDeclarationSyntax accessorDecl:
                        // Accessors with no body are auto-properties, in which case we have a substitution for the whole accessor declaration.
                        Invariant.Assert( !symbol.IsAbstract );

                        return accessorDecl.Body ?? (SyntaxNode?) accessorDecl.ExpressionBody ?? accessorDecl;

                    case ArrowExpressionClauseSyntax arrowExpressionClause:
                        // Expression-bodied property.
                        return arrowExpressionClause;

                    case VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: EventFieldDeclarationSyntax } } variableDecl:
                        // Event field accessors start replacement as variableDecls.
                        return variableDecl;

                    case ParameterSyntax parameterSyntax:
                        // Record positional property.
                        return parameterSyntax;

                    default:
                        throw new AssertionFailedException( $"Unexpected symbol: '{symbol}'." );
                }
            }

            if ( this._injectionRegistry.IsOverride( symbol ) )
            {
                switch ( declaration )
                {
                    case MethodDeclarationSyntax methodDecl:
                        return (SyntaxNode?) methodDecl.Body
                               ?? methodDecl.ExpressionBody ?? throw new AssertionFailedException( $"'{symbol}' has no implementation." );

                    case ConstructorDeclarationSyntax constructorDecl:
                        return (SyntaxNode?) constructorDecl.Body
                               ?? constructorDecl.ExpressionBody ?? throw new AssertionFailedException( $"'{symbol}' has no implementation." );

                    case AccessorDeclarationSyntax accessorDecl:
                        return (SyntaxNode?) accessorDecl.Body
                               ?? accessorDecl.ExpressionBody ?? throw new AssertionFailedException( $"'{symbol}' has no implementation." );

                    default:
                        throw new AssertionFailedException( $"Unexpected symbol: '{symbol}'." );
                }
            }

            throw new AssertionFailedException( $"'{symbol}' is not an override target." );
        }
    }
}