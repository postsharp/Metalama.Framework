// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if DEBUG && NETSTANDARD2_0

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

[AttributeUsage( AttributeTargets.Parameter )]
public sealed class CallerArgumentExpressionAttribute : Attribute
{
    public CallerArgumentExpressionAttribute( string parameterName )
    {
        this.ParameterName = parameterName;
    }

    public string ParameterName { get; }
}

#endif