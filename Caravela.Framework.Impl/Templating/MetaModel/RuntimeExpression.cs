using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// Contains information about an expression that is passed to dynamic methods.
    /// </summary>
    public sealed class RuntimeExpression
    {

        /// <summary>
        /// Determines whether it is legal to use the 'out' or 'ref' argument modifier with this expression.
        /// </summary>
        public bool IsReferenceable { get; }

        public ExpressionSyntax Syntax { get; }

        public ITypeSymbol? ExpressionType { get; }

        private RuntimeExpression( ExpressionSyntax syntax, ITypeSymbol? expressionType, bool isReferenceable )
        {
            this.Syntax = syntax;
            this.ExpressionType = expressionType;
            this.IsReferenceable = isReferenceable;
        }

        public RuntimeExpression( ExpressionSyntax syntax, IType? type = null, bool isReferenceable = false ) : this( syntax, type?.GetSymbol(),
            isReferenceable )
        {
        }

        /// <summary>
        /// Converts the current <see cref="RuntimeExpression"/> into an <see cref="ExpressionSyntax"/> and emits a cast
        /// if necessary.
        /// </summary>
        /// <param name="targetType">The target type, or <c>null</c> if no cast must be emitted in any case.</param>
        /// <returns></returns>
        public ExpressionSyntax ToTypedExpression( IType targetType, bool addsParenthesis = false )
        {
            var expressionType = this.ExpressionType;

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


        public static RuntimeExpression? FromDynamic( object? value )
            => value switch
            {
                null => null,
                RuntimeExpression runtimeExpression => runtimeExpression,
                
                // This case is used to simplify tests.
                IDynamicMember dynamicMember => dynamicMember.CreateExpression(),
                
                _ => throw new ArgumentOutOfRangeException( nameof(value) )
            };

        public static RuntimeExpression?[]? FromDynamic( object[]? array )
        {
            RuntimeExpression?[] ConvertArray()
            {
                if ( array.Length == 0 )
                {
                    return Array.Empty<RuntimeExpression>();
                }
                else
                {
                    var newArray = new RuntimeExpression?[array.Length];
                    for ( int i = 0; i < newArray.Length; i++ )
                    {
                        newArray[i] = FromDynamic( array[i] );
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
    }
}
