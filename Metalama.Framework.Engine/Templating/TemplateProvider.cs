// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Engine.Templating;

public readonly record struct TemplateProvider( object? Value )
{
    public Type ValueType
    {
        get
        {
            Invariant.AssertNotNull( this.Value );

            return this.Value as Type ?? this.Value!.GetType();
        }
    }
}