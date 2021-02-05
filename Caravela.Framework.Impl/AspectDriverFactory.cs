using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl
{
    internal class AspectDriverFactory
    {
        private readonly ICompilation _compilation;
        private readonly ILookup<string, IAspectWeaver> _weaverTypes;

        public AspectDriverFactory( ICompilation compilation, ImmutableArray<object> plugins )
        {
            this._compilation = compilation;

            this._weaverTypes = plugins.OfType<IAspectWeaver>()
                .ToLookup( weaver => weaver.GetType().GetCustomAttribute<AspectWeaverAttribute>().AspectType.FullName );
        }

        public IAspectDriver GetAspectDriver( INamedType type )
        {
            var weavers = this._weaverTypes[type.FullName].ToList();

            if ( weavers.Count > 1 )
            {
                throw new CaravelaException( GeneralDiagnosticDescriptors.AspectHasMoreThanOneWeaver, type, string.Join( ", ", weavers ) );
            }

            if ( weavers.Count == 1 )
            {
                return weavers.Single();
            }

            return new AspectDriver( type, this._compilation );
        }
    }
}