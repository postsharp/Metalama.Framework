// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal static class EnumSerializer
    {
        private static readonly Type[] _unsignedTypes = { typeof(ushort), typeof(uint), typeof(ulong), typeof(byte) };

        public static ExpressionSyntax Serialize( Enum o )
        {
            var enumType = o.GetType();
            var typeName = TypeNameUtility.ToCSharpQualifiedName( enumType );
            var name = Enum.GetName( enumType, o );

            if ( name != null )
            {
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ParseExpression( typeName ),
                    IdentifierName( name ) );
            }

            var underlyingType = Enum.GetUnderlyingType( o.GetType() );
            var literal = _unsignedTypes.Contains( underlyingType ) ? Literal( Convert.ToUInt64( o ) ) : Literal( Convert.ToInt64( o ) );

            return CastExpression(
                ParseTypeName( typeName ),
                ParenthesizedExpression(
                    LiteralExpression(
                        SyntaxKind.NumericLiteralExpression,
                        literal ) ) );
        }
    }
}