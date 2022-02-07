// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Introspection;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Introspection;

internal abstract class BaseIntrospectionAspectClass : IIntrospectionAspectClass
{
    private readonly IAspectClass _aspectClass;

    public string FullName => this._aspectClass.FullName;

    public string ShortName => this._aspectClass.ShortName;

    public string DisplayName => this._aspectClass.DisplayName;

    public string? Description => this._aspectClass.Description;

    public bool IsAbstract => this._aspectClass.IsAbstract;

    public bool IsInherited => this._aspectClass.IsInherited;

    public bool IsAttribute => this._aspectClass.IsAttribute;

    public Type Type => this._aspectClass.Type;

    public BaseIntrospectionAspectClass( IAspectClass aspectClass )
    {
        this._aspectClass = aspectClass;
    }

    public abstract ImmutableArray<IIntrospectionAspectInstance> Instances { get; }
}