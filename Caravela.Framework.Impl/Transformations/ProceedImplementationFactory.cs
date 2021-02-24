using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.MetaModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class ProceedImplementationFactory
    {
        public abstract IProceedImpl Get( AspectPartId aspectPartId, IMethod overriddenDeclaration );
    }
}
