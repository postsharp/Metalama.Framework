using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.Aspects.AspectMemberRef.IntroducedPropertyRef
{

    public class RetryAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce]
        void IntroducedMethod1( string name ) 
        {
            this.IntroducedProperty = name;
        }
        
        [Introduce]
        string IntroducedProperty { get { return meta.Target.Property.DeclaringType.Name; } set {} }
        
    }
    
    // <target>
    [Retry]
    class Program
    {

    }
}