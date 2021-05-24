// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a constructed type, for instance an array, a generic type instance, a pointer.
    /// A class, struct, enum or delegate are represented as an <see cref="INamedType"/>, which
    /// derive from <see cref="IType"/>.
    /// </summary>
    [CompileTimeOnly]
    public interface IType : ICompilationElement, IDisplayable
    {
        /// <summary>
        /// Gets the kind of type.
        /// </summary>
        TypeKind TypeKind { get; }

        /// <summary>
        /// Gets a reflection <see cref="Type"/> that represents the current type at run time.
        /// </summary>
        /// <returns>A <see cref="Type"/> that can be used only in run-time code.</returns>
        [return: RunTimeOnly]
        Type ToType();

        [return: RunTimeOnly]
        // Casts a value to this type (and returns the syntax).
        dynamic Cast( dynamic value );
    }
}