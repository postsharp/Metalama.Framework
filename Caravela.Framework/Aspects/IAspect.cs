using Caravela.Framework.Advices;

namespace Caravela.Framework.Aspects
{
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

        // TODO: should there be an AdviceFactory instead, as per spec?
        void AddAdvice<T>( IAdvice<T> advice ) where T : ICodeElement;
    }

    public interface IAspectBuilder<out T> : IAspectBuilder
        where T : ICodeElement
    {
        new T TargetDeclaration { get; }
    }
}