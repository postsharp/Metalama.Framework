using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl
{
    internal class AttributeAspectSource 
    {
        private readonly ICompilation _compilation;
        private readonly CompileTimeAssemblyLoader _loader;

        public AttributeAspectSource( ICompilation compilation, CompileTimeAssemblyLoader loader )
        {
            this._compilation = compilation;
            this._loader = loader;
        }

        
    }
}
