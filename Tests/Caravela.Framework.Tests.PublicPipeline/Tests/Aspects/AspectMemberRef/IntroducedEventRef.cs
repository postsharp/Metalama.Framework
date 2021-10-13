using System.ComponentModel;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.IntegrationTests.Aspects.AspectMemberRef.IntroducedEventRef
{
    public class RetryAttribute : TypeAspect
    {
        [Introduce]
        private void IntroducedMethod1( string name )
        {
            MyEvent?.Invoke( meta.This, new PropertyChangedEventArgs( name ) );
            MyEvent( meta.This, new PropertyChangedEventArgs( name ) );
        }

        [Introduce]
        private event PropertyChangedEventHandler? MyEvent;
    }

    // <target>
    [Retry]
    internal class Program { }
}