// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Linq;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.Templating
{
    /// <summary>
    /// Helper methods that would ideally be in the <see cref="SyntaxFactory"/> class.
    /// </summary>
    public static class SyntaxFactoryEx
    {
        public static LiteralExpressionSyntax Null => SyntaxFactory.LiteralExpression( SyntaxKind.NullLiteralExpression );

        public static LiteralExpressionSyntax Default
            => SyntaxFactory.LiteralExpression(
                SyntaxKind.DefaultLiteralExpression,
                SyntaxFactory.Token( SyntaxKind.DefaultKeyword ) );

        public static ExpressionSyntax LiteralExpression( object? obj, bool addSuffix = false )
            => LiteralExpressionOrNull( obj, addSuffix ) ?? throw new ArgumentOutOfRangeException( nameof(obj) );

        public static ExpressionSyntax? LiteralExpressionOrNull( object? obj, bool suffix = false )
            => obj switch
            {
                string s => LiteralExpression( s ),
                char s => LiteralExpression( s ),
                int s => LiteralExpression( s ),
                uint s => LiteralExpression( s ),
                long s => LiteralExpression( s ),
                ulong s => LiteralExpression( s ),
                short s => LiteralExpression( s ),
                ushort s => LiteralExpression( s ),
                double s => LiteralExpression( s ),
                float s => LiteralExpression( s ),
                decimal s => LiteralExpression( s ),
                _ => null
            };

        public static SyntaxToken LiteralTokenOrDefault( object obj )
            => obj switch
            {
                string s => SyntaxFactory.Literal( s ),
                char s => SyntaxFactory.Literal( s ),
                int s => SyntaxFactory.Literal( s ),
                uint s => SyntaxFactory.Literal( s ),
                long s => SyntaxFactory.Literal( s ),
                ulong s => SyntaxFactory.Literal( s ),
                short s => SyntaxFactory.Literal( s ),
                ushort s => SyntaxFactory.Literal( s ),
                double s => SyntaxFactory.Literal( s ),
                float s => SyntaxFactory.Literal( s ),
                decimal s => SyntaxFactory.Literal( s ),
                _ => default
            };

        public static ExpressionSyntax LiteralExpression( string? s )
            => s == null
                ? SyntaxFactory.ParenthesizedExpression(
                        SyntaxFactory.CastExpression(
                            SyntaxFactory.NullableType( SyntaxFactory.PredefinedType( SyntaxFactory.Token( SyntaxKind.StringKeyword ) ) ),
                            SyntaxFactory.LiteralExpression( SyntaxKind.NullLiteralExpression ) ) )
                    .WithAdditionalAnnotations( Simplifier.Annotation )
                : LiteralNonNullExpression( s );

        public static LiteralExpressionSyntax LiteralNonNullExpression( string s )
            => SyntaxFactory.LiteralExpression( SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal( s ) );

        public static LiteralExpressionSyntax LiteralExpression( int i )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( uint i )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( short i )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( ushort i )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( long i )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( ulong i )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( float i )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( double i )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( decimal i )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( char c )
            => SyntaxFactory.LiteralExpression( SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal( c ) );

        private static ExpressionSyntax EmptyExpression => SyntaxFactory.IdentifierName( SyntaxFactory.MissingToken( SyntaxKind.IdentifierToken ) );

        public static StatementSyntax EmptyStatement
            => SyntaxFactory.ExpressionStatement( EmptyExpression )
                .WithSemicolonToken( SyntaxFactory.MissingToken( SyntaxKind.SemicolonToken ) );

        public static IdentifierNameSyntax DiscardToken
            => SyntaxFactory.IdentifierName(
                SyntaxFactory.Identifier(
                    default,
                    SyntaxKind.UnderscoreToken,
                    "_",
                    "_",
                    default ) );

        public static SyntaxToken RefKindToken( RefKind refKind )
            => refKind switch
            {
                RefKind.In => SyntaxFactory.Token( SyntaxKind.InKeyword ),
                RefKind.Out => SyntaxFactory.Token( SyntaxKind.OutKeyword ),
                RefKind.Ref => SyntaxFactory.Token( SyntaxKind.RefKeyword ),
                _ => default
            };

        /// <summary>
        /// Generates a string that contains C# code that instantiates the given node
        /// using SyntaxFactory. Used for debugging.
        /// </summary>
        public static string ToSyntaxFactoryDebug( this SyntaxNode node, Compilation compilation, IServiceProvider serviceProvider )
        {
            MetaSyntaxRewriter rewriter = new( serviceProvider, compilation, RoslynApiVersion.Current );
            var normalized = NormalizeRewriter.Instance.Visit( node );
            var transformedNode = rewriter.Visit( normalized );

            return transformedNode.ToFullString();
        }

        private class NormalizeRewriter : CSharpSyntaxRewriter
        {
            public static readonly NormalizeRewriter Instance = new();

            private NormalizeRewriter() : base( true ) { }

            public override SyntaxNode? VisitQualifiedName( QualifiedNameSyntax node )
            {
                if ( node.Parent == null )
                {
                    throw new AssertionFailedException();
                }

                // The following list of exceptions is incomplete. If you get into an InvalidCastException in the rewriter, you have to extend it.
                if ( !node.AncestorsAndSelf()
                        .Any(
                            a =>
                                a is GenericNameSyntax or UsingDirectiveSyntax ||
                                (a.Parent is NamespaceDeclarationSyntax namespaceDeclaration && namespaceDeclaration.Name == a) ||
                                (a.Parent is FileScopedNamespaceDeclarationSyntax fileScopeNamespaceDeclaration && fileScopeNamespaceDeclaration.Name == a) ||
                                (a.Parent is MethodDeclarationSyntax methodDeclaration && methodDeclaration.ReturnType == a) ||
                                (a.Parent is VariableDeclarationSyntax variable && variable.Type == a) ||
                                (a.Parent is TypeConstraintSyntax typeConstraint && typeConstraint.Type == a) ||
                                (a.Parent is ArrayTypeSyntax arrayType && arrayType.ElementType == a) ||
                                (a.Parent is ObjectCreationExpressionSyntax objectCreation && objectCreation.Type == a) ||
                                (a.Parent is DefaultExpressionSyntax defaultExpression && defaultExpression.Type == a) ||
                                (a.Parent is CastExpressionSyntax castExpression && castExpression.Type == a) ||
                                (a.Parent is ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier && explicitInterfaceSpecifier.Name == a) ||
                                (a.Parent is ParameterSyntax parameter && parameter.Type == a) ||
                                a.Parent is SimpleBaseTypeSyntax ) )
                {
                    return SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        (ExpressionSyntax) this.Visit( node.Left ),
                        node.DotToken,
                        node.Right );
                }
                else
                {
                    return base.VisitQualifiedName( node );
                }
            }

            public override SyntaxNode? VisitXmlComment( XmlCommentSyntax node ) => null;

            public override SyntaxNode? VisitDocumentationCommentTrivia( DocumentationCommentTriviaSyntax node ) => null;

            public override SyntaxTrivia VisitTrivia( SyntaxTrivia trivia )
            {
                switch ( trivia.Kind() )
                {
                    case SyntaxKind.SingleLineCommentTrivia:
                    case SyntaxKind.MultiLineCommentTrivia:
                        return default;

                    default:
                        return trivia;
                }
            }

            public override SyntaxNode? VisitPragmaWarningDirectiveTrivia( PragmaWarningDirectiveTriviaSyntax node ) => null;
        }
    }
}