using Caravela.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Advices
{
    [AttributeUsage( AttributeTargets.Method, Inherited = true )]
    public class IntroduceMethodTemplateAttribute : TemplateAttribute { }

    [AttributeUsage( AttributeTargets.Method, Inherited = true )]
    public class IntroduceMethodAttribute : IntroduceMethodTemplateAttribute, IAdviceAttribute<IIntroductionAdvice>
    {
        public IntroductionScope? Scope { get; set; }

        public string? Name { get; set; }

        public bool IsStatic { get; set; }

        public bool IsVirtual { get; set; }

        public Visibility? Visibility { get; set; }
    }
}
