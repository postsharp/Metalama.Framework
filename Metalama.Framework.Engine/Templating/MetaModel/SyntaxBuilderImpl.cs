// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Reflection;
using System.Text;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Templating.MetaModel;

internal class SyntaxBuilderImpl : ISyntaxBuilderImpl
{
    public ICompilation Compilation { get; }

    public OurSyntaxGenerator SyntaxGenerator { get; }

    public SyntaxBuilderImpl( ICompilation compilation, OurSyntaxGenerator syntaxGenerator )
    {
        this.Compilation = compilation;
        this.SyntaxGenerator = syntaxGenerator;
    }

    [Obfuscation( Exclude = true )]
    public IProject Project => this.Compilation.Project;

    public IExpression Expression( object? expression )
        => RuntimeExpression.FromValue( expression, this.Compilation, TemplateExpansionContext.CurrentSyntaxGenerationContext )
            .ToUserExpression( this.Compilation );

    public IExpression BuildArray( ArrayBuilder arrayBuilder ) => new ArrayUserExpression( arrayBuilder );

    public IExpression BuildInterpolatedString( InterpolatedStringBuilder interpolatedStringBuilder )
        => new InterpolatedStringUserExpression( interpolatedStringBuilder, this.Compilation );

    public IExpression ParseExpression( string code )
    {
        var expression = SyntaxFactory.ParseExpression( code ).WithAdditionalAnnotations( Formatter.Annotation );

        return new RuntimeExpression( expression, this.Compilation, this.Project.ServiceProvider ).ToUserExpression( this.Compilation );
    }

    public IStatement ParseStatement( string code )
    {
        var statement = SyntaxFactory.ParseStatement( code );

        return new UserStatement( statement );
    }

    public void AppendLiteral( object? value, StringBuilder stringBuilder, SpecialType specialType, bool stronglyTyped )
    {
        if ( value == null )
        {
            stringBuilder.Append( stronglyTyped ? "default(string)" : "null" );
        }
        else
        {
            var expression = GetLiteralImpl( value, specialType, stronglyTyped );
            stringBuilder.Append( expression.ToFullString() );
        }
    }

    private static ExpressionSyntax GetLiteralImpl( object value, SpecialType specialType, bool stronglyTyped )
    {
        var literal = (LiteralExpressionSyntax) SyntaxFactoryEx.LiteralExpression( value );

        if ( stronglyTyped && specialType != SpecialType.String )
        {
            var hasSuffix = !char.IsDigit( literal.Token.Text[literal.Token.Text.Length - 1] );

            if ( !hasSuffix && specialType != SpecialType.Int32 )
            {
                var suffix = specialType switch
                {
                    SpecialType.UInt32 => "u",
                    SpecialType.Int64 => "l",
                    SpecialType.UInt64 => "ul",
                    SpecialType.Single => "f",
                    SpecialType.Double => "d",
                    SpecialType.Decimal => "m",
                    _ => null
                };

                if ( suffix != null )
                {
                    return literal.WithToken( SyntaxFactory.Literal( default, literal.Token.Text + suffix,  ) )
                }
                
                var cast = specialType switch
                {
                    SpecialType.Byte => SyntaxKind.ByteKeyword,
                    SpecialType.SByte => SyntaxKind.SByteKeyword,
                    SpecialType.Int16 => SyntaxKind.ShortKeyword,
                    SpecialType.UInt16 => SyntaxKind.UShortKeyword,
                    _ => SyntaxKind.None
                };

                if ( cast != SyntaxKind.None )
                {
                    return SyntaxFactory.CastExpression( SyntaxFactory.PredefinedType( SyntaxFactory.Token( cast ) ), literal );
                }
            }
        }

        return literal;
    }

    public IExpression Literal( object? value, SpecialType specialType, bool stronglyTyped )
    {
        ExpressionSyntax expression;

        if ( value == null )
        {
            expression = stronglyTyped
                ? SyntaxFactory.DefaultExpression( SyntaxFactory.PredefinedType( SyntaxFactory.Token( SyntaxKind.StringKeyword ) ) ) : SyntaxFactoryEx.Null;
        }
        else
        {
            expression = GetLiteralImpl( value, specialType, stronglyTyped );
        }
        
        return new RuntimeExpression( expression, this.Compilation, this.Project.ServiceProvider ).ToUserExpression( this.Compilation );
    }

    public void AppendTypeName( IType type, StringBuilder stringBuilder )
    {
        var code = this.SyntaxGenerator.Type( type.GetSymbol().AssertNotNull() ).ToString();
        stringBuilder.Append( code );
    }

    public void AppendTypeName( Type type, StringBuilder stringBuilder )
        => this.AppendTypeName(
            this.Compilation.GetCompilationModel().Factory.GetTypeByReflectionType( type ),
            stringBuilder );

    public void AppendExpression( IExpression expression, StringBuilder stringBuilder )
    {
        stringBuilder.Append(
            ((IUserExpression) expression.Value!).ToRunTimeExpression()
            .Syntax.NormalizeWhitespace()
            .ToFullString() );
    }

    public void AppendDynamic( object? expression, StringBuilder stringBuilder )
        => stringBuilder.Append(
            expression == null
                ? "null"
                : ((RuntimeExpression) expression).Syntax.NormalizeWhitespace().ToFullString() );
}