using System.Collections.Generic;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Async.AsyncIterators.Introduce
{
    internal class Aspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.IntroduceMethod( builder.Target, nameof(ProgrammaticallyMethodAsync) );
        }

        [Introduce]
        public async IAsyncEnumerable<int> DeclarativelyMethodAsync()
        {
            await Task.Yield();

            yield return 1;
        }

        [Template]
        public async IAsyncEnumerable<int> ProgrammaticallyMethodAsync()
        {
            await Task.Yield();

            yield return 1;
        }
    }

    // <target>
    [Aspect]
    internal class TargetCode { }
}