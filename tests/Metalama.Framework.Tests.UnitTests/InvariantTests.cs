// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine;
using System;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests
{
    public class InvariantTests
    {
        // These tests essentially verify that the behavior is the same in the debug and release mode.

        [Fact]
        public void AssertExecutesArgument()
        {
            var called = false;

            Invariant.Assert(
                Execute(
                    () =>
                    {
                        called = true;

                        return true;
                    } ) );

            Assert.True( called );
        }

        [Fact]
        public void ImpliesExecutesArgument()
        {
            var called = false;

            Invariant.Implies(
                true,
                Execute(
                    () =>
                    {
                        called = true;

                        return true;
                    } ) );

            Assert.True( called );
        }

        private static bool Execute( Func<bool> action ) => action();
    }
}