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
            var code = ((LiteralExpressionSyntax) SyntaxFactoryEx.LiteralExpression( value )).Token.Text;

            string suffix = "", prefix = "";

            if ( stronglyTyped )
            {
                if ( int.TryParse( code, out _ ) && specialType != SpecialType.Int32 )
                {
                    // Specify the suffix if there is an ambiguity.

                    suffix = specialType switch
                    {
                        SpecialType.UInt32 => "u",
                        SpecialType.Int64 => "l",
                        SpecialType.UInt64 => "ul",
                        SpecialType.Single => "f",
                        SpecialType.Double => "d",
                        SpecialType.Decimal => "m",
                        _ => ""
                    };
                }

                prefix = specialType switch
                {
                    SpecialType.Byte => "(byte) ",
                    SpecialType.SByte => "(sbyte) ",
                    SpecialType.Int16 => "(short) ",
                    SpecialType.UInt16 => "(ushort) ",
                    _ => ""
                };
            }

            stringBuilder.Append( prefix );
            stringBuilder.Append( code );
            stringBuilder.Append( suffix );
        }
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