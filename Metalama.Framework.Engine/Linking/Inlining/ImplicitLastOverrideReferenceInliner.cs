using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Linking.Inlining
{
    internal class ImplicitLastOverrideReferenceInliner : Inliner
    {
        public static ImplicitLastOverrideReferenceInliner Instance { get; } = new ImplicitLastOverrideReferenceInliner();

        private ImplicitLastOverrideReferenceInliner()
        {
        }

        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            return true;
        }

        public override InliningAnalysisInfo GetInliningAnalysisInfo( InliningAnalysisContext context, ResolvedAspectReference aspectReference )
        {
            SyntaxNode body =
                aspectReference.ContainingSemantic.Symbol.GetPrimaryDeclaration() switch
                {
                    MethodDeclarationSyntax { Body : { } methodBody } => methodBody,
                    MethodDeclarationSyntax { ExpressionBody: { } methodBody } => methodBody,
                    AccessorDeclarationSyntax { Body: { } accessorBody } => accessorBody,
                    AccessorDeclarationSyntax { ExpressionBody: { } accessorBody } => accessorBody,
                    AccessorDeclarationSyntax { Body: null, ExpressionBody: null } accessor => accessor,
                    ArrowExpressionClauseSyntax arrowExpressionClause => arrowExpressionClause,
                    VariableDeclaratorSyntax { Parent: { Parent: EventFieldDeclarationSyntax } } eventFieldVariable => eventFieldVariable,
                    _ => throw new AssertionFailedException(),
                };

            return new InliningAnalysisInfo( body, null );
        }

        public override StatementSyntax Inline( SyntaxGenerationContext syntaxGenerationContext, InliningSpecification specification, SyntaxNode currentNode, StatementSyntax linkedTargetBody )
        {
            return linkedTargetBody;
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
