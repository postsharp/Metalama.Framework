// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    /// <summary>
    /// A base interface for <see cref="IProperty"/> and <see cref="IEvent"/>. Exposes <see cref="GetAccessor"/>.
    /// </summary>
    public interface IMemberWithAccessors : IMember
    {
        /// <summary>
        /// Gets the accessor for a given <see cref="MethodKind"/>, or <c>null</c> if the member does not define
        /// an accessor of this kind.
        /// </summary>
        /// <param name="methodKind"><see cref="MethodKind.PropertyGet"/>, <see cref="MethodKind.PropertySet"/>,
        /// <see cref="MethodKind.EventAdd"/>, <see cref="MethodKind.EventRemove"/> or <see cref="MethodKind.EventRaise"/>.</param>
        /// <returns></returns>
        IMethod? GetAccessor( MethodKind methodKind );
    }
}