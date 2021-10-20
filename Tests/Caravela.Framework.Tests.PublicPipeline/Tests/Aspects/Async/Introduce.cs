using System.Threading.Tasks;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Async.Introduce
{
    internal class Aspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advices.IntroduceMethod( builder.Target, nameof(ProgrammaticallyMethodAsync) );
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