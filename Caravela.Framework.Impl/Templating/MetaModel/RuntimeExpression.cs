// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// Contains information about an expression that is passed to dynamic methods.
    /// </summary>
    public sealed class RuntimeExpression
    {
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
        public static implicit operator ExpressionSyntax( RuntimeExpression runtimeExpression ) => runtimeExpression.Syntax;

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

        private RuntimeExpression( ExpressionSyntax syntax, ITypeSymbol? expressionType, bool isReferenceable )
        {
            this.Syntax = syntax;
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
            : this( syntax, type.GetSymbol(), isReferenceable )
        {
        }

        public RuntimeExpression( ExpressionSyntax syntax )
            : this( syntax, (ITypeSymbol?) null, false )
        {
        }

        public static ExpressionSyntax GetSyntaxFromDynamic( object? value )
            => FromDynamic( value )?.Syntax ?? SyntaxFactory.LiteralExpression( SyntaxKind.NullKeyword );

        public static RuntimeExpression? FromDynamic( object? value )
            => value switch
            {
                null => null,
                RuntimeExpression runtimeExpression => runtimeExpression,

                // This case is used to simplify tests.
                IDynamicMember dynamicMember => dynamicMember.CreateExpression(),

                _ => throw new ArgumentOutOfRangeException( nameof( value ) )
            };

        public static RuntimeExpression[]? FromDynamic( object[]? array )
        {
            RuntimeExpression[] ConvertArray()
            {
                if ( array.Length == 0 )
                {
                    return Array.Empty<RuntimeExpression>();
                }
                else
                {
                    var newArray = new RuntimeExpression[array.Length];
                    for ( var i = 0; i < newArray.Length; i++ )
                    {
                        newArray[i] = FromDynamic( array[i] ).AssertNotNull();
                    }

                    return newArray;
                }
            }

            return array switch
            {
                null => null,
                RuntimeExpression[] runtimeExpressions => runtimeExpressions,
                _ => ConvertArray()
            };
        }

        /// <summary>
        /// Converts the current <see cref="RuntimeExpression"/> into an <see cref="ExpressionSyntax"/> and emits a cast
        /// if necessary.
        /// </summary>
        /// <param name="targetType">The target type, or <c>null</c> if no cast must be emitted in any case.</param>
        /// <returns></returns>
        public ExpressionSyntax ToTypedExpression( IType targetType, bool addsParenthesis = false )
        {
            var expressionType = this.GetExpressionType( targetType.Compilation.TypeFactory );

            var targetTypeSymbol = targetType.GetSymbol();

            if ( SymbolEqualityComparer.Default.Equals( expressionType, targetTypeSymbol ) )
            {
                return this.Syntax;
            }
            else
            {
                var cast = (ExpressionSyntax) CSharpSyntaxGenerator.Instance.CastExpression( targetTypeSymbol, this.Syntax );

                return addsParenthesis ? SyntaxFactory.ParenthesizedExpression( cast ) : cast;
            }
        }
    }
}
