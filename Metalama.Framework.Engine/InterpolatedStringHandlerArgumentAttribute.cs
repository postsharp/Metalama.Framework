// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if NETSTANDARD2_0

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

[AttributeUsage( AttributeTargets.Parameter )]
public sealed class InterpolatedStringHandlerArgumentAttribute : Attribute
{
    public InterpolatedStringHandlerArgumentAttribute( string argument )
    {
        this.Arguments = new[] { argument };
    }

    public InterpolatedStringHandlerArgumentAttribute( params string[] arguments )
    {
        this.Arguments = arguments;
    }

    public string[] Arguments { get; }
}
#endif