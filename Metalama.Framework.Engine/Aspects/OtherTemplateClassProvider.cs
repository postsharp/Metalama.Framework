// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Services;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Aspects;

internal class OtherTemplateClassProvider : IProjectService
{
    private readonly ImmutableDictionary<string, OtherTemplateClass> _otherTemplateClasses;

    public OtherTemplateClassProvider( ImmutableDictionary<string, OtherTemplateClass> otherTemplateClasses )
    {
        this._otherTemplateClasses = otherTemplateClasses;
    }

    public TemplateClass Get( object templateProvider )
    {
        var type = templateProvider as Type ?? templateProvider.GetType();

        return this._otherTemplateClasses[type.FullName.AssertNotNull()];
    }
}