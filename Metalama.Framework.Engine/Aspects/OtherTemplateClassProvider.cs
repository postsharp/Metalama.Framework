﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Services;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Aspects;

internal class OtherTemplateClassProvider : IProjectService
{
    private readonly ImmutableDictionary<string, OtherTemplateClass> _otherTemplateClasses;

    public OtherTemplateClassProvider( ImmutableDictionary<string, OtherTemplateClass> otherTemplateClasses )
    {
        this._otherTemplateClasses = otherTemplateClasses;
    }

    public TemplateClass Get( TemplateProvider templateProvider )
    {
        var type = templateProvider.Type;

        if ( type.IsGenericType )
        {
            type = type.GetGenericTypeDefinition();
        }

        return this._otherTemplateClasses[type.FullName.AssertNotNull()];
    }
}