// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Services;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Aspects;

internal sealed class OtherTemplateClassProvider : IProjectService
{
    private readonly ImmutableDictionary<string, OtherTemplateClass> _otherTemplateClasses;

    public OtherTemplateClassProvider( ImmutableDictionary<string, OtherTemplateClass> otherTemplateClasses )
    {
        this._otherTemplateClasses = otherTemplateClasses;
    }

    public TemplateClass Get( TemplateProvider templateProvider ) => this._otherTemplateClasses[templateProvider.Type.FullName.AssertNotNull()];
}