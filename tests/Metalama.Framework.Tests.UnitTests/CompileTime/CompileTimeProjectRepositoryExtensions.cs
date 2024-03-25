// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Services;

namespace Metalama.Framework.Tests.UnitTests.CompileTime;

internal static class CompileTimeProjectRepositoryExtensions
{
    // Should be used only in tests.
    public static UserCodeAttributeDeserializer CreateAttributeDeserializer( this CompileTimeProjectRepository repo, in ProjectServiceProvider serviceProvider )
        => new( serviceProvider.WithService( new ProjectSpecificCompileTimeTypeResolver( serviceProvider.WithService( repo ) ) ) );
}