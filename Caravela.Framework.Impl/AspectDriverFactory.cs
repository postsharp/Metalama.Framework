using System;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;
using Caravela.Reactive;

namespace Caravela.Framework.Impl
{
    class AspectDriverFactory
    {
        private readonly ICompilation _compilation;
        private readonly CompileTimeAssemblyLoader _loader;
        private readonly IReactiveGroupBy<IType, INamedType>? _weaverTypes;

        public AspectDriverFactory( ICompilation compilation, CompileTimeAssemblyLoader loader )
        {
            this._compilation = compilation;
            this._loader = loader;

            var aspectWeaverAttributeType = compilation.GetTypeByReflectionName( typeof( AspectWeaverAttribute ).FullName );

            if ( aspectWeaverAttributeType == null )
            {
                // this should mean that the Sdk assembly was not referenced, which means there can't be any weavers
                this._weaverTypes = null;
            }
            else
            {
                // TODO: nested types?
                this._weaverTypes =
                    from weaverType in compilation.DeclaredAndReferencedTypes
                    from attribute in weaverType.Attributes
                    where attribute.Type.Is( aspectWeaverAttributeType )
                    group weaverType by (IType) attribute.ConstructorArguments.Single()!;
            }
        }

        public IAspectDriver GetAspectDriver( INamedType type )
        {
            var weavers = this._weaverTypes?[type].GetValue().ToList();

            if ( weavers?.Count > 1 )
                throw new CaravelaException( GeneralDiagnosticDescriptors.AspectHasMoreThanOneWeaver, type, string.Join( ", ", weavers ) );

            if ( weavers?.Count == 1 )
                // TODO: this needs to be the same instance for equivalent type, to make reactive grouping work
                return (IAspectDriver) this._loader.CreateInstance( weavers.Single().GetSymbol() );

            return new AspectDriver( type, this._compilation );
        }
    }
}