using Metalama.Framework.Aspects;

namespace Metalama.Framework.Introspection;

public interface IIntrospectionAspectPredecessorInternal : IIntrospectionAspectPredecessor
{
    void AddSuccessor( IAspectInstance aspectInstance );
}