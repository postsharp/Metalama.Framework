using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal class CaravelaParameterInfoSerializer : TypedObjectSerializer<CompileTimeParameterInfo>
    {
        private readonly CaravelaMethodInfoSerializer _caravelaMethodInfoSerializer;

        public CaravelaParameterInfoSerializer( CaravelaMethodInfoSerializer caravelaMethodInfoSerializer )
        {
            this._caravelaMethodInfoSerializer = caravelaMethodInfoSerializer;
        }

        public override ExpressionSyntax Serialize( CompileTimeParameterInfo o )
        {
            var container = o.ContainingMember;
            var containerAsMember = container as IMember;
            var method = containerAsMember as Method;
            var property = containerAsMember as Property;
            var ordinal = o.ParameterSymbol.Ordinal;

            if ( method == null && property != null )
            {
                method = property.Setter as Method;
            }

            var retrieveMethodBase = this._caravelaMethodInfoSerializer.Serialize( new CompileTimeMethodInfo( method! ) );

            return ElementAccessExpression(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            retrieveMethodBase,
                            IdentifierName( "GetParameters" ) ) ) )
                .WithArgumentList(
                    BracketedArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal( ordinal ) ) ) ) ) )
                .NormalizeWhitespace();
        }
    }
}