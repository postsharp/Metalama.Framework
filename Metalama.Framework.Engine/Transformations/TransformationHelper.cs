// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Transformations
{
    internal static class TransformationHelper
    {
        public static BlockSyntax CreateIdentityAccessorBody( SyntaxKind accessorDeclarationKind, ExpressionSyntax proceedExpression )
        {
            switch ( accessorDeclarationKind )
            {
                case SyntaxKind.GetAccessorDeclaration:
                    return SyntaxFactoryEx.FormattedBlock(
                        SyntaxFactory.ReturnStatement(
                            SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.ReturnKeyword ),
                            proceedExpression,
                            SyntaxFactory.Token( SyntaxKind.SemicolonToken ) ) );

                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.InitAccessorDeclaration:
                    return SyntaxFactoryEx.FormattedBlock( SyntaxFactory.ExpressionStatement( proceedExpression ) );

                default:
                    throw new AssertionFailedException( $"Unexpected SyntaxKind: {accessorDeclarationKind}." );
            }
        }

        public static ExpressionSyntax CreatePropertyProceedGetExpression( 
            AspectReferenceSyntaxProvider aspectReferenceSyntaxProvider,
            SyntaxGenerationContext syntaxGenerationContext,
            IProperty targetProperty,
            AspectLayerId aspectLayer )
        => aspectReferenceSyntaxProvider.GetPropertyReference(
            aspectLayer,
            targetProperty,
            AspectReferenceTargetKind.PropertyGetAccessor,
            syntaxGenerationContext.SyntaxGenerator );

        public static ExpressionSyntax CreatePropertyProceedSetExpression(
            AspectReferenceSyntaxProvider aspectReferenceSyntaxProvider,
            SyntaxGenerationContext syntaxGenerationContext,
            IProperty targetProperty,
            AspectLayerId aspectLayer )
            => SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                aspectReferenceSyntaxProvider.GetPropertyReference(
                    aspectLayer,
                    targetProperty,
                    AspectReferenceTargetKind.PropertySetAccessor,
                    syntaxGenerationContext.SyntaxGenerator ),
                SyntaxFactory.IdentifierName( "value" ) );

        public static ExpressionSyntax CreateIndexerProceedGetExpression(
            AspectReferenceSyntaxProvider aspectReferenceSyntaxProvider,
            SyntaxGenerationContext syntaxGenerationContext,
            IIndexer targetIndexer,
            AspectLayerId aspectLayer )
        => aspectReferenceSyntaxProvider.GetIndexerReference(
            aspectLayer,
            targetIndexer,
            AspectReferenceTargetKind.PropertyGetAccessor,
            syntaxGenerationContext.SyntaxGenerator );

        public static ExpressionSyntax CreateIndexerProceedSetExpression(
            AspectReferenceSyntaxProvider aspectReferenceSyntaxProvider,
            SyntaxGenerationContext syntaxGenerationContext,
            IIndexer targetIndexer,
            AspectLayerId aspectLayer )
            => SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                aspectReferenceSyntaxProvider.GetIndexerReference(
                    aspectLayer,
                    targetIndexer,
                    AspectReferenceTargetKind.PropertySetAccessor,
                    syntaxGenerationContext.SyntaxGenerator ),
                SyntaxFactory.IdentifierName( "value" ) );

        public static BracketedParameterListSyntax GetIndexerOverrideParameterList( 
            CompilationModel compilation,
            SyntaxGenerationContext syntaxGenerationContext,
            IIndexer indexer,
            TypeSyntax additionalParameterType )
        {
            var originalParameterList = syntaxGenerationContext.SyntaxGenerator.ParameterList( indexer, compilation, removeDefaultValues: true );
            var overriddenByParameterType = additionalParameterType;

            return originalParameterList.WithAdditionalParameters( (overriddenByParameterType, AspectReferenceSyntaxProvider.LinkerOverrideParamName) );
        }
    }
}