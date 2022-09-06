// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.Advised
{
    /// <summary>
    /// Represents a parameter in the method being overridden by the advice. This interface augments <see cref="IParameter"/>
    /// with a <see cref="IExpression.Value"/> property, which allows to get or set the run-time value.
    /// </summary>
    public interface IAdvisedParameter : IParameter, IExpression { }
}