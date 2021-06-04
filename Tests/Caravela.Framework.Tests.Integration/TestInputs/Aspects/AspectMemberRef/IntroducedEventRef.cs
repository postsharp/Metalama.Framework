using System;
using System.Text;
using System.Linq;
using System.ComponentModel;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.Aspects.AspectMemberRef.IntroducedEventRef
{

    public class RetryAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce]
        void IntroducedMethod1( string name ) 
        {
            this.MyEvent.Invoke( meta.This, new PropertyChangedEventArgs( name ) );
            this.MyEvent( meta.This, new PropertyChangedEventArgs( name ) );
        }
        
        [Introduce]
        event PropertyChangedEventHandler? MyEvent;
        
    }
    
    [TestOutput]
    [Retry]
    class Program
    {

    }
}