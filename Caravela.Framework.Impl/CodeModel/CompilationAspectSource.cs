using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class CompilationAspectSource : IAspectSource
    {
        private readonly CompilationModel _compilation;
        private readonly CompileTimeAssemblyLoader _loader;

        public CompilationAspectSource( CompilationModel compilation, CompileTimeAssemblyLoader loader )
        {
            this._compilation = compilation;
            this._loader = loader;
        }

        public IEnumerable<INamedType> AspectTypes
        {
            get
            {
                var aspectType = this._compilation.Factory.GetTypeByReflectionType( typeof(IAspect) );
                return this._compilation.GetAllAttributeTypes().Where( t => t.Is(aspectType) && t.TypeKind == TypeKind.Class );
            }
        }

        public IEnumerable<AspectInstance> GetAspectInstances( INamedType aspectType ) =>
            this._compilation.GetAllAttributesOfType( aspectType ).Select( attribute =>
            {
                var aspect = (IAspect) this._loader.CreateAttributeInstance( attribute );
                return new AspectInstance( aspect, attribute.ContainingElement!, attribute.Type );
            } );
    }
}