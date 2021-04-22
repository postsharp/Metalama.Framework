// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Advices
{
    /// <summary>
    /// Represents an advice that overrides the implementation of a method.
    /// </summary>
    public interface IOverrideMethodAdvice : IAdvice<IMethod>
    {
        // TODO: Members (get-only in the spec).
    }
}