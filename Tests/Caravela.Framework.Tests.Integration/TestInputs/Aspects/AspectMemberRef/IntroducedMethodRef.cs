using System;
using System.Text;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.Aspects.AspectMemberRef.IntroducedMethodRef
{

    public class RetryAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce]
        void IntroducedMethod1( string name ) 
        {
            this.IntroducedMethod2( meta.Method.Name);
        }
        
        [Introduce]
        void IntroducedMethod2( string name ) 
        {
            this.IntroducedMethod1( meta.Method.Name);
        }
        
    }
    
    [TestOutput]
    [Retry]
    class Program
    {

    }
}