// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Xunit;
using Xunit.Abstractions;

namespace Caravela.TestFramework.XunitFramework
{
    internal class Test : LongLivedMarshalByRefObject, ITest
    {
        public string DisplayName => this.TestCase.DisplayName;

        public ITestCase TestCase { get; }

        public Test( ITestCase testCase )
        {
            this.TestCase = testCase;
        }
    }
}