// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Testing.AspectTesting.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Metalama.Testing.AspectTesting
{
    /// <summary>
    /// Represents the parameters of the integration test input.
    /// </summary>
    internal sealed class TestInput
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestInput"/> class.
        /// </summary>
        /// <param name="testName">Short name of the test. Typically a relative path.</param>
        /// <param name="sourceCode">Full source of the input code.</param>
        private TestInput(
            TestProjectProperties projectProperties,
            string testName,
            string sourceCode,
            TestDirectoryOptionsReader? directoryOptionsReader = null,
            string? projectDirectory = null,
            string? relativePath = null,
            string? fullPath = null )
        {
            this.ProjectProperties = projectProperties;
            this.TestName = testName;
            this.SourceCode = sourceCode;
            this.RelativePath = relativePath;
            this.FullPath = fullPath;
            this.ProjectDirectory = projectDirectory ?? projectProperties.ProjectDirectory;

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

            if ( fullPath != null )
            {
                // Find companion files.
                var directory = Path.GetDirectoryName( fullPath )!;

                foreach ( var companionFile in Directory.EnumerateFiles( directory, Path.GetFileNameWithoutExtension( fullPath ) + ".*.cs" ) )
                {
                    if ( !companionFile.EndsWith( ".t.cs", StringComparison.OrdinalIgnoreCase ) )
                    {
                        this.Options.IncludedFiles.Add( PathUtil.GetRelativePath( directory, companionFile ) );
                    }
                }
            }

            this.SkipReason = this.Options.SkipReason;

            if ( !this.IsSkipped )
            {
                var missingConstants = this.Options.RequiredConstants.Where( c => !this.ProjectProperties.PreprocessorSymbols.Contains( c ) ).ToList();

                if ( missingConstants.Count > 0 )
                {
                    this.SkipReason =
                        $"The following constant(s) are not defined: {string.Join( ", ", missingConstants.SelectEnumerable( c => "'" + c + "'" ) )}.";
                }
            }
        }

        private TestInput(
            TestProjectProperties projectProperties,
            string testName,
            string sourceCode,
            string projectDirectory,
            string? relativePath,
            string? fullPath,
            TestOptions options,
            string? skipReason )
        {
            this.ProjectProperties = projectProperties;
            this.TestName = testName;
            this.SourceCode = sourceCode;
            this.ProjectDirectory = projectDirectory;
            this.RelativePath = relativePath;
            this.FullPath = fullPath;
            this.Options = options;
            this.SkipReason = skipReason;
        }

        [ExcludeFromCodeCoverage]
        internal static TestInput FromSource( TestProjectProperties projectProperties, string sourceCode, string path )
        {
            var projectDirectory = FindProjectDirectory( Path.GetDirectoryName( path ) );

            if ( projectDirectory != null )
            {
                var directoryOptionsReader = new TestDirectoryOptionsReader( projectDirectory );

                return new TestInput(
                    projectProperties,
                    Path.GetFileNameWithoutExtension( path ),
                    sourceCode,
                    directoryOptionsReader,
                    projectDirectory,
                    PathUtil.GetRelativePath( projectDirectory, path ),
                    path );
            }
            else
            {
                // Coverage: ignore
                // The project could not be found. Continue without reading directory options.

                return new TestInput( projectProperties, "interactive", sourceCode );
            }

            static string? FindProjectDirectory( string? directory )
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
        }

        internal static TestInput FromFile( TestProjectProperties projectProperties, TestDirectoryOptionsReader directoryOptionsReader, string relativePath )
        {
            var fullPath = Path.Combine( directoryOptionsReader.ProjectDirectory, relativePath );
            var sourceCode = File.ReadAllText( fullPath );

            return new TestInput(
                projectProperties,
                Path.GetFileNameWithoutExtension( relativePath ),
                sourceCode,
                directoryOptionsReader,
                projectProperties.ProjectDirectory,
                relativePath,
                fullPath );
        }

        internal TestInput WithSource( string newSource )
        {
            return new TestInput(
                this.ProjectProperties,
                this.TestName,
                newSource,
                this.ProjectDirectory,
                this.RelativePath,
                this.FullPath,
                this.Options,
                this.SkipReason );
        }

        /// <summary>
        /// Gets the project properties for the current test.
        /// </summary>
        internal TestProjectProperties ProjectProperties { get; }

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
        public string ProjectDirectory { get; }

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

        /// <summary>
        /// Gets the reason why the current test must be skipped, or <c>null</c> if it must not be skipped.
        /// </summary>
        public string? SkipReason { get; }

        /// <summary>
        /// Gets a value indicating whether the current test must be skipped.
        /// </summary>
        public bool IsSkipped => this.SkipReason != null;
    }
}