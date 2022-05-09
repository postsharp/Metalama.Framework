// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code.Advised
{
    /// <summary>
    /// Represents the event being overwritten or introduced. This interface extends <see cref="IEvent"/> but overrides the <see cref="AddMethod"/>,
    /// <see cref="RemoveMethod"/> and <see cref="RaiseMethod"/> members to expose their <see cref="IAdvisedMethod.Invoke"/> method.
    /// </summary>
    public interface IAdvisedEvent : IEvent
    {
        /// <summary>
        /// Gets the method implementing the <c>add</c> semantic. In case of field-like events, this property returns
        /// an object that does not map to source code but allows to add aspects and advice as with a normal method.
        /// </summary>
        new IAdvisedMethod AddMethod { get; }

        /// <summary>
        /// Gets the method implementing the <c>remove</c> semantic. In case of field-like events, this property returns
        /// an object that does not map to source code but allows to add aspects and advice as with a normal method.
        /// </summary>
        new IAdvisedMethod RemoveMethod { get; }

        /// <summary>
        /// Gets an object that represents the <c>raise</c> semantic and allows to add aspects and advice
        /// as with a normal method.
        /// </summary>
        new IAdvisedMethod? RaiseMethod { get; }
    }
}