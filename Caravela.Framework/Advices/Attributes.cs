using Caravela.Framework.Aspects;
using System;

namespace Caravela.Framework.Advices
{
    public interface IAdviceAttribute { }

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class OverrideMethodTemplateAttribute : TemplateAttribute { }
}
