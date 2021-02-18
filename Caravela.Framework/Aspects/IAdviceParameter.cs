using Caravela.Framework.Code;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Represents a parameter in the method being overridden by the advice. This interface augments <see cref="IParameter"/>
    /// with a <see cref="IHasRuntimeValue.Value"/> property, which allows to get or set the run-time value.
    /// </summary>
    public interface IAdviceParameter : IParameter, IHasRuntimeValue
    {
    }
}