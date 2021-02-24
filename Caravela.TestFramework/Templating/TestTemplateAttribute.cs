using Caravela.Framework.Aspects;
using System;

namespace Caravela.TestFramework.Templating
{
    /// <summary>
    /// The attribute that marks a template method in the templating integration tests.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public sealed class TestTemplateAttribute : TemplateAttribute
    {
    }
}