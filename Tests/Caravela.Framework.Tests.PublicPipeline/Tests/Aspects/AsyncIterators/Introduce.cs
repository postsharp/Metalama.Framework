using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.AsyncIterators.Introduce
{
    class Aspect : Attribute, IAspect<INamedType>
    {
        public void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.AdviceFactory.IntroduceMethod( builder.Target, nameof(ProgrammaticallyMethodAsync) );
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
    class TargetCode
    {
    }
}