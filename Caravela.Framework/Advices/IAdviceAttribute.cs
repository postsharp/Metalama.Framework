// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Advices
{
    /// <summary>
    /// Interface to be implemented by all custom attributes representing an advice.
    /// </summary>
    public interface IAdviceAttribute
    {
    }

    /// <summary>
    /// Interface to be implemented by all custom attributes representing an advice.
    /// </summary>
    // ReSharper disable once UnusedTypeParameter
    public interface IAdviceAttribute<T> : IAdviceAttribute
        where T : IAdvice
    {
    }
}
