using Caravela.Framework.Aspects;

namespace Caravela.Framework
{
    public class AspectInstance
    {
        public IAspect Aspect { get; }
        public ICodeElement CodeElement { get; }
        internal AspectType AspectType { get; }

        internal AspectInstance(IAspect aspect, ICodeElement codeElement, AspectType aspectType)
        {
            Aspect = aspect;
            CodeElement = codeElement;
            AspectType = aspectType;
        }
    }
}
