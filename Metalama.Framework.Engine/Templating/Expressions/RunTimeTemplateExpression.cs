// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    /// <summary>
    /// Contains information about an expression that is passed to dynamic methods. The main difference
    /// between a <see cref="RunTimeTemplateExpression"/> and an <see cref="IUserExpression"/> is that the <see cref="RunTimeTemplateExpression"/>
    /// is bound to a specific <see cref="SyntaxGenerationContext"/>. Another difference is that this class tries to find the expression type from
    /// the expression syntax tree thanks to a syntax annotation. This class seems to exist because of the implicit operators from and
    /// to an <see cref="ExpressionSyntax"/>: the template compiler generates code that relies on these implicit conversions.
    /// </summary>
    public sealed class RunTimeTemplateExpression
    {
        private const string _typeIdAnnotationName = "metalama-typeid";

        internal SyntaxGenerationContext SyntaxGenerationContext { get; }

        /// <summary>
        /// Gets the expression type, or <c>null</c> if the expression is actually the <c>null</c> or <c>default</c> expression.
        /// </summary>
        public ITypeSymbol? ExpressionType { get; }

        /// <summary>
        /// Gets a value indicating whether it is legal to use the <c>out</c> or <c>ref</c> argument modifier with this expression.
        /// </summary>
        public bool IsReferenceable { get; }

        public ExpressionSyntax Syntax { get; }

        /// <summary>
        /// Returns the <see cref="ExpressionSyntax"/> encapsulated by the current <see cref="RunTimeTemplateExpression"/>. Called from generated
        /// code. Do not remove.
        /// </summary>
        /// <param name="runtimeExpression"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull( "runtimeExpression" )]
        public static implicit operator ExpressionSyntax?( RunTimeTemplateExpression? runtimeExpression ) => runtimeExpression?.Syntax;

        public static implicit operator ExpressionStatementSyntax?( RunTimeTemplateExpression? runtimeExpression )
            => runtimeExpression?.Syntax switch
            {
                null => null,
                ParenthesizedExpressionSyntax parenthesized => SyntaxFactory.ExpressionStatement( parenthesized.Expression ),
                _ => SyntaxFactory.ExpressionStatement( runtimeExpression.Syntax )
            };

        private static ExpressionSyntax AddTypeAnnotation( ExpressionSyntax node, ITypeSymbol? type, Compilation? compilation )
        {
            if ( type != null && compilation != null && !node.GetAnnotations( _typeIdAnnotationName ).Any() )
            {
                return node.WithAdditionalAnnotations(
                    new SyntaxAnnotation(
                        _typeIdAnnotationName,
                        SymbolIdGenerator.GetInstance( compilation ).GetId( type ).ToString( CultureInfo.InvariantCulture ) ) );
            }
            else
            {
                return node;
            }
        }

        internal RunTimeTemplateExpression(
            ExpressionSyntax syntax,
            ITypeSymbol? expressionType,
            SyntaxGenerationContext generationContext,
            bool isReferenceable )
        {
#if DEBUG
            if ( generationContext.Compilation == SyntaxGenerationContext.EmptyCompilation )
            {
                throw new AssertionFailedException();
            }
#endif

            if ( expressionType == null )
            {
                // This should happen only for null and default expressions.
                TryFindExpressionType( syntax, generationContext.Compilation, out expressionType );
            }
            else
            {
                syntax = AddTypeAnnotation( syntax, expressionType, generationContext.Compilation );
            }

            this.Syntax = syntax;
            this.ExpressionType = expressionType;
            this.IsReferenceable = isReferenceable;
            this.SyntaxGenerationContext = generationContext;
        }

        internal RunTimeTemplateExpression( ExpressionSyntax syntax, IType type, SyntaxGenerationContext generationContext, bool isReferenceable = false )
            : this( syntax, type.GetSymbol(), generationContext, isReferenceable ) { }

        // This overload must be used only in tests or when the expression type is really unknown.
        internal RunTimeTemplateExpression( ExpressionSyntax syntax, IServiceProvider serviceProvider )
            : this(
                syntax,
                (ITypeSymbol) null!,
                serviceProvider.GetRequiredService<SyntaxGenerationContextFactory>().Default,
                false ) { }

        internal RunTimeTemplateExpression( ExpressionSyntax syntax, SyntaxGenerationContext syntaxGenerationContext )
            : this(
                syntax,
                (ITypeSymbol) null!,
                syntaxGenerationContext,
                false ) { }

        internal static ExpressionSyntax GetSyntaxFromValue( object? value, ICompilation compilation, SyntaxGenerationContext generationContext )
            => FromValue( value, compilation, generationContext ).Syntax;

        internal static RunTimeTemplateExpression FromValue( object? value, ICompilation compilation, SyntaxGenerationContext generationContext )
        {
            switch ( value )
            {
                case null:
                    return new RunTimeTemplateExpression( SyntaxFactoryEx.Null, generationContext );

                case RunTimeTemplateExpression runtimeExpression:
                    return runtimeExpression;

                case IUserExpression dynamicMember:
                    return dynamicMember.ToRunTimeTemplateExpression( generationContext );

                case ExpressionSyntax syntax:
                    return new RunTimeTemplateExpression( syntax, generationContext );

                default:
                    var expression = SyntaxFactoryEx.LiteralExpressionOrNull( value );

                    if ( expression != null )
                    {
                        return new RunTimeTemplateExpression(
                            expression,
                            compilation.GetCompilationModel().Factory.GetTypeByReflectionType( value.GetType() ),
                            generationContext );
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(value),
                            $"Cannot convert an instance of type {value.GetType().Name} to a run-time expression." );
                    }
            }
        }

        internal static RunTimeTemplateExpression[]? FromValue( object?[]? array, ICompilation compilation, SyntaxGenerationContext generationContext )
        {
            switch ( array )
            {
                case null:
                    return null;

                case RunTimeTemplateExpression[] runtimeExpressions:
                    return runtimeExpressions;

                default:
                    if ( array.Length == 0 )
                    {
                        return Array.Empty<RunTimeTemplateExpression>();
                    }

                    var newArray = new RunTimeTemplateExpression[array.Length];

                    for ( var i = 0; i < newArray.Length; i++ )
                    {
                        newArray[i] = FromValue( array[i], compilation, generationContext ).AssertNotNull();
                    }

                    return newArray;
            }
        }

        public static bool TryFindExpressionType( SyntaxNode node, Compilation compilation, out ITypeSymbol? type )
        {
            // If we don't know the exact type, check if we have a type annotation on the syntax.

            var typeAnnotation = node.GetAnnotations( _typeIdAnnotationName ).FirstOrDefault();

            if ( typeAnnotation != null! )
            {
                var symbolId = typeAnnotation.Data!;

                type = (ITypeSymbol) SymbolIdGenerator.GetInstance( compilation ).GetSymbol( symbolId );

                return true;
            }
            else if ( SyntaxTreeAnnotationMap.TryGetExpressionType( node, compilation, out var symbol ) )
            {
                type = (ITypeSymbol) symbol;

                return true;
            }
            else
            {
                type = null;

                return false;
            }
        }

        /// <summary>
        /// Converts the current <see cref="RunTimeTemplateExpression"/> into an <see cref="ExpressionSyntax"/> and emits a cast
        /// if necessary.
        /// </summary>
        /// <param name="targetType">The target type, or <c>null</c> if no cast must be emitted in any case.</param>
        /// <param name="addsParenthesis"></param>
        /// <returns></returns>
        public ExpressionSyntax ToTypedExpression( IType targetType, bool addsParenthesis = false )
        {
            var targetTypeSymbol = targetType.GetSymbol();
            var compilation = targetType.GetCompilationModel().RoslynCompilation;

            if ( this.ExpressionType != null )
            {
                // If we know the type of the current expression, check if a cast is necessary.

                if ( compilation.HasImplicitConversion( this.ExpressionType, targetType.GetSymbol() ) )
                {
                    return this.Syntax;
                }
            }

            // We may need a cast. We are not sure, but we cannot do more. This could be removed later in the simplification step.
            var cast = (ExpressionSyntax) this.SyntaxGenerationContext.SyntaxGenerator.CastExpression( targetTypeSymbol, this.Syntax );

            var expression = (addsParenthesis ? SyntaxFactory.ParenthesizedExpression( cast ) : cast).WithAdditionalAnnotations( Simplifier.Annotation );

            return AddTypeAnnotation( expression, this.ExpressionType, compilation );
        }

        public override string ToString() => this.Syntax.ToString();

        public IUserExpression ToUserExpression( ICompilation compilation )
        {
            var declarationFactory = compilation.GetCompilationModel().Factory;

            return new UserExpression(
                this.Syntax,
                this.ExpressionType != null ? declarationFactory.GetIType( this.ExpressionType ) : declarationFactory.GetSpecialType( SpecialType.Object ),
                isReferenceable: this.IsReferenceable,
                isAssignable: false );
        }
    }
}