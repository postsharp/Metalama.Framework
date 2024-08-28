﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;

namespace Metalama.Testing.AspectTesting
{
    /// <summary>
    /// Enumerates the scenarios that are simulated for the test, for instance compilation (the <see cref="Default"/> scenario),
    /// applying a code fix, or others.
    /// </summary>
    [PublicAPI]
    public enum TestScenario
    {
        /// <summary>
        /// The default test scenario is that the code is transformed as during compilation.
        /// </summary>
        Default,

        /// <summary>
        /// Tests the application of a code fix. By default, the first suggested code fix is applied.
        /// To apply a different code fix, use the <see cref="TestOptions.AppliedCodeFixIndex"/> property.
        /// To set this scenario in a test, add this comment to your test file: <c>// @TestScenario(CodeFix)</c>.
        /// </summary>
        CodeFix,

        /// <summary>
        /// Tests the preview of a code fix. By default, the first suggested code fix is applied.
        /// To apply a different code fix, use the <see cref="TestOptions.AppliedCodeFixIndex"/> property.
        /// To enable this option in a test, add this comment to your test file: <c>// @TestScenario(CodeFixPreview)</c>.
        /// </summary>
        CodeFixPreview,

        /// <summary>
        /// Tests the application of an aspect as a live template. The test file must contain a single attribute of
        /// type <see cref="TestLiveTemplateAttribute"/> indicating the target and the type of the aspect to be applied.
        /// To enable this option in a test, add this comment to your test file: <c>// @TestScenario(LiveTemplate)</c>.
        /// </summary>
        LiveTemplate,

        /// <summary>
        /// Tests the preview of an aspect as a live template. The test file must contain a single attribute of
        /// type <see cref="TestLiveTemplateAttribute"/> indicating the target and the type of the aspect to be applied.
        /// To enable this option in a test, add this comment to your test file: <c>// @TestScenario(LiveTemplatePreview)</c>.
        /// </summary>
        LiveTemplatePreview,

        /// <summary>
        /// Tests the background code and diagnostic generation at design time.
        /// To enable this option in a test, add this comment to your test file: <c>// @TestScenario(DesignTime)</c>. 
        /// </summary>
        DesignTime,

        /// <summary>
        /// Tests the output of the "diff preview" feature.
        /// To enable this option in a test, add this comment to your test file: <c>// @TestScenario(Preview)</c>. 
        /// </summary>
        Preview
    }
}