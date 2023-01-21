// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Collections;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Code.Advised
{
    /// <summary>
    /// Represents the list of parameters of the method being overridden by the advice.
    /// The  <see cref="IAdvisedParameter"/> interface augments <see cref="IParameter"/>
    /// with a <see cref="IExpression.Value"/> property, which allows to get or set the run-time value.
    /// </summary>
    public interface IAdvisedParameterList : IReadOnlyList<IAdvisedParameter>
    {
        IParameterList AsParameterList();

        IAdvisedParameter this[ string name ] { get; }

        IEnumerable<IAdvisedParameter> OfType( IType type );

        IEnumerable<IAdvisedParameter> OfType( Type type );

        IAdvisedParameterValueList Values { get; }

        new int Count { get; }
    }
}