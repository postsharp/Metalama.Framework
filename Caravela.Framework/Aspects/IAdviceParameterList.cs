using Caravela.Framework.Code;
using System.Collections.Generic;

namespace Caravela.Framework.Aspects
{
    public interface IAdviceParameterList : IReadOnlyList<IAdviceParameter>
    {
        //IAdviceParameter this[string name] { get; }

        //IEnumerable<IAdviceParameter> this[Type type] { get; }
    }

    public interface IAdviceParameter : IParameter, IExposeRuntimeValue
    {
    }
}