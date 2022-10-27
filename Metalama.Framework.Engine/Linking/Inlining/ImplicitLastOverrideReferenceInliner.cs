﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Linking.Inlining
{
    internal class ImplicitLastOverrideReferenceInliner : Inliner
    {
        public static ImplicitLastOverrideReferenceInliner Instance { get; } = new();

        private ImplicitLastOverrideReferenceInliner() { }

        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            return true;
        }

        public override InliningAnalysisInfo GetInliningAnalysisInfo( InliningAnalysisContext context, ResolvedAspectReference aspectReference )
        {
            SyntaxNode body =
                aspectReference.ContainingSemantic.Symbol.GetPrimaryDeclaration() switch
                {
                    MethodDeclarationSyntax { Body: { } methodBody } => methodBody,
                    MethodDeclarationSyntax { ExpressionBody: { } methodBody } => methodBody,
                    MethodDeclarationSyntax { Body: null, ExpressionBody: null } partialMethodDeclaration => partialMethodDeclaration,
                    DestructorDeclarationSyntax { Body: { } destructorBody } => destructorBody,
                    DestructorDeclarationSyntax { ExpressionBody: { } destructorBody } => destructorBody,
                    OperatorDeclarationSyntax { Body: { } operatorBody } => operatorBody,
                    OperatorDeclarationSyntax { ExpressionBody: { } operatorBody } => operatorBody,
                    ConversionOperatorDeclarationSyntax { Body: { } conversionOperatorBody } => conversionOperatorBody,
                    ConversionOperatorDeclarationSyntax { ExpressionBody: { } conversionOperatorBody } => conversionOperatorBody,
                    AccessorDeclarationSyntax { Body: { } accessorBody } => accessorBody,
                    AccessorDeclarationSyntax { ExpressionBody: { } accessorBody } => accessorBody,
                    AccessorDeclarationSyntax { Body: null, ExpressionBody: null } accessor => accessor,
                    ArrowExpressionClauseSyntax arrowExpressionClause => arrowExpressionClause,
                    VariableDeclaratorSyntax { Parent: { Parent: EventFieldDeclarationSyntax } } eventFieldVariable => eventFieldVariable,
                    ParameterSyntax { Parent: ParameterListSyntax { Parent: RecordDeclarationSyntax } } recordParameter => recordParameter,
                    _ => throw new AssertionFailedException( $"Declaration '{aspectReference.ContainingSemantic.Symbol}' has an unexpected declaration node." )
                };

            return new InliningAnalysisInfo( body, null );
        }

        public override StatementSyntax Inline(
            SyntaxGenerationContext syntaxGenerationContext,
            InliningSpecification specification,
            SyntaxNode currentNode,
            StatementSyntax linkedTargetBody )
        {
            return linkedTargetBody
                .WithLeadingTrivia( currentNode.GetLeadingTrivia().AddRange( linkedTargetBody.GetLeadingTrivia() ) )
                .WithTrailingTrivia( linkedTargetBody.GetTrailingTrivia().AddRange( currentNode.GetTrailingTrivia() ) );
        }

        public override bool IsValidForContainingSymbol( ISymbol symbol )
        {
            throw new NotSupportedException();
        }

        public override bool IsValidForTargetSymbol( ISymbol symbol )
        {
            throw new NotSupportedException();
        }
    }
}