// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Text.RegularExpressions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Represents the parameters of the integration test input.
    /// </summary>
    public class TestInput
    {
        private static readonly Regex _directivesRegex = new( "^// @(\\w+)" );

        /// <summary>
        /// Initializes a new instance of the <see cref="TestInput"/> class.
        /// </summary>
        /// <param name="testName">Short name of the test. Typically a relative path.</param>
        /// <param name="testSource">Full source of the input code.</param>
        public TestInput( string testName, string testSource )
        {
            this.TestName = testName;
            this.TestSource = testSource;

            foreach ( Match? directive in _directivesRegex.Matches( testSource ) )
            {
                if ( directive == null )
                {
                    continue;
                }

                var directiveName = directive.Groups[1].Value;

                switch ( directiveName )
                {
                    case "IncludeFinalDiagnostics":
                        this.Options.IncludeFinalDiagnostics = true;

                        break;

                    case "IncludeAllSeverities":
                        this.Options.IncludeAllSeverities = true;

                        break;

                    case "DesignTime":
                        this.Options.TestRunnerKind = TestRunnerKind.DesignTime;

                        break;
                }
            }
        }

        /// <summary>
        /// Gets the name of the test. Usually equals the relative path of the test source.
        /// </summary>
        public string TestName { get; }

        /// <summary>
        /// Gets the content of the test source file.
        /// </summary>
        public string TestSource { get; }

        public TestOptions Options { get; } = new();
    }

    public enum TestRunnerKind
    {
        Default,
        Template,
        DesignTime
    }
}