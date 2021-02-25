using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.MetaModel;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class ProceedImplementationFactory
    {
        public abstract IProceedImpl Get( AspectPartId aspectPartId, IMethod overriddenDeclaration );
    }
}
