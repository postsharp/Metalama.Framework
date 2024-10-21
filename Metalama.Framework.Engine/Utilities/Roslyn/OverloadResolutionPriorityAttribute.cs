// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace System.Runtime.CompilerServices;

#if !NET9_0_OR_GREATER

[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = false, Inherited = false )]
public sealed class OverloadResolutionPriorityAttribute( int priority ) : Attribute
{
    public int Priority => priority;
}

#endif