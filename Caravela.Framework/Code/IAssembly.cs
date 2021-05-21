// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents an assembly (typically a reference assembly).
    /// </summary>
    public interface IAssembly : IDeclaration
    {
        string? Name { get; }
    }
}