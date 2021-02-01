using Caravela.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Advices
{

    [AttributeUsage( AttributeTargets.Method, Inherited = true )]
    public class OverrideMethodTemplateAttribute : TemplateAttribute { }

    public class OverrideMethodAttribute : OverrideMethodTemplateAttribute, IAdviceAttribute<IOverrideMethodAdvice>
    {
    }
}
