using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Advices
{
    /// <summary>
    /// Interface to be implemented by all custom attributes representing an advice.
    /// </summary>
    public interface IAdviceAttribute
    {
    }

    /// <summary>
    /// Interface to be implemented by all custom attributes representing an advice.
    /// </summary>
    public interface IAdviceAttribute<T> : IAdviceAttribute
        where T : IAdvice
    {
    }
}
