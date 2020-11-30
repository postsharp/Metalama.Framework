using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaParameterInfoSerializer : TypedObjectSerializer<CaravelaParameterInfo>
    {
        private readonly CaravelaMethodInfoSerializer _caravelaMethodInfoSerializer;

        public CaravelaParameterInfoSerializer( CaravelaMethodInfoSerializer caravelaMethodInfoSerializer ) => this._caravelaMethodInfoSerializer = caravelaMethodInfoSerializer;

        public override ExpressionSyntax Serialize( CaravelaParameterInfo o )
        {
            // TODO generics
            // TODO properties
            // TODO lots of side cases
            IParameterSymbol symbol = o.Symbol;
            ICodeElement container = o.ContainingMember;
            IMember containerAsMember = container as IMember;
            var method = containerAsMember as Method;
            var property = containerAsMember as Property;
            int ordinal = o.Symbol.Ordinal;

            // Emit load method
            // Emit load parameter info
            if ( method == null && property != null )
            {
                method = property.Setter as Method;
            }

            var retrieveMethodBase = this._caravelaMethodInfoSerializer.Serialize( new CaravelaMethodInfo( method! ) );

            return ElementAccessExpression(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            retrieveMethodBase,
                            IdentifierName( "GetParameters" ) ) ) )
                .WithArgumentList(
                    BracketedArgumentList(
                        SingletonSeparatedList<ArgumentSyntax>(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal( ordinal ) ) ) ) ) )
                .NormalizeWhitespace();
        }
    }
}