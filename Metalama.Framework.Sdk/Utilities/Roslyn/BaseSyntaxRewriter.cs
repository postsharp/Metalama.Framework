// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

/// <summary>
/// A derivation of <see cref="CSharpSyntaxVisitor{TResult}"/> that throws a <see cref="SyntaxProcessingException"/>
/// when an unhandled exception is detected while processing a node. 
/// </summary>
public abstract class SafeSyntaxVisitor<T> : CSharpSyntaxVisitor<T>
{
    public sealed override T? Visit( SyntaxNode? node )
    {
        try
        {
            return this.VisitCore( node );
        }
        catch ( Exception e ) when ( node != null )
        {
            throw new SyntaxProcessingException( node, e );
        }
    }

    protected virtual T? VisitCore( SyntaxNode? node )
    {
        return base.Visit( node );
    }
}