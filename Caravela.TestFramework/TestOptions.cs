// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// A set of test options, which can be included in the source text of tests using special comments like <c>// @IncludeFinalDiagnostics</c>.
    /// </summary>
    public class TestOptions
    {
        private static readonly Regex _directivesRegex = new( "^// @(\\w+)" );

        public string? SkipReason { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the diagnostics of the compilation of the transformed target code should be included in the test result.
        /// This is useful when diagnostic suppression is being tested.
        /// </summary>
        public bool? IncludeFinalDiagnostics { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether diagnostics of all severities should be included in the rest result. By default, only
        /// warnings and errors are included. 
        /// </summary>
        public bool? IncludeAllSeverities { get; set; }

        /// <summary>
        /// Gets or sets the fully-qualified name of the test runner factory type (implementing <see cref="ITestRunnerFactory"/>).
        /// </summary>
        public string? TestRunnerFactoryType { get; set; }

        public List<string> Assemblies { get; } = new List<string>();

        public virtual void ApplyDirectoryOptions( TestDirectoryOptions directoryOptions )
        {
            if ( this.SkipReason == null )
            {
                this.SkipReason = directoryOptions.SkipReason;
            }

            if ( this.IncludeFinalDiagnostics == null )
            {
                this.IncludeFinalDiagnostics = directoryOptions.IncludeFinalDiagnostics;
            }

            if ( this.IncludeAllSeverities == null )
            {
                this.IncludeAllSeverities = directoryOptions.IncludeAllSeverities;
            }

            if ( this.TestRunnerFactoryType == null )
            {
                this.TestRunnerFactoryType = directoryOptions.TestRunnerFactoryType;
            }
        }

        public void ApplySourceDirectives( string sourceCode )
        {
            foreach ( Match? directive in _directivesRegex.Matches( sourceCode ) )
            {
                if ( directive == null )
                {
                    continue;
                }

                var directiveName = directive.Groups[1].Value;

                switch ( directiveName )
                {
                    case "IncludeFinalDiagnostics":
                        this.IncludeFinalDiagnostics = true;

                        break;

                    case "IncludeAllSeverities":
                        this.IncludeAllSeverities = true;

                        break;
                    
                    case "Skipped":
                        this.SkipReason = "Skipped by directive in source code.";
                        
                        break;

                    case "DesignTime":
                        this.TestRunnerFactoryType = "Caravela.Framework.Tests.Integration.Runners.DesignTimeTestRunnerFactory, Caravela.Framework.Tests.Integration";

                        break;
                }
            }
        }

        internal void ApplyOptions( string sourceCode, string path, TestDirectoryOptionsReader optionsReader )
        {
            this.ApplySourceDirectives( sourceCode );
            this.ApplyDirectoryOptions( optionsReader.GetDirectoryOptions( Path.GetDirectoryName( path )! ) );
        }
    }
}