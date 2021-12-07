// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.LinqPad
{
    /// <summary>
    /// Represents a property in an <see cref="FacadeType"/>.
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Type"></param>
    /// <param name="GetFunc"></param>
    /// <param name="IsLazy"></param>
    internal record FacadeProperty( string Name, Type Type, Func<object, object?> GetFunc, bool IsLazy = false );
}