// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Aspects.AdvisedCode
{
    public interface IAdviceEvent : IEvent
    {
        /// <summary>
        /// Gets the method implementing the <c>add</c> semantic. In case of field-like events, this property returns
        /// an object that does not map to source code but allows to add aspects and advices as with a normal method.
        /// </summary>
        new IAdviceMethod Adder { get; }

        /// <summary>
        /// Gets the method implementing the <c>remove</c> semantic. In case of field-like events, this property returns
        /// an object that does not map to source code but allows to add aspects and advices as with a normal method.
        /// </summary>
        new IAdviceMethod Remover { get; }

        /// <summary>
        /// Gets an object that represents the <c>raise</c> semantic and allows to add aspects and advices
        /// as with a normal method.
        /// </summary>
        new IAdviceMethod? Raiser { get; }
    }
}