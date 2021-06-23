// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// Helper methods that would ideally be in the <see cref="SyntaxFactory"/> class.
    /// </summary>
    internal static class SyntaxFactoryEx
    {
        public static LiteralExpressionSyntax Null => SyntaxFactory.LiteralExpression( SyntaxKind.NullLiteralExpression );

        public static LiteralExpressionSyntax LiteralExpression( object? obj )
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
                _ => throw new ArgumentOutOfRangeException()
            };
        
        public static LiteralExpressionSyntax LiteralExpression( string? s )
            => s == null ? Null : SyntaxFactory.LiteralExpression( SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal( s ) );

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

        public static StatementSyntax EmptyStatement
            => SyntaxFactory.ExpressionStatement( SyntaxFactory.IdentifierName( SyntaxFactory.MissingToken( SyntaxKind.IdentifierToken ) ) )
                .WithSemicolonToken( SyntaxFactory.MissingToken( SyntaxKind.SemicolonToken ) );

        public static SyntaxToken RefKindToken( RefKind refKind )
            => refKind switch
            {
                RefKind.In => SyntaxFactory.Token( SyntaxKind.InKeyword ),
                RefKind.Out => SyntaxFactory.Token( SyntaxKind.OutKeyword ),
                RefKind.Ref => SyntaxFactory.Token( SyntaxKind.RefKeyword ),
                _ => default
            };
    }
}