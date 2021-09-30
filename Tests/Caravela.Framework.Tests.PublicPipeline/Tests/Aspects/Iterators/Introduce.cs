using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Iterators.Introduce
{
    class Aspect : Attribute, IAspect<INamedType>
    {
        public void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advices.IntroduceMethod( builder.Target, nameof(ProgrammaticallyMethodAsync) );
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
    class TargetCode
    {
    }
}