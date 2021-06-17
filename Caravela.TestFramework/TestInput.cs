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
            this.ProjectDirectory = directoryOptionsReader?.ProjectDirectory;
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

        private static string? FindProjectDirectory( string? directory )
        {
            if ( directory == null )
            {
                return null;
            }

            if ( Directory.GetFiles( directory, "*.csproj" ).Length > 0 )
            {
                return directory;
            }
            else
            {
                var parentDirectory = Path.GetDirectoryName( directory );

                return FindProjectDirectory( parentDirectory );
            }
        }

        public static TestInput FromSource( string sourceCode, string? path )
        {
            if ( path != null )
            {
                var projectDirectory = FindProjectDirectory( Path.GetDirectoryName( path ) );

                if ( projectDirectory != null )
                {
                    var directoryOptionsReader = new TestDirectoryOptionsReader( projectDirectory );

                    return new TestInput(
                        Path.GetFileNameWithoutExtension( path ),
                        sourceCode,
                        directoryOptionsReader,
                        Path.GetRelativePath( projectDirectory, path ),
                        path );
                }
            }

            return new TestInput( "interactive", sourceCode );
        }

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

        /// <summary>
        /// Gets the directory containing the project (<c>csproj</c>) file.
        /// </summary>
        public string? ProjectDirectory { get; }

        /// <summary>
        /// Gets the path of the current test file relatively to <see cref="ProjectDirectory"/>.
        /// </summary>
        public string? RelativePath { get; }

        /// <summary>
        /// Gets the full path of the current test file.
        /// </summary>
        public string? FullPath { get; }

        /// <summary>
        /// Gets the options of the current test.
        /// </summary>
        public TestOptions Options { get; } = new();
    }
}