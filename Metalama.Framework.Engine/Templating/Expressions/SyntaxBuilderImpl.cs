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
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.Templating.Expressions;

internal class ParameterExpression : UserExpression
{
    private IParameter _parameter;

    public ParameterExpression( IParameter parameter ) {
        this._parameter = parameter;
    }

    public override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext ) => SyntaxFactory.IdentifierName( this._parameter.Name );

    public override IType Type => this._parameter.Type;
}

internal class FieldOrPropertyExpression : UserExpression
{
    private readonly IFieldOrProperty _fieldOrProperty;
    private readonly UserExpression? _instance;

    public FieldOrPropertyExpression( IFieldOrProperty fieldOrProperty, UserExpression? instance )
    {
        this._fieldOrProperty = fieldOrProperty;
        this._instance = instance;
    }

    public override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
    {
        if ( this._fieldOrProperty.IsStatic )
        {
            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                syntaxGenerationContext.SyntaxGenerator.Type( this._fieldOrProperty.DeclaringType.GetSymbol() ),
                SyntaxFactory.IdentifierName( this._fieldOrProperty.Name ));            
        }
        else if ( this._instance != null )
        {
            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                this._instance.ToSyntax( syntaxGenerationContext ),
                SyntaxFactory.IdentifierName( this._fieldOrProperty.Name ));
            
        }
        else
        {
            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.ThisExpression(),
                SyntaxFactory.IdentifierName( this._fieldOrProperty.Name ) );
        }
        

       
    }

    public override IType Type => this._fieldOrProperty.Type;
}
internal class SyntaxBuilderImpl : ISyntaxBuilderImpl
{
    // Note that the implementation of this class cannot use TemplateExpansionContext because there is no necessarily one active.
    // For instance, in the BuildAspect method, there is none.

    private readonly CompilationModel _compilation;
    private readonly SyntaxGenerationContext _syntaxGenerationContext;

    public ICompilation Compilation => this._compilation;

    private OurSyntaxGenerator SyntaxGenerator => this._syntaxGenerationContext.SyntaxGenerator;

    public SyntaxBuilderImpl( CompilationModel compilation, SyntaxGenerationContext syntaxGenerationContext )
    {
        this._compilation = compilation;
        this._syntaxGenerationContext = syntaxGenerationContext;
    }

    public SyntaxBuilderImpl( CompilationModel compilation, IServiceProvider serviceProvider )
    {
        this._compilation = compilation;
        var syntaxGenerationContextFactory = serviceProvider.GetService<SyntaxGenerationContextFactory>();

        if ( syntaxGenerationContextFactory != null )
        {
            this._syntaxGenerationContext = syntaxGenerationContextFactory.Default;
        }
        else
        {
            // This should happen in tests only.
            this._syntaxGenerationContext = SyntaxGenerationContext.Create( serviceProvider, compilation.RoslynCompilation );
        }
    }

    [Obfuscation( Exclude = true )]
    public IProject Project => this.Compilation.Project;

    public IExpression Capture( object? expression ) => new CapturedUserExpression( this.Compilation, expression );

    public IExpression BuildArray( ArrayBuilder arrayBuilder ) => new ArrayUserExpression( arrayBuilder );

    public IExpression BuildInterpolatedString( InterpolatedStringBuilder interpolatedStringBuilder )
        => new InterpolatedStringUserExpression( interpolatedStringBuilder, this.Compilation );

    public IExpression ParseExpression( string code )
    {
        var expression = SyntaxFactory.ParseExpression( code ).WithAdditionalAnnotations( Formatter.Annotation );

        return new BuiltUserExpression( expression, this._compilation.Factory.GetSpecialType( SpecialType.Object ) );
    }

    public IStatement ParseStatement( string code )
    {
        var statement = SyntaxFactory.ParseStatement( code );

        return new UserStatement( statement );
    }

    public IStatement CreateExpressionStatement( IExpression expression )
        => new UserStatement( SyntaxFactory.ExpressionStatement( ((UserExpression) expression).ToSyntax( this._syntaxGenerationContext ) ) );

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
        var options = stronglyTyped ? ObjectDisplayOptions.IncludeTypeSuffix : ObjectDisplayOptions.None;
        var expression = (LiteralExpressionSyntax) SyntaxFactoryEx.LiteralExpression( value, options );

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
                return SyntaxFactory.CastExpression( SyntaxFactory.PredefinedType( SyntaxFactory.Token( cast ) ), expression );
            }
        }

        return expression;
    }

    public IExpression Literal( object? value, SpecialType specialType, bool stronglyTyped )
    {
        ExpressionSyntax expression;
        IType type;

        if ( value == null )
        {
            expression = stronglyTyped
                ? SyntaxFactory.DefaultExpression( SyntaxFactory.PredefinedType( SyntaxFactory.Token( SyntaxKind.StringKeyword ) ) )
                : SyntaxFactoryEx.Null;
        }
        else
        {
            expression = GetLiteralImpl( value, specialType, stronglyTyped );
        }

        type = this._compilation.Factory.GetSpecialType( specialType );

        return new BuiltUserExpression( expression, type );
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
            ((IUserExpression) expression).ToSyntax( this._syntaxGenerationContext )
            .NormalizeWhitespace()
            .ToFullString() );
    }

    public void AppendDynamic( object? expression, StringBuilder stringBuilder )
        => stringBuilder.Append(
            expression == null
                ? "null"
                : ((RunTimeTemplateExpression) expression).Syntax.NormalizeWhitespace().ToFullString() );

    public IExpression Cast( IExpression expression, IType targetType )
        => expression.Type.Is( targetType ) ? expression : new CastUserExpression( targetType, expression );

    public object TypedConstant( in TypedConstant typedConstant )
        => new BuiltUserExpression( this.SyntaxGenerator.TypedConstant( typedConstant ), typedConstant.Type );

    public IExpression This( INamedType type ) => new BuiltUserExpression( SyntaxFactory.ThisExpression(), type, false );

    public IExpression ToExpression( IFieldOrProperty fieldOrProperty, IExpression? instance ) => new FieldOrPropertyExpression( fieldOrProperty, (UserExpression?) instance );

    public IExpression ToExpression( IParameter parameter ) => new ParameterExpression( parameter );
}