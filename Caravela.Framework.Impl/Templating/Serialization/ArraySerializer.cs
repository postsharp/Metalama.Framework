using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    internal class ArraySerializer
    {
        private readonly ObjectSerializers _serializers;

        public ArraySerializer( ObjectSerializers serializers ) => this._serializers = serializers;

        public ExpressionSyntax Serialize( Array array )
        {
            var elementType = array.GetType().GetElementType()!;
            if ( array.Rank > 1 )
            {
                throw new CaravelaException( GeneralDiagnosticDescriptors.MultidimensionalArray, array );
            }

            var lt = new List<ExpressionSyntax>();
            foreach ( var o in array )
            {
                ObjectSerializer.ThrowIfStackTooDeep(o);
                lt.Add( this._serializers.SerializeToRoslynCreationExpression( o ) );
            }

            return ArrayCreationExpression(
                    ArrayType(
                            ParseTypeName( TypeNameUtility.ToCSharpQualifiedName( elementType ) )
                        )
                        .WithRankSpecifiers(
                            SingletonList(
                                ArrayRankSpecifier(
                                    SingletonSeparatedList<ExpressionSyntax>(
                                        OmittedArraySizeExpression()))))
                    )
                .WithInitializer(
                    InitializerExpression(
                        SyntaxKind.ArrayInitializerExpression,
                        SeparatedList( lt ) ) )
                .NormalizeWhitespace();
        }
    }
}