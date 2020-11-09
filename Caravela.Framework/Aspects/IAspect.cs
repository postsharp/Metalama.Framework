using Caravela.Framework.Code;
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

        IAdviceFactory AdviceFactory { get; }
    }

    public interface IAspectBuilder<out T> : IAspectBuilder
        where T : ICodeElement
    {
        new T TargetDeclaration { get; }
    }
}