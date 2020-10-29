using System.Collections.Immutable;
using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;
using Caravela.Reactive;

namespace Caravela.Framework.Impl
{
    class AttributeAspectSource : AspectSource
    {
        private readonly ICompilation _compilation;
        private readonly CompileTimeAssemblyLoader _loader;

        public AttributeAspectSource( ICompilation compilation, CompileTimeAssemblyLoader loader )
        {
            this._compilation = compilation;
            this._loader = loader;
        }

        public override IReactiveCollection<AspectInstance> GetAspects()
        {
            var results = ImmutableArray.CreateBuilder<AspectInstance>();

            var iAspect = this._compilation.GetTypeByReflectionType(typeof(IAspect))!;

            return from type in this._compilation.DeclaredTypes
                   from attribute in type.Attributes
                   where attribute.Type.Is( iAspect )
                   select new AspectInstance( (IAspect) this._loader.CreateInstance( attribute.Type.GetSymbol() ), type, attribute.Type );
        }
    }
}
