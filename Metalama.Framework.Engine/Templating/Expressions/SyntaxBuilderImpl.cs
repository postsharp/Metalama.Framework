// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Text;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.Templating.Expressions;

internal class SyntaxBuilderImpl : ISyntaxBuilderImpl
{
    // Note that the implementation of this class cannot use TemplateExpansionContext because there is no necessarily one active.
    // For instance, in the BuildAspect method, there is none.

    private readonly CompilationModel _compilation;
    private readonly SyntaxGenerationContext _syntaxGenerationContext;

    public ICompilation Compilation => this._compilation;

    private ContextualSyntaxGenerator SyntaxGenerator => this._syntaxGenerationContext.SyntaxGenerator;

    protected SyntaxBuilderImpl( CompilationModel compilation, SyntaxGenerationContext syntaxGenerationContext )
    {
        this._compilation = compilation;
        this._syntaxGenerationContext = syntaxGenerationContext;
    }

    public SyntaxBuilderImpl( CompilationModel compilation, SyntaxGenerationOptions syntaxGenerationOptions )
        : this( compilation, compilation.CompilationContext.GetSyntaxGenerationContext( syntaxGenerationOptions ) ) { }

    public IProject Project => this.Compilation.Project;

    public IExpression Capture( object? expression ) => new CapturedUserExpression( this.Compilation, expression );

    public IExpression BuildArray( ArrayBuilder arrayBuilder ) => new ArrayUserExpression( arrayBuilder );

    public IExpression BuildInterpolatedString( InterpolatedStringBuilder interpolatedStringBuilder )
        => new InterpolatedStringUserExpression( interpolatedStringBuilder, this.Compilation );

    public IExpression ParseExpression( string code )
    {
        var expression = SyntaxFactoryEx.ParseExpressionSafe( code ).WithAdditionalAnnotations( Formatter.Annotation );

        return new SyntaxUserExpression( expression, this._compilation.Cache.SystemObjectType );
    }

    public IStatement ParseStatement( string code )
    {
        var statement = SyntaxFactoryEx.ParseStatementSafe( code );

        return new UserStatement( statement );
    }

    public IStatement CreateExpressionStatement( IExpression expression )
        => new UserStatement(
            SyntaxFactory.ExpressionStatement(
                ((UserExpression) expression).ToExpressionSyntax( new SyntaxSerializationContext( this._compilation, this._syntaxGenerationContext ) ) ) );

    public void AppendLiteral( object? value, StringBuilder stringBuilder, SpecialType specialType, bool stronglyTyped )
    {
        if ( value == null )
        {
            stringBuilder.Append( stronglyTyped ? "default(string)" : "null" );
        }
        else
        {
            var expression = this.GetLiteralImpl( value, specialType, stronglyTyped );
            stringBuilder.Append( expression.ToFullString() );
        }
    }

    private ExpressionSyntax GetLiteralImpl( object value, SpecialType specialType, bool stronglyTyped )
    {
        var options = stronglyTyped ? ObjectDisplayOptions.IncludeTypeSuffix : ObjectDisplayOptions.None;
        var expression = SyntaxFactoryEx.LiteralExpression( value, options );

        if ( stronglyTyped && specialType != SpecialType.String )
        {
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
                return this._syntaxGenerationContext.SyntaxGenerator.SafeCastExpression(
                    SyntaxFactory.PredefinedType( SyntaxFactory.Token( cast ) ),
                    expression );
            }
        }

        return expression;
    }

    public IExpression Literal( object? value, SpecialType specialType, bool stronglyTyped )
    {
        ExpressionSyntax expression;

        if ( value == null )
        {
            expression = stronglyTyped
                ? SyntaxFactory.DefaultExpression( SyntaxFactory.PredefinedType( SyntaxFactory.Token( SyntaxKind.StringKeyword ) ) )
                : SyntaxFactoryEx.Null;
        }
        else
        {
            expression = this.GetLiteralImpl( value, specialType, stronglyTyped );
        }

        IType type = this._compilation.Factory.GetSpecialType( specialType );

        return new SyntaxUserExpression( expression, type );
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
        => stringBuilder.Append(
            expression.ToExpressionSyntax( new SyntaxSerializationContext( this._compilation, this._syntaxGenerationContext ) )
                .NormalizeWhitespace()
                .ToFullString() );

    public void AppendDynamic( object? expression, StringBuilder stringBuilder )
        => stringBuilder.Append(
            expression == null
                ? "null"
                : TypedExpressionSyntaxImpl.GetSyntaxFromValue( expression, new SyntaxSerializationContext( this._compilation, this._syntaxGenerationContext ) )
                    .NormalizeWhitespace()
                    .ToFullString() );

    public IExpression Cast( IExpression expression, IType targetType )
        => expression.Type.Is( targetType ) ? expression : new CastUserExpression( targetType, expression );

    public object TypedConstant( in TypedConstant typedConstant )
        => new SyntaxUserExpression( this.SyntaxGenerator.TypedConstant( typedConstant ), typedConstant.Type );

    public IExpression ThisExpression( INamedType type ) => new SyntaxUserExpression( SyntaxFactory.ThisExpression(), type );

    public IExpression ToExpression( IFieldOrProperty fieldOrProperty, IExpression? instance )
    {
        if ( fieldOrProperty is { DeclarationKind: DeclarationKind.Field, IsImplicitlyDeclared: true } )
        {
            throw new InvalidOperationException(
                MetalamaStringFormatter.Format( $"Cannot convert '{fieldOrProperty}' to an IExpression because it is an implicitly declared field." ) );
        }

        return new FieldOrPropertyInvoker( fieldOrProperty, InvokerOptions.Default, instance );
    }

    public IExpression ToExpression( IParameter parameter ) => new ParameterExpression( parameter );
}