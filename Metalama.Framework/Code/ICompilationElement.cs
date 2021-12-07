﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Exposes a <see cref="Compilation"/> property.
    /// </summary>
    [InternalImplement]
    [CompileTimeOnly]
    public interface ICompilationElement
    {
        /// <summary>
        /// Gets the <see cref="ICompilation"/> to which this type belongs (which does not mean that the type is declared
        /// by the main project of the compilation).
        /// </summary>
        ICompilation Compilation { get; }
    }
}