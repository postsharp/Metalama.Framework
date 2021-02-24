// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using Caravela.Framework.Code;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Represents the list of parameters of the method being overridden by the advice.
    /// The  <see cref="IAdviceParameter"/> interface augments <see cref="IParameter"/>
    /// with a <see cref="IHasRuntimeValue.Value"/> property, which allows to get or set the run-time value.
    /// </summary>
    public interface IAdviceParameterList : IReadOnlyList<IAdviceParameter>
    {
        IAdviceParameter this[string name] { get; }

        IEnumerable<IAdviceParameter> OfType( IType type );

        IEnumerable<IAdviceParameter> OfType( Type type );

        IAdviceParameterValueList Values { get; }
    }

    public interface IAdviceParameterValueList
    {
        dynamic ToArray();

        dynamic ToValueTuple();
    }
}