// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Sdk
{
    /// <summary>
    /// Extends the user-level <see cref="INamedType"/> interface with a <see cref="ISdkType.TypeSymbol"/> exposing the Roslyn <see cref="ITypeSymbol"/>. 
    /// </summary>
    public interface ISdkNamedType : INamedType, ISdkType { }
}