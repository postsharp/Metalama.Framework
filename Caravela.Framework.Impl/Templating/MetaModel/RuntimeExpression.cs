// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// Contains information about an expression that is passed to dynamic methods.
    /// </summary>
    public sealed class RuntimeExpression
    {
        private const string _typeIdAnnotationName = "caravela-typeid";

        private readonly string? _expressionTypeName;
        private ITypeSymbol? _expressionType;
   

        /// <summary>
        /// Gets a value indicating whether it is legal to use the <c>out</c> or <c>ref</c> argument modifier with this expression.
        /// </summary>
        public bool IsReferenceable { get; }

        public ExpressionSyntax Syntax { get; }

        /// <summary>
        /// Returns the <see cref="ExpressionSyntax"/> encapsulated by the current <see cref="RuntimeExpression"/>. Called from generated
        /// code. Do not remove.
        /// </summary>
        /// <param name="runtimeExpression"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull( "runtimeExpression" )]
        public static implicit operator ExpressionSyntax?( RuntimeExpression? runtimeExpression ) => runtimeExpression?.Syntax;

        public static implicit operator ExpressionStatementSyntax?( RuntimeExpression? runtimeExpression )
            => runtimeExpression?.Syntax switch
            {
                null => null,
                ParenthesizedExpressionSyntax parenthesized => SyntaxFactory.ExpressionStatement( parenthesized.Expression ),
                _ => SyntaxFactory.ExpressionStatement( runtimeExpression.Syntax )
            };

        private ITypeSymbol? GetExpressionType( ITypeFactory typeFactory )
        {
            if ( this._expressionType == null )
            {
                if ( this._expressionTypeName == null )
                {
                    // We don't know the expression type, for instance because it is a `null` expression.
                    return null;
                }

                this._expressionType = typeFactory.GetTypeByReflectionName( this._expressionTypeName.AssertNotNull() ).GetSymbol();
            }

            return this._expressionType;
        }

        private static ExpressionSyntax AddTypeAnnotation(ExpressionSyntax node, ITypeSymbol? type, Compilation? compilation)
        {
            if ( type != null && compilation != null && !node.GetAnnotations( _typeIdAnnotationName ).Any() )
            {
                return node.WithAdditionalAnnotations(
                    new SyntaxAnnotation( _typeIdAnnotationName,  SymbolIdGenerator.GetInstance( compilation! ).GetId( type ).ToString( CultureInfo.InvariantCulture ) ) );
            }
            else
            {
                return node;
            }
        }

        private RuntimeExpression( ExpressionSyntax syntax, Compilation? compilation, ITypeSymbol? expressionType, bool isReferenceable )
        {
            this.Syntax = AddTypeAnnotation( syntax, expressionType, compilation );
            this._expressionType = expressionType;
            this.IsReferenceable = isReferenceable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeExpression"/> class by passing a type name. This constructor
        /// is called from generated code and must not be changed or removed.
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="typeName"></param>
        /// <param name="isReferenceable"></param>
        public RuntimeExpression( ExpressionSyntax syntax, string typeName )
        {
            this.Syntax = syntax;
            this._expressionTypeName = typeName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeExpression"/> class and passes a flag telling that the
        /// instance represents the <c>null</c> expression. This constructor is called from generated code and must not be changed or removed.
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="typeName"></param>
        /// <param name="isReferenceable"></param>
        public RuntimeExpression( ExpressionSyntax syntax, bool isNull )
        {
            this.Syntax = syntax;
            _ = isNull;
        }

        public RuntimeExpression( ExpressionSyntax syntax, IType type, bool isReferenceable = false )
            : this( syntax, type.GetCompilationModel().RoslynCompilation, type.GetSymbol(), isReferenceable ) { }

        public RuntimeExpression( ExpressionSyntax syntax )
            : this( syntax, null, null, false ) { }

        public static ExpressionSyntax GetSyntaxFromValue( object? value )
            => FromValue( value )?.Syntax ?? SyntaxFactory.LiteralExpression( SyntaxKind.NullKeyword );

        public static RuntimeExpression? FromValue( object? value )
        {
            switch ( value )
            {
                case null:
                    return null;

                case RuntimeExpression runtimeExpression:
                    return runtimeExpression;

                case IDynamicExpression dynamicMember:
                    return dynamicMember.CreateExpression();

                case ExpressionSyntax syntax:
                    return new RuntimeExpression( syntax );

                default:
                    var expression = SyntaxFactoryEx.LiteralExpressionOrNull( value );

                    if ( expression != null )
                    {
                        return new RuntimeExpression( expression );
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(value),
                            $"Cannot convert an instance of type {value.GetType().Name} to a run-time expression." );
                    }
            }
        }

        public static RuntimeExpression[]? FromValue( object?[]? array )
        {
            RuntimeExpression[] ConvertArray()
            {
                if ( array.Length == 0 )
                {
                    return Array.Empty<RuntimeExpression>();
                }

                var newArray = new RuntimeExpression[array.Length];

                for ( var i = 0; i < newArray.Length; i++ )
                {
                    newArray[i] = FromValue( array[i] ).AssertNotNull();
                }

                return newArray;
            }

            return array switch
            {
                null => null,
                RuntimeExpression[] runtimeExpressions => runtimeExpressions,
                _ => ConvertArray()
            };
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
        /// Converts the current <see cref="RuntimeExpression"/> into an <see cref="ExpressionSyntax"/> and emits a cast
        /// if necessary.
        /// </summary>
        /// <param name="targetType">The target type, or <c>null</c> if no cast must be emitted in any case.</param>
        /// <returns></returns>
        public ExpressionSyntax ToTypedExpression( IType targetType, bool addsParenthesis = false )
        {
            var targetTypeSymbol = targetType.GetSymbol();
            var compilation = targetType.GetCompilationModel().RoslynCompilation;

            var expressionType = this.GetExpressionType( targetType.Compilation.TypeFactory );

            if ( expressionType != null || TryFindExpressionType( this.Syntax, compilation, out expressionType ) )
            {
                // If we know the type of the current expression, check if a cast is necessary.

                if ( compilation.HasImplicitConversion( expressionType, targetType.GetSymbol() ) )
                {
                    return this.Syntax;
                }
            }
           
            // We may need a cast. We are not sure, but we cannot do more. This could be removed later in the simplification step.
            var cast = (ExpressionSyntax) LanguageServiceFactory.CSharpSyntaxGenerator.CastExpression( targetTypeSymbol, this.Syntax );

            var expression = (addsParenthesis ? SyntaxFactory.ParenthesizedExpression( cast ) : cast).WithAdditionalAnnotations( Simplifier.Annotation );

            return AddTypeAnnotation( expression, this._expressionType, compilation );
        }

        public override string ToString() => this.Syntax.ToString();
    }
}