// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;

namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// An <see cref="AttributeDeserializer"/> that can deserialize custom attributes whose type are defined in the user project.
/// </summary>
internal interface IUserCodeAttributeDeserializer : IAttributeDeserializer, IProjectService;

internal sealed class UserCodeAttributeDeserializer : AttributeDeserializer, IUserCodeAttributeDeserializer
{
    public UserCodeAttributeDeserializer( ProjectServiceProvider serviceProvider ) : base(
        serviceProvider,
        serviceProvider.GetRequiredService<ProjectSpecificCompileTimeTypeResolver>() ) { }
}