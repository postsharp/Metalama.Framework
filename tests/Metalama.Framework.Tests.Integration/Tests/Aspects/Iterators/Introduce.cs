using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Iterators.Introduce
{
    internal class Aspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advise.IntroduceMethod( builder.Target, nameof(ProgrammaticallyMethodAsync) );
        }

        [Introduce]
        public IEnumerable<int> DeclarativelyMethodAsync()
        {
            yield return 1;
        }

        [Template]
        public IEnumerable<int> ProgrammaticallyMethodAsync()
        {
            yield return 1;
        }
    }

    // <target>
    [Aspect]
    internal class TargetCode { }
}