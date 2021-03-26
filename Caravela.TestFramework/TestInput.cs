// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.TestFramework
{
    /// <summary>
    /// Represents the parameters of the integration test input.
    /// </summary>
    public class TestInput
    {
        public TestInput( string testName, string projectDirectory, string testSource, string testSourcePath )
        {
            this.TestName = testName;
            this.ProjectDirectory = projectDirectory;
            this.TestSource = testSource;
            this.TestSourcePath = testSourcePath;
        }

        /// <summary>
        /// Gets the name of the test. Usually equals the relative path of the test source.
        /// </summary>
        public string TestName { get; }

        /// <summary>
        /// Gets the project directory.
        /// </summary>
        public string ProjectDirectory { get; }

        /// <summary>
        /// Gets the content of the test source file.
        /// </summary>
        public string TestSource { get; }

        /// <summary>
        /// Gets the path of the test source file.
        /// </summary>
        public string TestSourcePath { get; }
    }
}
