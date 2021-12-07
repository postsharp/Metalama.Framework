// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code.Collections
{
    /// <summary>
    /// Read-only list of <see cref="IField"/> or <see cref="IProperty"/>.
    /// </summary>
    /// <remarks>
    ///  <para>The order of items in this list is undetermined and may change between versions.</para>
    /// </remarks>
    public interface IFieldOrPropertyList : IMemberList<IFieldOrProperty> { }
}