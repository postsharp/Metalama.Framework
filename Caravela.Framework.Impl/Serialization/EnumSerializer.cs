// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Globalization;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class EnumSerializer : ObjectSerializer
    {
        private static readonly Type[] _unsignedTypes = { typeof( ushort ), typeof( uint ), typeof( ulong ), typeof( byte ) };

        public override ExpressionSyntax Serialize( object obj, SyntaxSerializationContext serializationContext )
        {
            var o = (Enum) obj;

            var enumType = o.GetType();
            var typeName = serializationContext.GetTypeSyntax( enumType );
            var name = Enum.GetName( enumType, o );

            if ( name != null )
            {
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    typeName,
                    IdentifierName( name ) );
            }

            var underlyingType = Enum.GetUnderlyingType( o.GetType() );

            var literal = _unsignedTypes.Contains( underlyingType )
                ? Literal( Convert.ToUInt64( o, CultureInfo.InvariantCulture ) )
                : Literal( Convert.ToInt64( o, CultureInfo.InvariantCulture ) );

            return CastExpression(
                typeName,
                ParenthesizedExpression(
                    LiteralExpression(
                        SyntaxKind.NumericLiteralExpression,
                        literal ) ) );
        }

        public override Type InputType => typeof( Enum );

        public override Type? OutputType => throw new NotSupportedException();

        public EnumSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}