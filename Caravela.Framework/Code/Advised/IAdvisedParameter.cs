// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code.Advised
{
    /// <summary>
    /// Represents a parameter in the method being overridden by the advice. This interface augments <see cref="IParameter"/>
    /// with a <see cref="IHasRuntimeValue.Value"/> property, which allows to get or set the run-time value.
    /// </summary>
    public interface IAdvisedParameter : IParameter, IHasRuntimeValue { }
}