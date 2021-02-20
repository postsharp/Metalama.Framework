using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CompileTime;

namespace Caravela.Framework.Impl
{
    internal class AspectTypeFactory
    {
        private readonly AspectDriverFactory _aspectDriverFactory;
        private readonly CompileTimeAssemblyLoader _compileTimeAssemblyLoader;

        private readonly Dictionary<INamedType, AspectType> _aspectTypes = new();

        public AspectTypeFactory( AspectDriverFactory aspectDriverFactory, CompileTimeAssemblyLoader compileTimeAssemblyLoader )
        {
            this._aspectDriverFactory = aspectDriverFactory;
            this._compileTimeAssemblyLoader = compileTimeAssemblyLoader;
        }

        public AspectType GetAspectType( INamedType attributeType )
        {
            if ( !this._aspectTypes.TryGetValue( attributeType, out var aspectType ) )
            {
                var aspectDriver = this._aspectDriverFactory.GetAspectDriver( attributeType );

                // TODO: create AspectLayers properly
                aspectType = new( attributeType, aspectDriver, this._compileTimeAssemblyLoader );

                this._aspectTypes.Add( attributeType, aspectType );
            }

            return aspectType;
        }
    }
}
