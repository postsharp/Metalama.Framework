using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaReturnParameterInfoSerializer : TypedObjectSerializer<CaravelaReturnParameterInfo>
    {
        private readonly CaravelaMethodInfoSerializer _methodInfoSerializer;

        public CaravelaReturnParameterInfoSerializer( CaravelaMethodInfoSerializer methodInfoSerializer ) => this._methodInfoSerializer = methodInfoSerializer;

        public override ExpressionSyntax Serialize( CaravelaReturnParameterInfo o )
        {
            var methodBaseExpression = this._methodInfoSerializer.Serialize( new CaravelaMethodInfo( o.Method ) );
            return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ParenthesizedExpression(
                        CastExpression(
                            QualifiedName(
                                QualifiedName(
                                    IdentifierName( "System" ),
                                    IdentifierName( "Reflection" ) ),
                                IdentifierName( "MethodInfo" ) ),
                            methodBaseExpression ) ),
                    IdentifierName( "ReturnParameter" ) )
                .NormalizeWhitespace();
        }
    }
}