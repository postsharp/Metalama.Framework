// unset

using Caravela.Framework.Advices;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class MemberBuilder : CodeElementBuilder, IMemberBuilder
    {
        public bool IsSealed { get; set; }

        public INamedType DeclaringType { get; }

        public Visibility Visibility { get; set; }
        public string Name { get; set; }

        public bool IsStatic { get; set; }

        public bool IsVirtual { get; set; }
        
        public sealed override ICodeElement? ContainingElement => this.DeclaringType;

        public MemberBuilder( IAdvice advice, INamedType declaringType ) : base()
        {
            this.DeclaringType = declaringType;
        }
    }
}