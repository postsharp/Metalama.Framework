﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.TestFramework
{
    public enum TestScenario
    {
        Transform,

        /// <summary>
        /// This value indicates that a code fix should be applied. When this value is set, the output buffer
        /// of the test is not the one transformed by the aspect, but the one transformed by the code fix. The test will fail
        /// if it does not generate any diagnostic with a code fix. By default, the first emitted code fix is applied.
        /// To apply a different code fix, use the <see cref="TestOptions.AppliedCodeFixIndex"/> property.
        /// To set this scenario in a test, add this comment to your test file: <c>// @ApplyCodeFix</c>.
        /// </summary>
        ApplyCodeFix,

        /// <summary>
        /// This value indicates that a code fix preview should be applied. When this value is set, the output buffer
        /// of the test is not the one transformed by the aspect, but the one transformed by the code fix. The test will fail
        /// if it does not generate any diagnostic with a code fix. By default, the first emitted code fix is applied.
        /// To apply a different code fix, use the <see cref="TestOptions.AppliedCodeFixIndex"/> property.
        /// To enable this option in a test, add this comment to your test file: <c>// @PreviewCodeFix</c>.
        /// </summary>
        PreviewCodeFix,

        /// <summary>
        /// This value indicates that a live template should be applied.
        /// To enable this option in a test, add this comment to your test file: <c>// @ApplyLiveTemplate</c>.
        /// </summary>
        ApplyLiveTemplate,

        /// <summary>
        /// This value indicates that a live template preview should be applied.
        /// To enable this option in a test, add this comment to your test file: <c>// @PreviewLiveTemplate</c>.
        /// </summary>
        PreviewLiveTemplate
    }
}