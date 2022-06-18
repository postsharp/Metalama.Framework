﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.Templating
{
    /// <summary>
    /// Helper methods that would ideally be in the <see cref="SyntaxFactory"/> class.
    /// </summary>
    internal static partial class SyntaxFactoryEx
    {
        public static LiteralExpressionSyntax Null => SyntaxFactory.LiteralExpression( SyntaxKind.NullLiteralExpression );

        public static LiteralExpressionSyntax Default
            => SyntaxFactory.LiteralExpression(
                SyntaxKind.DefaultLiteralExpression,
                SyntaxFactory.Token( SyntaxKind.DefaultKeyword ) );

        public static SyntaxToken LiteralImpl<T>( T value, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => LiteralFormatter<T>.Instance.Format( value, options );

        public static ExpressionSyntax LiteralExpression( object? obj, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => LiteralExpressionOrNull( obj, options ) ?? throw new ArgumentOutOfRangeException( nameof(obj) );

        public static TypeSyntax ExpressionSyntaxType { get; } = SyntaxFactory.QualifiedName(
            SyntaxFactory.QualifiedName(
                SyntaxFactory.QualifiedName(
                    SyntaxFactory.QualifiedName(
                        SyntaxFactory.AliasQualifiedName(
                            SyntaxFactory.IdentifierName( SyntaxFactory.Token( SyntaxKind.GlobalKeyword ) ),
                            SyntaxFactory.IdentifierName( "Microsoft" ) ),
                        SyntaxFactory.IdentifierName( "CodeAnalysis" ) ),
                    SyntaxFactory.IdentifierName( "CSharp" ) ),
                SyntaxFactory.IdentifierName( "Syntax" ) ),
            SyntaxFactory.IdentifierName( "ExpressionSyntax" ) );

        public static TypeSyntax TypeSyntaxType { get; } = SyntaxFactory.QualifiedName(
            SyntaxFactory.QualifiedName(
                SyntaxFactory.QualifiedName(
                    SyntaxFactory.QualifiedName(
                        SyntaxFactory.AliasQualifiedName(
                            SyntaxFactory.IdentifierName( SyntaxFactory.Token( SyntaxKind.GlobalKeyword ) ),
                            SyntaxFactory.IdentifierName( "Microsoft" ) ),
                        SyntaxFactory.IdentifierName( "CodeAnalysis" ) ),
                    SyntaxFactory.IdentifierName( "CSharp" ) ),
                SyntaxFactory.IdentifierName( "Syntax" ) ),
            SyntaxFactory.IdentifierName( "TypeSyntax" ) );

        public static ExpressionSyntax? LiteralExpressionOrNull( object? obj, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => obj switch
            {
                byte b => LiteralExpression( (int) b, options ),
                sbyte b => LiteralExpression( (int) b, options ),
                string s => LiteralExpression( s, options ),
                char s => LiteralExpression( s, options ),
                int s => LiteralExpression( s, options ),
                uint s => LiteralExpression( s, options ),
                long s => LiteralExpression( s, options ),
                ulong s => LiteralExpression( s, options ),
                short s => LiteralExpression( (int) s, options ),
                ushort s => LiteralExpression( (int) s, options ),
                double s => LiteralExpression( s, options ),
                float s => LiteralExpression( s, options ),
                decimal s => LiteralExpression( s, options ),
                bool b => LiteralExpression( b, options ),
                _ => null
            };

        public static SyntaxToken LiteralTokenOrDefault( object obj, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => obj switch
            {
                byte b => LiteralImpl( (int) b, options ),
                sbyte b => LiteralImpl( (int) b, options ),
                string s => LiteralImpl( s, options ),
                char s => LiteralImpl( s, options ),
                int s => LiteralImpl( s, options ),
                uint s => LiteralImpl( s, options ),
                long s => LiteralImpl( s, options ),
                ulong s => LiteralImpl( s, options ),
                short s => LiteralImpl( (int) s, options ),
                ushort s => LiteralImpl( (int) s, options ),
                double s => LiteralImpl( s, options ),
                float s => LiteralImpl( s, options ),
                decimal s => LiteralImpl( s, options ),
                _ => default
            };

        public static ExpressionSyntax LiteralExpression( string? s, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => s == null
                ? SyntaxFactory.ParenthesizedExpression(
                        SyntaxFactory.CastExpression(
                            SyntaxFactory.NullableType( SyntaxFactory.PredefinedType( SyntaxFactory.Token( SyntaxKind.StringKeyword ) ) ),
                            SyntaxFactory.LiteralExpression( SyntaxKind.NullLiteralExpression ) ) )
                    .WithAdditionalAnnotations( Simplifier.Annotation )
                : LiteralNonNullExpression( s, options );

        public static LiteralExpressionSyntax LiteralNonNullExpression( string s, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => SyntaxFactory.LiteralExpression( SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal( s ) );

        public static LiteralExpressionSyntax LiteralExpression( int i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( i, options ) );

        public static LiteralExpressionSyntax LiteralExpression( uint i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( i, options ) );

        public static LiteralExpressionSyntax LiteralExpression( short i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( (int) i, options ) );

        public static LiteralExpressionSyntax LiteralExpression( ushort i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( (uint) i, options ) );

        public static LiteralExpressionSyntax LiteralExpression( long i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( i, options ) );

        public static LiteralExpressionSyntax LiteralExpression( ulong i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( i, options ) );

        public static LiteralExpressionSyntax LiteralExpression( float i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( i, options ) );

        public static LiteralExpressionSyntax LiteralExpression( double i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( i, options ) );

        public static LiteralExpressionSyntax LiteralExpression( decimal i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( i, options ) );

        public static LiteralExpressionSyntax LiteralExpression( char c, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => SyntaxFactory.LiteralExpression( SyntaxKind.CharacterLiteralExpression, LiteralImpl( c, options ) );

        public static LiteralExpressionSyntax LiteralExpression( bool b, ObjectDisplayOptions options = ObjectDisplayOptions.None )
            => SyntaxFactory.LiteralExpression( b ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression );

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
    }
}