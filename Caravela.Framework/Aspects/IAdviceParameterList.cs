using System.Collections.Generic;
using Caravela.Framework.Code;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Represents the list of parameters of the method being overridden by the advice.
    /// The  <see cref="IAdviceParameter"/> interface augments <see cref="IParameter"/>
    /// with a <see cref="IExposeRuntimeValue.Value"/> property, which allows to get or set the run-time value.
    /// </summary>
    public interface IAdviceParameterList : IReadOnlyList<IAdviceParameter>
    {
        // IAdviceParameter this[string name] { get; }

        // IEnumerable<IAdviceParameter> this[Type type] { get; }
    }
}