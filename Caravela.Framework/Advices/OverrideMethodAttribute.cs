using Caravela.Framework.Aspects;
using System;

namespace Caravela.Framework.Advices
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OverrideMethodAttribute : TemplateAttribute
    {
    }
}
