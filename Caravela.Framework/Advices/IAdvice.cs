﻿namespace Caravela.Framework.Advices
{
    public interface IAdvice { }

    public interface IAdvice<T> : IAdvice where T : ICodeElement
    {
        T TargetDeclaration { get; }
    }
}
