using Caravela.Framework.Aspects;
using System;

namespace Caravela.Framework.Advices
{
    public interface IAdviceAttribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class OverrideMethodTemplateAttribute : TemplateAttribute { }
}
