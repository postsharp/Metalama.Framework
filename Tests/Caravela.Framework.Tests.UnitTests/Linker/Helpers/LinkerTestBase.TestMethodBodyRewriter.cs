// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Tests.UnitTests.Linker.Helpers
{
    public partial class LinkerTestBase
    {
        private class TestMethodBodyRewriter : CSharpSyntaxRewriter
        {
            private readonly string _aspectName;
            private readonly string? _layerName;

            public TestMethodBodyRewriter( string aspectName, string? layerName )
            {
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

                    var annotatedExpression = node.ArgumentList.Arguments[0].Expression;

                    AspectReferenceFlags flags = default;

                    // Since most of the linker tests linking normal aspects, the default order for linker tests is Base.
                    var order = AspectReferenceOrder.Base;

                    for ( var i = 1; i < node.ArgumentList.Arguments.Count; i++ )
                    {
                        var tag = node.ArgumentList.Arguments[i].ToString();

                        switch ( tag )
                        {
                            case "inline":
                                flags |= AspectReferenceFlags.Inlineable;

                                break;

                            case "next":
                                order = AspectReferenceOrder.Next;

                                break;

                            case "original":
                                order = AspectReferenceOrder.Original;

                                break;

                            case "final":
                                order = AspectReferenceOrder.Final;

                                break;

                            default:
                                throw new ArgumentException( $"unsupported link() tag {tag}" );
                        }
                    }

                    var target = AspectReferenceTargetKind.Self;

                    if ( annotatedExpression is MemberAccessExpressionSyntax memberAccess )
                    {
                        switch ( memberAccess.Name.Identifier.ValueText )
                        {
                            case "get":
                                annotatedExpression = memberAccess.Expression;
                                target = AspectReferenceTargetKind.PropertyGetAccessor;

                                break;

                            case "set":
                                annotatedExpression = memberAccess.Expression;
                                target = AspectReferenceTargetKind.PropertySetAccessor;

                                break;

                            case "add":
                                annotatedExpression = memberAccess.Expression;
                                target = AspectReferenceTargetKind.EventAddAccessor;

                                break;

                            case "remove":
                                annotatedExpression = memberAccess.Expression;
                                target = AspectReferenceTargetKind.EventRemoveAccessor;

                                break;
                        }
                    }

                    return annotatedExpression.WithAspectReferenceAnnotation( new AspectLayerId( this._aspectName, this._layerName ), order, target, flags );
                }

                return base.VisitInvocationExpression( node );
            }
        }
    }
}