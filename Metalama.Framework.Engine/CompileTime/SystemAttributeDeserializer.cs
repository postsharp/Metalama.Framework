// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;

namespace Metalama.Framework.Engine.CompileTime;

internal sealed class SystemAttributeDeserializer : AttributeDeserializer
{
    public SystemAttributeDeserializer( in ProjectServiceProvider serviceProvider, SystemTypeResolver systemTypeResolver ) : base(
        serviceProvider,
        systemTypeResolver ) { }

    public new class Provider : CompilationServiceProvider<AttributeDeserializer>
    {
        public Provider( in ProjectServiceProvider serviceProvider ) : base( in serviceProvider ) { }

        protected override AttributeDeserializer Create( CompilationContext compilationContext )
        {
            var resolver = (SystemTypeResolver) this.ServiceProvider.GetRequiredService<SystemTypeResolver.Provider>().Get( compilationContext );

            return new SystemAttributeDeserializer( this.ServiceProvider, resolver );
        }
    }
}