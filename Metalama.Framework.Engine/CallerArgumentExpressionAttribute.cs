// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace System.Runtime.CompilerServices;

#if DEBUG && NETSTANDARD2_0

[AttributeUsage( AttributeTargets.Parameter, AllowMultiple = false, Inherited = false )]
public sealed class CallerArgumentExpressionAttribute : Attribute
{
    public CallerArgumentExpressionAttribute( string parameterName )
    {
        this.ParameterName = parameterName;
    }

    public string ParameterName { get; }
}

#endif