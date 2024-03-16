// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct, Inherited = false )]
public sealed class InterpolatedStringHandlerAttribute : Attribute { }
#endif