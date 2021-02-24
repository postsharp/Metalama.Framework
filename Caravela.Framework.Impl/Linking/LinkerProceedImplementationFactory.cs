using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Impl.Transformations;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerProceedImplementationFactory : ProceedImplementationFactory
    {
        public override IProceedImpl Get( AspectPartId aspectPartId, IMethod overriddenDeclaration )
        {
            return new LinkerOverrideProceedImpl( aspectPartId, overriddenDeclaration );
        }
    }
}
