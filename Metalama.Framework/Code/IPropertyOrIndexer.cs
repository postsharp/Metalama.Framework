// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Reflection;

namespace Metalama.Framework.Code;

public interface IPropertyOrIndexer : IHasWriteability, IMemberWithAccessors
{
    /// <summary>
    /// Gets the <c>in</c>, <c>ref</c>, <c>ref readonly</c> property type modifier.
    /// </summary>
    RefKind RefKind { get; }

    /// <summary>
    /// Gets a <see cref="PropertyInfo"/> that represents the current property at run time.
    /// </summary>
    /// <returns>A <see cref="PropertyInfo"/> that can be used only in run-time code.</returns>
    PropertyInfo ToPropertyInfo();
}