// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Impl.Serialization
{
    internal class ArraySerializer : ObjectSerializer
    {
        public ArraySerializer( SyntaxSerializationService serializers ) : base( serializers ) { }

        public override ExpressionSyntax Serialize( object obj, SyntaxSerializationContext serializationContext )
        {
            var array = (Array) obj;

            var elementType = array.GetType().GetElementType()!;

            if ( array.Rank > 1 )
            {
                throw SerializationDiagnosticDescriptors.MultidimensionalArray.CreateException( array.GetType() );
            }

            var lt = new List<ExpressionSyntax>();

            foreach ( var o in array )
            {
                ThrowIfStackTooDeep( o );
                lt.Add( this.Service.Serialize( o, serializationContext ) );
            }

            return ArrayCreationExpression(
                    ArrayType( serializationContext.GetTypeSyntax( elementType ) )
                        .WithRankSpecifiers( SingletonList( ArrayRankSpecifier( SingletonSeparatedList<ExpressionSyntax>( OmittedArraySizeExpression() ) ) ) ) )
                .WithInitializer(
                    InitializerExpression(
                        SyntaxKind.ArrayInitializerExpression,
                        SeparatedList( lt ) ) )
                .NormalizeWhitespace();
        }

        public override Type InputType => typeof(Array);

        public override Type? OutputType => throw new NotSupportedException();
    }
}