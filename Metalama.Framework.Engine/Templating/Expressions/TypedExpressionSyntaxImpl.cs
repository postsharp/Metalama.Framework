// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    /// <summary>
    /// Represents an <see cref="ExpressionSyntax"/> and its <see cref="IType"/>. Annotates the <see cref="ExpressionSyntax"/>
    /// with <see cref="ExpressionTypeAnnotationHelper"/>.
    /// </summary>
    internal sealed class TypedExpressionSyntaxImpl : ITypedExpressionSyntax
    {
        /// <summary>
        /// Gets the expression type, or <c>null</c> if the expression is actually the <c>null</c> or <c>default</c> expression.
        /// </summary>
        public ITypeSymbol? ExpressionType { get; }

        /// <summary>
        /// Gets a value indicating whether it is legal to use the <c>out</c> or <c>ref</c> argument modifier with this expression.
        /// </summary>
        public bool IsReferenceable { get; }

        public ExpressionSyntax Syntax { get; }

        public ExpressionStatementSyntax? ToStatement() => SyntaxFactory.ExpressionStatement( this.Syntax.RemoveParenthesis() );

        public static implicit operator TypedExpressionSyntax( TypedExpressionSyntaxImpl impl ) => new( impl );

        public static implicit operator TypedExpressionSyntaxImpl( TypedExpressionSyntax wrapper ) => (TypedExpressionSyntaxImpl) wrapper.Implementation;

        internal TypedExpressionSyntaxImpl(
            ExpressionSyntax syntax,
            ITypeSymbol? expressionType,
            SyntaxGenerationContext generationContext,
            bool isReferenceable )
        {
#if DEBUG
            if ( generationContext.Compilation == CompilationContextFactory.EmptyCompilation )
            {
                throw new AssertionFailedException( "The compilation is empty." );
            }
#endif

            if ( expressionType == null )
            {
                // This should happen only for null and default expressions.
                ExpressionTypeAnnotationHelper.TryFindTypeFromAnnotation( syntax, generationContext.Compilation, out expressionType );
            }
            else
            {
                syntax = syntax.WithTypeAnnotation( expressionType, generationContext.Compilation );
            }

            this.Syntax = syntax;
            this.ExpressionType = expressionType;
            this.IsReferenceable = isReferenceable;
        }

        internal TypedExpressionSyntaxImpl( ExpressionSyntax syntax, IType type, ISyntaxGenerationContext generationContext, bool isReferenceable = false )
            : this( syntax, type, (SyntaxGenerationContext) generationContext, isReferenceable ) { }

        internal TypedExpressionSyntaxImpl( ExpressionSyntax syntax, IType type, SyntaxGenerationContext generationContext, bool isReferenceable = false )
            : this( syntax, type.GetSymbol(), generationContext, isReferenceable ) { }

        internal TypedExpressionSyntaxImpl( ExpressionSyntax syntax, SyntaxGenerationContext syntaxGenerationContext )
            : this(
                syntax,
                (ITypeSymbol) null!,
                syntaxGenerationContext,
                false ) { }

        internal static ExpressionSyntax GetSyntaxFromValue( object? value, ICompilation compilation, SyntaxGenerationContext generationContext )
            => FromValue( value, compilation, generationContext ).Syntax;

        internal static TypedExpressionSyntaxImpl FromValue( object? value, ICompilation compilation, SyntaxGenerationContext generationContext )
        {
            switch ( value )
            {
                case null:
                    return new TypedExpressionSyntaxImpl( SyntaxFactoryEx.Null, generationContext );

                case TypedExpressionSyntaxImpl runtimeExpression:
                    return runtimeExpression;

                case TypedExpressionSyntax runtimeExpression:
                    return runtimeExpression;

                case IUserExpression dynamicMember:
                    return dynamicMember.ToTypedExpressionSyntax( generationContext );

                case ExpressionSyntax syntax:
                    return new TypedExpressionSyntaxImpl( syntax, generationContext );

                default:
                    var expression = SyntaxFactoryEx.LiteralExpressionOrNull( value );

                    if ( expression != null )
                    {
                        return new TypedExpressionSyntaxImpl(
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

        internal static TypedExpressionSyntaxImpl[]? FromValue( object?[]? array, ICompilation compilation, SyntaxGenerationContext generationContext )
        {
            switch ( array )
            {
                case null:
                    return null;

                default:
                    if ( array.Length == 0 )
                    {
                        return Array.Empty<TypedExpressionSyntaxImpl>();
                    }

                    var newArray = new TypedExpressionSyntaxImpl[array.Length];

                    for ( var i = 0; i < newArray.Length; i++ )
                    {
                        newArray[i] = FromValue( array[i], compilation, generationContext );
                    }

                    return newArray;
            }
        }

        /// <summary>
        /// Converts the current <see cref="TypedExpressionSyntaxImpl"/> into an <see cref="ExpressionSyntax"/> and emits a cast
        /// if necessary.
        /// </summary>
        /// <param name="targetType">The target type, or <c>null</c> if no cast must be emitted in any case.</param>
        /// <returns></returns>
        public TypedExpressionSyntaxImpl Convert( IType targetType, SyntaxGenerationContext generationContext )
        {
            var targetTypeSymbol = targetType.GetSymbol();
            var compilation = targetType.GetCompilationModel().RoslynCompilation;

            if ( this.ExpressionType != null )
            {
                // If we know the type of the current expression, check if a cast is necessary.

                if ( compilation.HasImplicitConversion( this.ExpressionType, targetType.GetSymbol() ) )
                {
                    return new TypedExpressionSyntaxImpl( this.Syntax, targetType, generationContext );
                }
            }

            // We may need a cast. We are not sure, but we cannot do more. This could be removed later in the simplification step.
            var cast = (ExpressionSyntax) generationContext.SyntaxGenerator.CastExpression( targetTypeSymbol, this.Syntax );

            var expression = SyntaxFactory.ParenthesizedExpression( cast ).WithAdditionalAnnotations( Simplifier.Annotation );

            return new TypedExpressionSyntaxImpl( expression, targetType, generationContext );
        }

        public override string ToString() => this.Syntax.ToString();
    }
}