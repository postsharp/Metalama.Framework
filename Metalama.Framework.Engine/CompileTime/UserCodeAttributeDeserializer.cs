// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;

namespace Metalama.Framework.Engine.CompileTime;

internal sealed class UserCodeAttributeDeserializer : AttributeDeserializer
{
    private UserCodeAttributeDeserializer( in ProjectServiceProvider serviceProvider, CompileTimeTypeResolver resolver ) : base(
        serviceProvider,
        resolver ) { }

    public class Provider : CompilationServiceProvider<AttributeDeserializer>
    {
        public Provider( in ProjectServiceProvider serviceProvider ) : base( in serviceProvider ) { }

        protected override AttributeDeserializer Create( CompilationContext compilationContext )
            => new UserCodeAttributeDeserializer(
                this.ServiceProvider,
                this.ServiceProvider.GetRequiredService<ProjectSpecificCompileTimeTypeResolver.Provider>().Get( compilationContext ) );
    }
}