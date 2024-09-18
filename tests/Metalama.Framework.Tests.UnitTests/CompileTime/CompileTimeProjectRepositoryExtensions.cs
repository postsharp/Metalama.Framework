// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Services;

namespace Metalama.Framework.Tests.UnitTests.CompileTime;

internal static class CompileTimeProjectRepositoryExtensions
{
    public static AttributeDeserializer CreateAttributeDeserializer(
        this CompileTimeProjectRepository repo,
        in ProjectServiceProvider serviceProvider,
        CompilationContext compilationContext )
    {
        var augmentedServiceProvider =
            serviceProvider.WithService( new ProjectSpecificCompileTimeTypeResolver.Provider( serviceProvider.WithService( repo ) ) );

        return new UserCodeAttributeDeserializer.Provider( augmentedServiceProvider ).Get( compilationContext );
    }
}