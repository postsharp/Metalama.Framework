// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.TestFramework
{
    /// <summary>
    /// A set of test options, which can be included in the source text of tests using special comments like <c>// @IncludeFinalDiagnostics</c>.
    /// </summary>
    public class TestOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the diagnostics of the compilation of the transformed target code should be included in the test result.
        /// This is useful when diagnostic suppression is being tested.
        /// </summary>
        public bool IncludeFinalDiagnostics { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether diagnostics of all severities should be included in the rest result. By default, only
        /// warnings and errors are included. 
        /// </summary>
        public bool IncludeAllSeverities { get; set; }
        
        public TestRunnerKind TestRunnerKind { get; set; }
    }
}