// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Advices
{
    /// <summary>
    /// Interface to be implemented by all custom attributes representing an advice.
    /// </summary>
    public interface IAdviceAttribute { }

    // ReSharper disable once UnusedTypeParameter

    /// <summary>
    /// Interface to be implemented by all custom attributes representing an advice.
    /// </summary>
    public interface IAdviceAttribute<T> : IAdviceAttribute
        where T : IAdvice
    { }
}