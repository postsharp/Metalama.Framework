// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Xunit.Abstractions;

namespace Caravela.TestFramework.XunitFramework
{
    internal class Test : ITest
    {
        public string DisplayName => this.TestCase.DisplayName;

        public ITestCase TestCase { get; }

        public Test( ITestCase testCase )
        {
            this.TestCase = testCase;
        }
    }
}