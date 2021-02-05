using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    internal class IntSerializer : TypedObjectSerializer<int>
    {
        public override ExpressionSyntax Serialize( int o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    internal class CharSerializer : TypedObjectSerializer<char>
    {
        public override ExpressionSyntax Serialize( char o )
        {
            return LiteralExpression( SyntaxKind.CharacterLiteralExpression, Literal( o ) );
        }
    }

    internal class BoolSerializer : TypedObjectSerializer<bool>
    {
        public override ExpressionSyntax Serialize( bool o )
        {
            return LiteralExpression( o ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression );
        }
    }

    internal class ByteSerializer : TypedObjectSerializer<byte>
    {
        public override ExpressionSyntax Serialize( byte o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    internal class SByteSerializer : TypedObjectSerializer<sbyte>
    {
        public override ExpressionSyntax Serialize( sbyte o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    internal class UShortSerializer : TypedObjectSerializer<ushort>
    {
        public override ExpressionSyntax Serialize( ushort o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    internal class ShortSerializer : TypedObjectSerializer<short>
    {
        public override ExpressionSyntax Serialize( short o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    internal class UIntSerializer : TypedObjectSerializer<uint>
    {
        public override ExpressionSyntax Serialize( uint o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    internal class ULongSerializer : TypedObjectSerializer<ulong>
    {
        public override ExpressionSyntax Serialize( ulong o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    internal class LongSerializer : TypedObjectSerializer<long>
    {
        public override ExpressionSyntax Serialize( long o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    internal class FloatSerializer : TypedObjectSerializer<float>
    {
        public override ExpressionSyntax Serialize( float o )
        {
            if ( float.IsPositiveInfinity( o ) )
            {
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    PredefinedType(
                        Token( SyntaxKind.FloatKeyword ) ),
                    IdentifierName( "PositiveInfinity" ) );
            }
            else if ( float.IsNegativeInfinity( o ) )
            {
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    PredefinedType(
                        Token( SyntaxKind.FloatKeyword ) ),
                    IdentifierName( "NegativeInfinity" ) );
            }
            else if ( float.IsNaN( o ) )
            {
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    PredefinedType(
                        Token( SyntaxKind.FloatKeyword ) ),
                    IdentifierName( "NaN" ) );
            }

            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    internal class DoubleSerializer : TypedObjectSerializer<double>
    {
        public override ExpressionSyntax Serialize( double o )
        {
            if ( double.IsPositiveInfinity( o ) )
            {
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    PredefinedType(
                        Token( SyntaxKind.DoubleKeyword ) ),
                    IdentifierName( "PositiveInfinity" ) );
            }
            else if ( double.IsNegativeInfinity( o ) )
            {
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    PredefinedType(
                        Token( SyntaxKind.DoubleKeyword ) ),
                    IdentifierName( "NegativeInfinity" ) );
            }
            else if ( double.IsNaN( o ) )
            {
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    PredefinedType(
                        Token( SyntaxKind.DoubleKeyword ) ),
                    IdentifierName( "NaN" ) );
            }

            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    internal class DecimalSerializer : TypedObjectSerializer<decimal>
    {
        public override ExpressionSyntax Serialize( decimal o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    internal class UIntPtrSerializer : TypedObjectSerializer<UIntPtr>
    {
        public override ExpressionSyntax Serialize( UIntPtr o )
        {
            return ObjectCreationExpression(
                    QualifiedName(
                        IdentifierName( "System" ),
                        IdentifierName( "UIntPtr" ) ) )
                .AddArgumentListArguments(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal( o.ToUInt64() ) ) ) );
        }
    }

    internal class IntPtrSerializer : TypedObjectSerializer<IntPtr>
    {
        public override ExpressionSyntax Serialize( IntPtr o )
        {
            return ObjectCreationExpression(
                    QualifiedName(
                        IdentifierName( "System" ),
                        IdentifierName( "IntPtr" ) ) )
                .AddArgumentListArguments(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal( o.ToInt64() ) ) ) );
        }
    }

    internal class StringSerializer : TypedObjectSerializer<string>
    {
        public override ExpressionSyntax Serialize( string o )
        {
            return LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( o ) );
        }
    }
}