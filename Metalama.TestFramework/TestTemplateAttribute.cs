// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System;
using System.ComponentModel;

namespace Caravela.TestFramework
{
    // The attribute that marks a template method in the templating integration tests.

    /// <exclude />
    [AttributeUsage( AttributeTargets.Method )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public sealed class TestTemplateAttribute : TemplateAttribute { }
}