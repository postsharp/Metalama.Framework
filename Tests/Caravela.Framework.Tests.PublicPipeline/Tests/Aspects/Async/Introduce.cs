using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Async.Introduce
{
    class Aspect : Attribute, IAspect<INamedType>
    {
        public void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.AdviceFactory.IntroduceMethod( builder.Target, nameof(ProgrammaticallyMethodAsync) );
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
    class TargetCode
    {
    }
}