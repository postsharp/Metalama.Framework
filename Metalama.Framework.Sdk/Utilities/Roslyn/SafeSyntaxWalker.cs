// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

/// <summary>
/// A derivation of <see cref="CSharpSyntaxWalker"/> that throws a <see cref="SyntaxProcessingException"/>
/// when an unhandled exception is detected while processing a node.
/// Also prevents <see cref="InsufficientExecutionStackException "/> for deeply nested trees.
/// </summary>
[PublicAPI]
public abstract class SafeSyntaxWalker : CSharpSyntaxWalker
{
    protected SafeSyntaxWalker( SyntaxWalkerDepth depth = SyntaxWalkerDepth.Node ) : base( depth ) { }

    private int _recursionDepth;

    // InsufficientExecutionStackException can be observed when this is > 900, so set it to a value that is significantly smaller than that, to be safe.
    private const int _maxRecursionDepth = 750;

    public sealed override void Visit( SyntaxNode? node )
    {
        try
        {
            this._recursionDepth++;

            // If there is a risk of running out of stack space on the current thread, switch to a different thread.
            // Note that RuntimeHelpers.EnsureSufficientExecutionStack wouldn't work here, since it doesn't take into account executing potentially
            // deeply recursive Roslyn methods.

            if ( this._recursionDepth % _maxRecursionDepth == 0 )
            {
                // The ContinueWith is used to prevent inline execution of the Task.

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                Task.Run( () => this.VisitCore( node ) ).ContinueWith( _ => { }, TaskScheduler.Default ).Wait();
#pragma warning restore VSTHRD002
            }
            else
            {
                this.VisitCore( node );
            }

            this._recursionDepth--;
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