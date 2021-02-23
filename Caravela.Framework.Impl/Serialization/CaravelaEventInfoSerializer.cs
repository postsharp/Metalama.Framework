using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal class CaravelaEventInfoSerializer : TypedObjectSerializer<CompileTimeEventInfo>
    {
        private readonly CaravelaTypeSerializer _caravelaTypeSerializer;

        public CaravelaEventInfoSerializer( CaravelaTypeSerializer caravelaTypeSerializer )
        {
            this._caravelaTypeSerializer = caravelaTypeSerializer;
        }

        public override ExpressionSyntax Serialize( CompileTimeEventInfo o )
        {
            var eventName = o.Symbol.Name;
            var typeCreation = this._caravelaTypeSerializer.Serialize( CompileTimeType.Create( o.ContainingType ) );
            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        typeCreation,
                        IdentifierName( "GetEvent" ) ) )
                .AddArgumentListArguments(
                    Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( eventName ) ) ) )
                .NormalizeWhitespace();
        }
    }
}