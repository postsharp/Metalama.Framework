using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.DeclarativeRunTimeOnly
{
    public class IntroductionAttribute : TypeAspect
    {
        // TODO: Indexers.    

        [Introduce]
        public RunTimeOnlyClass? IntroducedProperty_Accessors
        {
            get
            {
                Console.WriteLine( "Get" );

                return null;
            }

            set
            {
                Console.WriteLine( value );
            }
        }
    }

    public class RunTimeOnlyClass { }

    // <target>
    [Introduction]
    internal class TargetClass { }
}