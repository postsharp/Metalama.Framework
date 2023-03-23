// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

/// <summary>
/// <para>If there is a risk of running out of stack space on the current thread, switches to a different thread.
/// Note that <see cref="RuntimeHelpers.EnsureSufficientExecutionStack" />  wouldn't work here, since it doesn't take into account executing potentially
/// deeply recursive Roslyn methods.</para>
/// <para>Roslyn does not support unlimited recursion depth and supporting it here would mask infinite recursion bugs,
/// so the switching is limited to just a small number of tasks.</para>
/// <para>Note that this is a mutable struct, so make sure not to store it in a <see langword="readonly" /> field.</para>
/// </summary>
internal struct RecursionGuard
{
    private int _recursionDepth;

    // InsufficientExecutionStackException can be observed in SafeSyntaxWalker when this is > 900, so set it to a value that is significantly smaller than that, to be safe.
    private const int _maxRecursionDepth = 750;
    private const int _maxTasks = 4;

    public void IncrementDepth() => this._recursionDepth++;
    public void DecrementDepth() => this._recursionDepth--;

    public readonly bool ShouldSwitch => this._recursionDepth % _maxRecursionDepth == 0 && this._recursionDepth <= _maxRecursionDepth * _maxTasks;

    public readonly void Switch<TState>( TState state, Action<TState> recursiveAction )
    {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        // The ContinueWith is used to prevent inline execution of the Task.
        Task.Run( () => recursiveAction( state ) ).ContinueWith( _ => { }, TaskScheduler.Default ).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
    }

    public readonly TResult Switch<TState, TResult>( TState state, Func<TState, TResult> recursiveFunction )
    {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        // The ContinueWith is used to prevent inline execution of the Task.
        return Task.Run( () => recursiveFunction( state ) ).ContinueWith( task => task.GetAwaiter().GetResult(), TaskScheduler.Default ).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
    }

}
