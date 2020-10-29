using Caravela.Framework.Advices;
using Caravela.Framework.Project;

namespace Caravela.Framework.Aspects
{
    [CompileTime]
    public interface IAspect
    {
    }

    public interface IAspect<in T> : IAspect
        where T : ICodeElement
    {
        void Initialize( IAspectBuilder<T> aspectBuilder );
    }

    public interface IAspectBuilder
    {
        ICodeElement TargetDeclaration { get; }

        // TODO: there should be an AdviceFactory instead, as per spec
        void AddAdvice<T>( IAdvice<T> advice ) where T : ICodeElement;
    }

    public interface IAspectBuilder<out T> : IAspectBuilder
        where T : ICodeElement
    {
        new T TargetDeclaration { get; }
    }
}