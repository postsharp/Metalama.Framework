// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Caravela.Framework.Impl.Testing
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