// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

/// <summary>
/// A derivation of <see cref="CSharpSyntaxVisitor"/> that throws a <see cref="SyntaxProcessingException"/>
/// when an unhandled exception is detected while processing a node. 
/// </summary>
public abstract class SafeSyntaxVisitor : CSharpSyntaxVisitor
{
    public sealed override void Visit( SyntaxNode? node )
    {
        try
        {
            this.VisitCore( node );
        }
        catch ( Exception e ) when ( SyntaxProcessingException.ShouldWrapException( e, node ) )
        {
            throw new SyntaxProcessingException( e, node );
        }
    }

    protected virtual void VisitCore( SyntaxNode? node )
    {
        base.Visit( node );
    }
}