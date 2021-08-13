// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.Framework.Code.Advised
{
    /// <summary>
    /// Represents the list of parameters of the method being overridden by the advice.
    /// The  <see cref="IAdvisedParameter"/> interface augments <see cref="IParameter"/>
    /// with a <see cref="IExpression.Value"/> property, which allows to get or set the run-time value.
    /// </summary>
    public interface IAdvisedParameterList : IReadOnlyList<IAdvisedParameter>
    {
        IAdvisedParameter this[ string name ] { get; }

        IEnumerable<IAdvisedParameter> OfType( IType type );

        IEnumerable<IAdvisedParameter> OfType( Type type );

        IAdviseParameterValueList Values { get; }
    }
}