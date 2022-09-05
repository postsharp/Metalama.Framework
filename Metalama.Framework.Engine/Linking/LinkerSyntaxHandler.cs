// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using Metalama.Framework.Engine.Utilities.Roslyn;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal class LinkerSyntaxHandler
    {
        private readonly LinkerIntroductionRegistry _introductionRegistry;

        public LinkerSyntaxHandler(LinkerIntroductionRegistry introductionRegistry )
        {
            this._introductionRegistry = introductionRegistry;
        }

        public SyntaxNode GetCanonicalRootNode( IMethodSymbol symbol )
        {
            var declaration = symbol.GetPrimaryDeclaration();

            if ( this._introductionRegistry.IsOverrideTarget( symbol ) )
            {
                switch ( declaration )
                {
                    case MethodDeclarationSyntax methodDecl:
                        // Partial methods without declared body have empty implicit body.
                        return methodDecl.Body ?? (SyntaxNode?) methodDecl.ExpressionBody ?? Block();

                    case DestructorDeclarationSyntax destructorDecl:
                        return (SyntaxNode?) destructorDecl.Body ?? destructorDecl.ExpressionBody ?? throw new AssertionFailedException();

                    case OperatorDeclarationSyntax operatorDecl:
                        return (SyntaxNode?) operatorDecl.Body ?? operatorDecl.ExpressionBody ?? throw new AssertionFailedException();

                    case ConversionOperatorDeclarationSyntax operatorDecl:
                        return (SyntaxNode?) operatorDecl.Body ?? operatorDecl.ExpressionBody ?? throw new AssertionFailedException();

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
                        throw new AssertionFailedException();
                }
            }

            if ( this._introductionRegistry.IsOverride( symbol ) )
            {
                switch ( declaration )
                {
                    case MethodDeclarationSyntax methodDecl:
                        return (SyntaxNode?) methodDecl.Body ?? methodDecl.ExpressionBody ?? throw new AssertionFailedException();

                    case AccessorDeclarationSyntax accessorDecl:
                        return (SyntaxNode?) accessorDecl.Body ?? accessorDecl.ExpressionBody ?? throw new AssertionFailedException();

                    default:
                        throw new AssertionFailedException();
                }
            }

            throw new AssertionFailedException();
        }
    }
}