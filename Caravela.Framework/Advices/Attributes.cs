using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Advices
{
    public interface IAdviceAttribute { }

    [AttributeUsage( AttributeTargets.Method, Inherited = true )]
    public class OverrideMethodTemplateAttribute : TemplateAttribute { }

    public class OverrideMethodAttribute : OverrideMethodTemplateAttribute, IAdviceAttribute, IOverrideMethodAdvice
    {
        public IMethod TargetDeclaration => throw new NotSupportedException();
    }

    [AttributeUsage( AttributeTargets.Method, Inherited = true )]
    public class IntroduceMethodTemplateAttribute : TemplateAttribute { }

    [AttributeUsage( AttributeTargets.Method, Inherited = true )]
    public class IntroduceMethodAttribute : IntroduceMethodTemplateAttribute, IAdviceAttribute, IIntroductionAdvice
    {
        public INamedType TargetDeclaration => throw new NotSupportedException();
    }
}
