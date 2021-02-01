using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Advices
{
    public interface IAdviceAttribute
    {
    }

    public interface IAdviceAttribute<T> : IAdviceAttribute
        where T : IAdvice
    {
    }
}
