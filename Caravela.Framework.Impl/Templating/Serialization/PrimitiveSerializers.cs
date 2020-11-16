using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    public class IntSerializer : TypedObjectSerializer<int>
    {
        public override ExpressionSyntax Serialize( int o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    public class CharSerializer : TypedObjectSerializer<char>
    {
        public override ExpressionSyntax Serialize( char o )
        {
            return LiteralExpression( SyntaxKind.CharacterLiteralExpression, Literal( o ) );
        }
    }

    public class BoolSerializer : TypedObjectSerializer<bool>
    {
        public override ExpressionSyntax Serialize( bool o )
        {
            return LiteralExpression(o ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
        }
    }

    public class ByteSerializer : TypedObjectSerializer<byte>
    {
        public override ExpressionSyntax Serialize( byte o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    public class SByteSerializer : TypedObjectSerializer<sbyte>
    {
        public override ExpressionSyntax Serialize( sbyte o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    public class UShortSerializer : TypedObjectSerializer<ushort>
    {
        public override ExpressionSyntax Serialize( ushort o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    public class ShortSerializer : TypedObjectSerializer<short>
    {
        public override ExpressionSyntax Serialize( short o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    public class UIntSerializer : TypedObjectSerializer<uint>
    {
        public override ExpressionSyntax Serialize( uint o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    public class ULongSerializer : TypedObjectSerializer<ulong>
    {
        public override ExpressionSyntax Serialize( ulong o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    public class LongSerializer : TypedObjectSerializer<long>
    {
        public override ExpressionSyntax Serialize( long o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    public class FloatSerializer : TypedObjectSerializer<float>
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

    public class DoubleSerializer : TypedObjectSerializer<double>
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

    public class DecimalSerializer : TypedObjectSerializer<decimal>
    {
        public override ExpressionSyntax Serialize( decimal o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }

    public class UIntPtrSerializer : TypedObjectSerializer<UIntPtr>
    {
        public override ExpressionSyntax Serialize( UIntPtr o )
        {
            return ObjectCreationExpression(
                    QualifiedName(
                        IdentifierName("System"),
                        IdentifierName("UIntPtr")))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal( o.ToUInt64() ) ) ) ) ) );
        }
    }

    public class IntPtrSerializer : TypedObjectSerializer<IntPtr>
    {
        public override ExpressionSyntax Serialize( IntPtr o )
        {
            return ObjectCreationExpression(
                    QualifiedName(
                        IdentifierName("System"),
                        IdentifierName("IntPtr")))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal( o.ToInt64() ) ) ) ) ) );
        }
    }

    public class StringSerializer : TypedObjectSerializer<string>
    {
        public override ExpressionSyntax Serialize( string o )
        {
            return LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( o ) );
        }
    }
}