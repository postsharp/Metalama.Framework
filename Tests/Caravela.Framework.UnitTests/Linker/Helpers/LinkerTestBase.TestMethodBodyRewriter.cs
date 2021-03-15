// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.Linking;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.UnitTests.Linker.Helpers
{
    public partial class LinkerTestBase
    {
        private class TestMethodBodyRewriter : CSharpSyntaxRewriter
        {
            private readonly TestRewriter _owner;
            private readonly MethodDeclarationSyntax _currentMethod;
            private readonly string _aspectName;
            private readonly string? _layerName;

            public TestMethodBodyRewriter( TestRewriter owner, MethodDeclarationSyntax currentMethod, string aspectName, string? layerName )
            {
                this._owner = owner;
                this._currentMethod = currentMethod;
                this._aspectName = aspectName;
                this._layerName = layerName;
            }

            public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
            {
                if ( node.Expression is IdentifierNameSyntax identifier && identifier.Identifier.ValueText == "link" )
                {
                    // TODO: Annotation order.
                    if ( node.ArgumentList.Arguments.Count != 1 && node.ArgumentList.Arguments.Count != 2 )
                    {
                        throw new ArgumentException( "link method should have 1 or 2 arguments." );
                    }

                    var callExpression = node.ArgumentList.Arguments[0].Expression;

                    string? tag = null;
                    if ( node.ArgumentList.Arguments.Count == 2 )
                    {
                        tag = node.ArgumentList.Arguments[1].ToString();
                    }

                    if ( tag != null )
                    {
                        throw new ArgumentException( $"unsupported link() tag {tag}" );
                    }

                    return callExpression.AddLinkerAnnotation( new LinkerAnnotation( new AspectLayerId( this._aspectName, this._layerName ), LinkerAnnotationOrder.Default ) );
                }

                return base.VisitInvocationExpression( node );
            }
        }
    }
}
