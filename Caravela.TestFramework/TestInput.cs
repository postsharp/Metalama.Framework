// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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
        /// <param name="sourceCode">Full source of the input code.</param>
        private TestInput(
            string testName,
            string sourceCode,
            TestDirectoryOptionsReader? directoryOptionsReader = null,
            string? relativePath = null,
            string? fullPath = null )
        {
            this.TestName = testName;
            this.SourceCode = sourceCode;
            this.BaseDirectory = directoryOptionsReader?.ProjectDirectory;
            this.RelativePath = relativePath;
            this.FullPath = fullPath;

            if ( directoryOptionsReader != null )
            {
                this.Options.ApplyOptions(
                    sourceCode,
                    fullPath ?? throw new ArgumentNullException( nameof(fullPath) ),
                    directoryOptionsReader );
            }
            else
            {
                this.Options.ApplySourceDirectives( sourceCode );
            }
        }

        public static TestInput FromSource( string testName, string sourceCode ) => new( testName, sourceCode );

        internal static TestInput FromFile( TestDirectoryOptionsReader directoryOptionsReader, string relativePath )
        {
            var fullPath = Path.Combine( directoryOptionsReader.ProjectDirectory, relativePath );
            var sourceCode = File.ReadAllText( fullPath );

            return new TestInput( Path.GetFileNameWithoutExtension( relativePath ), sourceCode, directoryOptionsReader, relativePath, fullPath );
        }

        /// <summary>
        /// Gets the name of the test. Usually equals the relative path of the test source.
        /// </summary>
        public string TestName { get; }

        /// <summary>
        /// Gets the content of the test source file.
        /// </summary>
        public string SourceCode { get; }

        public string? BaseDirectory { get; }

        public string? RelativePath { get; }

        public string? FullPath { get; }

        public TestOptions Options { get; } = new();
    }
}