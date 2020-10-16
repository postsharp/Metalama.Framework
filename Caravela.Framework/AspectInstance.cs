using Caravela.Framework.Aspects;

namespace Caravela.Framework
{
    public class AspectInstance
    {
        public IAspect Aspect { get; }
        public ICodeElement CodeElement { get; }
        internal INamedType AspectType { get; }

        internal AspectInstance(IAspect aspect, ICodeElement codeElement, INamedType aspectType)
        {
            Aspect = aspect;
            CodeElement = codeElement;
            AspectType = aspectType;
        }
    }
}
