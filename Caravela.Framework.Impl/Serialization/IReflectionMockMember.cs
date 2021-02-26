// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal interface IReflectionMockMember : IReflectionMockCodeElement
    {

        ITypeSymbol? DeclaringTypeSymbol { get; }
    }
}