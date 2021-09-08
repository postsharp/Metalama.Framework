// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Stores async-local data about the current test.
    /// </summary>
    internal class TestExecutionContext : IDisposable
    {
        private static readonly AsyncLocal<TestExecutionContext> _current = new();

        private readonly ConcurrentQueue<UnloadableCompileTimeDomain> _domains = new();

        private TestExecutionContext() { }

        public static void RegisterDisposedDomain( UnloadableCompileTimeDomain domain )
        {
            _current.Value?._domains.Enqueue( domain );
        }

        public static TestExecutionContext Open()
        {
            TestExecutionContext executionContext = new();
            _current.Value = executionContext;

            return executionContext;
        }

        public void Dispose()
        {
            // GFR: The following code does not work because there are still, sometimes, GC roots to a Task<Result>.
            // I didn't manage to solve this issue in a couple of hours.

            /*
            foreach ( var domain in this._domains )
            {
                domain.WaitForDisposal();
            }
            */
        }
    }
}