// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{

    /// <summary>
    /// Additional methods that could belong to <see cref="SyntaxFactory"/>.
    /// </summary>
    internal static class SyntaxFactoryEx
    {
        public static LiteralExpressionSyntax LiteralExpression( string s ) => SyntaxFactory.LiteralExpression( SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal( s ) );

        public static LiteralExpressionSyntax LiteralExpression( char c ) => SyntaxFactory.LiteralExpression( SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal( c ) );

        public static LiteralExpressionSyntax LiteralExpression( int i ) => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( uint i ) => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( long i ) => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( ulong i ) => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( short i ) => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( ushort i ) => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( double i ) => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( float i ) => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );

        public static LiteralExpressionSyntax LiteralExpression( decimal i ) => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( i ) );
    }
}