using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    public class ListSerializer : ObjectSerializer
    {
        private readonly ObjectSerializers _serializers;

        public ListSerializer( ObjectSerializers serializers ) => this._serializers = serializers;

        public override ExpressionSyntax Serialize( object o )
        {
            Type argument = o.GetType().GetGenericArguments()[0];
            
            List<SyntaxNodeOrToken> lt = new List<SyntaxNodeOrToken>();
            bool first = true;
            foreach ( var obj in (IEnumerable) o)
            {
                if ( !first )
                {
                    lt.Add( SyntaxFactory.Token( SyntaxKind.CommaToken ) );
                }

                try
                {
                    RuntimeHelpers.EnsureSufficientExecutionStack();
                }
                catch
                {
                    throw new CaravelaException( GeneralDiagnosticDescriptors.CycleInSerialization, obj );
                }
                lt.Add( this._serializers.SerializeToRoslynCreationExpression( obj ) );
                first = false;
            }
            var list = SyntaxFactory.SeparatedList<ExpressionSyntax>( lt );
            
            return ObjectCreationExpression(
                QualifiedName(
                        QualifiedName(
                            QualifiedName(
                                IdentifierName("System"),
                                IdentifierName("Collections")),
                            IdentifierName("Generic")),
                        GenericName(
                                Identifier("List"))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.ParseTypeName( TypeNameUtility.ToCSharpQualifiedName(argument) )
                                )))))
                    .WithInitializer(
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.CollectionInitializerExpression,
                        list ) )
                .NormalizeWhitespace(  );
        }
    }
}