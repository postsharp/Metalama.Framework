// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Aspects;

/// <summary>
/// Custom attribute that, when applied to an aspect class, means that this aspect classes uses several
/// layers, and defines the name and the order of execution of these layers.
/// </summary>
[AttributeUsage( AttributeTargets.Class )]
public sealed class LayersAttribute : Attribute
{
    public string[] Layers { get; }

    public LayersAttribute( params string[] layers )
    {
        this.Layers = layers;
    }
}