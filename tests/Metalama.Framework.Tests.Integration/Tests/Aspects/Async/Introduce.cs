using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Async.Introduce
{
    internal class Aspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advise.IntroduceMethod( builder.Target, nameof(ProgrammaticallyMethodAsync) );
        }

        [Introduce]
        public async Task DeclarativelyMethodAsync()
        {
            await Task.Yield();
        }

        [Template]
        public async Task ProgrammaticallyMethodAsync()
        {
            await Task.Yield();
        }
    }

    // <target>
    [Aspect]
    internal class TargetCode { }
}