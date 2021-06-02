// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Aspects.AdvisedCode
{
    /// <summary>
    /// Represents the list of parameters of the method being overridden by the advice.
    /// The  <see cref="IAdviceParameter"/> interface augments <see cref="IParameter"/>
    /// with a <see cref="IHasRuntimeValue.Value"/> property, which allows to get or set the run-time value.
    /// </summary>
    public interface IAdviceParameterList : IReadOnlyList<IAdviceParameter>
    {
        IAdviceParameter this[ string name ] { get; }

        IEnumerable<IAdviceParameter> OfType( IType type );

        IEnumerable<IAdviceParameter> OfType( Type type );

        IAdviceParameterValueList Values { get; }
    }
}