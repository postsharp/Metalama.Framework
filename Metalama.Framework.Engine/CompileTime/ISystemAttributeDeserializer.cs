// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;

namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// An <see cref="AttributeDeserializer"/> that can deserialize only system attributes like the ones defined in <c>Metalama.Framework</c> but
/// not the ones defined in the user code.
/// </summary>
internal interface ISystemAttributeDeserializer : IAttributeDeserializer, IProjectService { }

internal sealed class SystemAttributeDeserializer : AttributeDeserializer, ISystemAttributeDeserializer
{
    public SystemAttributeDeserializer( in ProjectServiceProvider serviceProvider ) : base(
        serviceProvider,
        serviceProvider.GetRequiredService<SystemTypeResolver>() ) { }
}