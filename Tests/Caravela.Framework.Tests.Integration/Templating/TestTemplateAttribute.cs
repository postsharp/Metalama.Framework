// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System;

namespace Caravela.Framework.Tests.Integration.Templating
{
    /// <summary>
    /// The attribute that marks a template method in the templating integration tests.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public sealed class TestTemplateAttribute : TemplateAttribute
    {
    }
}