// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.Templating;

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

    private static SyntaxToken LiteralImpl<T>( T value, ObjectDisplayOptions options = ObjectDisplayOptions.None )
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

    public static ExpressionSyntax? LiteralExpressionOrNull( object? obj, ObjectDisplayOptions options = ObjectDisplayOptions.None )
        => obj switch
        {
            null => Null,
            byte b => LiteralExpression( (int) b, options ),
            sbyte b => LiteralExpression( (int) b, options ),
            string s => LiteralExpression( s ),
            char s => LiteralExpression( s, options ),
            int s => LiteralExpression( s, options ),
            uint s => LiteralExpression( s, options ),
            long s => LiteralExpression( s, options ),
            ulong s => LiteralExpression( s, options ),
            short s => LiteralExpression( (int) s, options ),
            ushort s => LiteralExpression( (int) s, options ),
            double s => LiteralExpression( s, options ),
            float s => LiteralExpression( s, options ),

            // force type suffix for decimal, since code like "decimal d = 3.14;" is not valid
            decimal s => LiteralExpression( s, options | ObjectDisplayOptions.IncludeTypeSuffix ),
            bool b => LiteralExpression( b ),
            _ => null
        };

    public static ExpressionSyntax LiteralExpression( string? s )
        => s == null
            ? SyntaxFactory.ParenthesizedExpression(
                    SyntaxFactory.CastExpression(
                        SyntaxFactory.NullableType( SyntaxFactory.PredefinedType( SyntaxFactory.Token( SyntaxKind.StringKeyword ) ) ),
                        SyntaxFactory.LiteralExpression( SyntaxKind.NullLiteralExpression ) ) )
                .WithAdditionalAnnotations( Simplifier.Annotation )
            : LiteralNonNullExpression( s );

    [PublicAPI]
    public static LiteralExpressionSyntax LiteralNonNullExpression( string s )
        => SyntaxFactory.LiteralExpression( SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal( s ) );

    [PublicAPI]
    public static LiteralExpressionSyntax LiteralExpression( int i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
        => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( i, options ) );

    [PublicAPI]
    public static LiteralExpressionSyntax LiteralExpression( uint i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
        => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( i, options ) );

    [PublicAPI]
    public static LiteralExpressionSyntax LiteralExpression( short i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
        => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( (int) i, options ) );

    [PublicAPI]
    public static LiteralExpressionSyntax LiteralExpression( ushort i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
        => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( (uint) i, options ) );

    [PublicAPI]
    public static LiteralExpressionSyntax LiteralExpression( long i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
        => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( i, options ) );

    [PublicAPI]
    public static LiteralExpressionSyntax LiteralExpression( ulong i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
        => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( i, options ) );

    [PublicAPI]
    public static LiteralExpressionSyntax LiteralExpression( float i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
        => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( i, options ) );

    [PublicAPI]
    public static LiteralExpressionSyntax LiteralExpression( double i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
        => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( i, options ) );

    [PublicAPI]
    public static LiteralExpressionSyntax LiteralExpression( decimal i, ObjectDisplayOptions options = ObjectDisplayOptions.None )
        => SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, LiteralImpl( i, options ) );

    [PublicAPI]
    public static LiteralExpressionSyntax LiteralExpression( char c, ObjectDisplayOptions options = ObjectDisplayOptions.None )
        => SyntaxFactory.LiteralExpression( SyntaxKind.CharacterLiteralExpression, LiteralImpl( c, options ) );

    [PublicAPI]
    public static LiteralExpressionSyntax LiteralExpression( bool b )
        => SyntaxFactory.LiteralExpression( b ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression );

    private static ExpressionSyntax EmptyExpression => SyntaxFactory.IdentifierName( SyntaxFactory.MissingToken( SyntaxKind.IdentifierToken ) );

    public static StatementSyntax EmptyStatement
        => SyntaxFactory.ExpressionStatement( EmptyExpression )
            .WithSemicolonToken( SyntaxFactory.MissingToken( SyntaxKind.SemicolonToken ) );

    public static CastExpressionSyntax SafeCastExpression( TypeSyntax type, ExpressionSyntax syntax )
    {
        if ( syntax is CastExpressionSyntax cast && cast.Type.IsEquivalentTo( type, topLevel: false ) )
        {
            // It's already a cast to the same type, no need to cast again.
            return cast;
        }

        var requiresParenthesis = syntax switch
        {
            CastExpressionSyntax => false,
            InvocationExpressionSyntax => false,
            MemberAccessExpressionSyntax => false,
            ElementAccessExpressionSyntax => false,
            IdentifierNameSyntax => false,
            LiteralExpressionSyntax => false,
            DefaultExpressionSyntax => false,
            TypeOfExpressionSyntax => false,
            ParenthesizedExpressionSyntax => false,
            ConditionalAccessExpressionSyntax => false,
            ObjectCreationExpressionSyntax => false,
            ArrayCreationExpressionSyntax => false,
            PostfixUnaryExpressionSyntax => false,

            // The syntax (T)-x is ambiguous and interpreted as binary minus, not cast of unary minus.
            PrefixUnaryExpressionSyntax { RawKind: not (int) SyntaxKind.UnaryMinusExpression } => false,
            TupleExpressionSyntax => false,
            ThisExpressionSyntax => false,
            _ => true
        };

        if ( requiresParenthesis )
        {
            return SyntaxFactory.CastExpression( type, SyntaxFactory.ParenthesizedExpression( syntax ).WithAdditionalAnnotations( Simplifier.Annotation ) )
                .WithAdditionalAnnotations( Simplifier.Annotation );
        }
        else
        {
            return SyntaxFactory.CastExpression( type, syntax ).WithAdditionalAnnotations( Simplifier.Annotation );
        }
    }

    public static BlockSyntax FormattedBlock( params StatementSyntax[] statements ) => FormattedBlock( (IEnumerable<StatementSyntax>) statements );

    private static bool NeedsLineFeed( StatementSyntax statement )
        => !statement.HasTrailingTrivia || !statement.GetTrailingTrivia()[^1].IsKind( SyntaxKind.EndOfLineTrivia );

    public static BlockSyntax FormattedBlock( IEnumerable<StatementSyntax> statements )
        => SyntaxFactory.Block(
            SyntaxFactory.Token( SyntaxKind.OpenBraceToken ).WithTrailingTrivia( SyntaxFactory.ElasticLineFeed ),
            SyntaxFactory.List(
                statements.Select(
                    s => NeedsLineFeed( s )
                        ? s.WithTrailingTrivia( s.GetTrailingTrivia().Add( SyntaxFactory.ElasticLineFeed ) )
                        : s ) ),
            SyntaxFactory.Token( SyntaxKind.CloseBraceToken ).WithLeadingTrivia( SyntaxFactory.ElasticLineFeed ) );

    public static ExpressionStatementSyntax DiscardStatement( ExpressionSyntax discardedExpression )
        => SyntaxFactory.ExpressionStatement(
            SyntaxFactory.AssignmentExpression( SyntaxKind.SimpleAssignmentExpression, DiscardIdentifier(), discardedExpression ) );

    public static IdentifierNameSyntax DiscardIdentifier()
        => SyntaxFactory.IdentifierName(
            SyntaxFactory.Identifier(
                SyntaxFactory.TriviaList(),
                SyntaxKind.UnderscoreToken,
                "_",
                "_",
                SyntaxFactory.TriviaList() ) );

    public static IdentifierNameSyntax VarIdentifier()
        => SyntaxFactory.IdentifierName(
            SyntaxFactory.Identifier(
                SyntaxFactory.TriviaList(),
                SyntaxKind.VarKeyword,
                "var",
                "var",
                SyntaxFactory.TriviaList( SyntaxFactory.ElasticSpace ) ) );

    public static ExpressionSyntax ParseExpressionSafe( string text )
    {
        var expression = SyntaxFactory.ParseExpression( text );

        var diagnostics = expression.GetDiagnostics().ToArray();

        if ( diagnostics.HasError() )
        {
            throw new DiagnosticException( $"Code '{text}' could not be parsed as an expression.", diagnostics.ToImmutableArray(), inSourceCode: false );
        }

        return expression;
    }

    public static StatementSyntax ParseStatementSafe( string text )
    {
        var statement = SyntaxFactory.ParseStatement( text );

        var diagnostics = statement.GetDiagnostics();
        var enumerable = diagnostics as Diagnostic[] ?? diagnostics.ToArray();

        if ( enumerable.HasError() )
        {
            throw new DiagnosticException( $"Code could not be parsed as a statement.", enumerable.ToImmutableArray(), inSourceCode: false );
        }

        return statement;
    }

    internal static SyntaxToken InvocationRefKindToken( this Code.RefKind refKind )
        => refKind switch
        {
            Code.RefKind.None or Code.RefKind.In => default,
            Code.RefKind.Out => SyntaxFactory.Token( SyntaxKind.OutKeyword ),
            Code.RefKind.Ref => SyntaxFactory.Token( SyntaxKind.RefKeyword ),
            Code.RefKind.RefReadOnly => SyntaxFactory.Token( SyntaxKind.InKeyword ),
            _ => throw new AssertionFailedException( $"Unexpected RefKind: {refKind}." )
        };

    public static ObjectCreationExpressionSyntax ObjectCreationExpression( TypeSyntax type, ArgumentListSyntax? arguments, InitializerExpressionSyntax? initializer = null )
        => SyntaxFactory.ObjectCreationExpression( SyntaxFactory.Token( default, SyntaxKind.NewKeyword, new( SyntaxFactory.ElasticSpace ) ), type, arguments, initializer );

    public static ObjectCreationExpressionSyntax ObjectCreationExpression( TypeSyntax type, ArgumentSyntax argument, InitializerExpressionSyntax? initializer = null )
        => ObjectCreationExpression( type, SyntaxFactory.ArgumentList( SyntaxFactory.SingletonSeparatedList( argument ) ), initializer );

    public static ObjectCreationExpressionSyntax ObjectCreationExpression( TypeSyntax type, params ArgumentSyntax[] arguments )
        => ObjectCreationExpression( type, SyntaxFactory.ArgumentList( SyntaxFactory.SeparatedList( arguments ) ) );

    public static ArrayTypeSyntax ArrayType( TypeSyntax elementType )
        => SyntaxFactory.ArrayType(
            elementType,
            SyntaxFactory.SingletonList(
                SyntaxFactory.ArrayRankSpecifier( SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>( SyntaxFactory.OmittedArraySizeExpression() ) ) ) );

    [return: NotNullIfNotNull( nameof(node) )]
    public static T? AddTrailingSpaceIfNecessary<T>( T? node )
        where T : CSharpSyntaxNode
    {
        if ( node?.GetTrailingTrivia().FullSpan.Length == 0 )
        {
            node = node.WithTrailingTrivia( SyntaxFactory.ElasticSpace );
        }

        return node;
    }

    public static SyntaxToken AddTrailingSpaceIfNecessary( SyntaxToken token )
    {
        if ( token.TrailingTrivia.FullSpan.Length == 0 )
        {
            token = token.WithTrailingTrivia( SyntaxFactory.ElasticSpace );
        }

        return token;
    }

#pragma warning disable RS0030 // Do not use banned APIs

    public static ParameterSyntax Parameter( SyntaxTokenList modifiers, TypeSyntax? type, SyntaxToken identifier, EqualsValueClauseSyntax? @default )
        => Parameter( default, modifiers, type, identifier, @default );

    public static ParameterSyntax Parameter(
        SyntaxList<AttributeListSyntax> attributeLists,
        SyntaxTokenList modifiers,
        TypeSyntax? type,
        SyntaxToken identifier,
        EqualsValueClauseSyntax? @default )
    {
        type = AddTrailingSpaceIfNecessary( type );

        return SyntaxFactory.Parameter( attributeLists, modifiers, type, identifier, @default );
    }

    private static SyntaxToken KeywordTokenWithSpace( SyntaxKind kind )
        => SyntaxFactory.Token( default, kind, new( SyntaxFactory.ElasticSpace ) );

    public static ThrowExpressionSyntax ThrowExpression( ExpressionSyntax expression )
        => SyntaxFactory.ThrowExpression( KeywordTokenWithSpace( SyntaxKind.ThrowKeyword ), expression );

    public static PragmaWarningDirectiveTriviaSyntax PragmaWarningDirectiveTrivia(
        SyntaxKind disableOrRestoreKind,
        SeparatedSyntaxList<ExpressionSyntax> errorCodes )
        => SyntaxFactory.PragmaWarningDirectiveTrivia(
            SyntaxFactory.Token( new( SyntaxFactory.ElasticLineFeed ), SyntaxKind.HashToken, default ),
            KeywordTokenWithSpace( SyntaxKind.PragmaKeyword ),
            KeywordTokenWithSpace( SyntaxKind.WarningKeyword ),
            KeywordTokenWithSpace( disableOrRestoreKind ),
            errorCodes,
            SyntaxFactory.Token( default, SyntaxKind.EndOfDirectiveToken, new( SyntaxFactory.ElasticLineFeed ) ),
            isActive: true );

    public static MethodDeclarationSyntax MethodDeclaration(
        SyntaxList<AttributeListSyntax> attributeLists,
        SyntaxTokenList modifiers,
        TypeSyntax returnType,
        ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier,
        SyntaxToken identifier,
        TypeParameterListSyntax? typeParameterList,
        ParameterListSyntax parameterList,
        SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses,
        BlockSyntax? body,
        ArrowExpressionClauseSyntax? expressionBody,
        SyntaxToken semicolonToken )
    {
        returnType = AddTrailingSpaceIfNecessary( returnType );

        return SyntaxFactory.MethodDeclaration(
            attributeLists,
            modifiers,
            returnType,
            explicitInterfaceSpecifier,
            identifier,
            typeParameterList,
            parameterList,
            constraintClauses,
            body,
            expressionBody,
            semicolonToken );
    }

    public static ReturnStatementSyntax ReturnStatement( ExpressionSyntax expression, SyntaxTrivia[]? leadingTrivia = null )
        => SyntaxFactory.ReturnStatement(
            SyntaxFactory.Token( leadingTrivia == null ? default : new( leadingTrivia ), SyntaxKind.ReturnKeyword, new( SyntaxFactory.ElasticSpace ) ),
            expression,
            SyntaxFactory.Token( SyntaxKind.SemicolonToken ) );

    public static ArrayCreationExpressionSyntax ArrayCreationExpression( ArrayTypeSyntax type, InitializerExpressionSyntax? initializer )
        => SyntaxFactory.ArrayCreationExpression(
            SyntaxFactory.Token( default, SyntaxKind.NewKeyword, new( SyntaxFactory.ElasticSpace ) ),
            type,
            initializer );

    public static VariableDeclarationSyntax VariableDeclaration( TypeSyntax type, SeparatedSyntaxList<VariableDeclaratorSyntax> variables )
    {
        type = AddTrailingSpaceIfNecessary( type );

        return SyntaxFactory.VariableDeclaration( type, variables );
    }

    public static ForEachStatementSyntax ForEachStatement(
        TypeSyntax type,
        SyntaxToken identifier,
        ExpressionSyntax expression,
        StatementSyntax statement )
        => ForEachStatement( default, type, identifier, expression, statement );

    public static ForEachStatementSyntax ForEachStatement(
        bool isAsync,
        TypeSyntax type,
        SyntaxToken identifier,
        ExpressionSyntax expression,
        StatementSyntax statement )
    {
        var awaitKeyword = isAsync ? SyntaxFactory.Token( default, SyntaxKind.AwaitKeyword, new( SyntaxFactory.ElasticSpace ) ) : default;
        type = AddTrailingSpaceIfNecessary( type );
        identifier = AddTrailingSpaceIfNecessary( identifier );

        return SyntaxFactory.ForEachStatement(
            default,
            awaitKeyword,
            SyntaxFactory.Token( SyntaxKind.ForEachKeyword ),
            SyntaxFactory.Token( SyntaxKind.OpenParenToken ),
            type,
            identifier,
            SyntaxFactory.Token( default, SyntaxKind.InKeyword, new(SyntaxFactory.ElasticSpace) ),
            expression,
            SyntaxFactory.Token( SyntaxKind.CloseParenToken ),
            statement );
    }

    public static YieldStatementSyntax YieldReturnStatement( ExpressionSyntax expression )
        => SyntaxFactory.YieldStatement(
            SyntaxKind.YieldReturnStatement,
            SyntaxFactory.Token( default, SyntaxKind.YieldKeyword, new( SyntaxFactory.ElasticSpace ) ),
            SyntaxFactory.Token( default, SyntaxKind.ReturnKeyword, new( SyntaxFactory.ElasticSpace ) ),
            expression,
            SyntaxFactory.Token( SyntaxKind.SemicolonToken ) );

    public static YieldStatementSyntax YieldBreakStatement()
        => SyntaxFactory.YieldStatement(
            SyntaxKind.YieldBreakStatement,
            default,
            SyntaxFactory.Token( default, SyntaxKind.YieldKeyword, new( SyntaxFactory.ElasticSpace ) ),
            SyntaxFactory.Token( SyntaxKind.BreakKeyword ),
            default,
            SyntaxFactory.Token( SyntaxKind.SemicolonToken ) );

#pragma warning restore RS0030
}