using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Aspects
{
    public interface IAspect
    {
        
    }

    public interface IAspect<in T> : IAspect 
        where T : ICodeElement
    {
        void Initialize(IAspectBuilder<T> aspectBuilder);
    }


    public interface IAspectBuilder 
    {
        ICodeElement TargetDeclaration { get; }
    }

    public interface IAspectBuilder<out T> : IAspectBuilder 
        where T : ICodeElement
    {
        new T TargetDeclaration { get; }
    }
}
