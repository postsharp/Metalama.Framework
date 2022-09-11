// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;
using System.ComponentModel;

namespace Metalama.TestFramework
{
    // The attribute that marks a template method in the templating integration tests.

    /// <exclude />
    [AttributeUsage( AttributeTargets.Method )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public sealed class TestTemplateAttribute : TemplateAttribute { }
}