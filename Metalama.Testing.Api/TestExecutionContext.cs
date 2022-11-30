// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Metalama.Testing.Api
{
    /// <summary>
    /// Stores async-local data about the current test.
    /// </summary>
    public class TestExecutionContext : IDisposable
    {
        private static readonly AsyncLocal<TestExecutionContext> _current = new();

        private readonly ConcurrentQueue<Action> _disposeActions = new();

        private TestExecutionContext() { }

        public static void RegisterDisposeAction( Action action )
        {
            _current.Value?._disposeActions.Enqueue( action );
        }

        public static TestExecutionContext Open()
        {
            TestExecutionContext executionContext = new();
            _current.Value = executionContext;

            return executionContext;
        }

        public void Dispose()
        {
            foreach ( var action in this._disposeActions )
            {
                action();
            }
        }
    }
}