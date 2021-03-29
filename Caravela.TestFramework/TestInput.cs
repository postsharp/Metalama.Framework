﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Represents the parameters of the integration test input.
    /// </summary>
    public class TestInput
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestInput"/> class.
        /// </summary>
        /// <param name="testName">Short name of the test. Typically a relative path.</param>
        /// <param name="testSource">Full source of the input code.</param>
        /// <param name="testSourceFullPath">Full path to the file containing the source code.</param>
        public TestInput( string testName, string testSource )
        {
            this.TestName = testName;
            this.TestSource = testSource;
        }

        /// <summary>
        /// Gets the name of the test. Usually equals the relative path of the test source.
        /// </summary>
        public string TestName { get; }

        /// <summary>
        /// Gets the content of the test source file.
        /// </summary>
        public string TestSource { get; }


    }
}
