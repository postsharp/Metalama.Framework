// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code.Collections
{
    /// <summary>
    /// Read-only list of <see cref="IEvent"/>.
    /// </summary>
    /// <remarks>
    ///  <para>The order of items in this list is undetermined and may change between versions.</para>
    /// </remarks>
    public interface IEventList : IMemberList<IEvent>
    {
        /// <summary>
        /// Gets an event that exactly matches the specified signature.
        /// </summary>
        /// <param name="signatureTemplate">Event signature of which to should be considered.</param>
        /// <param name="matchIsStatic">Value indicating whether the staticity of the event should be matched.</param>
        /// <returns>Enumeration of events matching specified constraints. If <paramref name="declaredOnly" /> is set to <c>false</c>, only the top-most visible event of the same signature is included.</returns>
        /// <returns>A <see cref="IEvent"/> that matches the given signature. If <paramref name="declaredOnly" /> is set to <c>false</c>, the top-most visible event is shown.</returns>
        IEvent? OfExactSignature(
            IEvent signatureTemplate,
            bool matchIsStatic = true,
            bool declaredOnly = true );
    }
}