// @DesignTime

using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.DesignTime
{
    public class IntroductionAttribute : TypeAspect
    {
        // TODO: Indexers.    

        //[IntroduceProperty]
        //public int IntroducedProperty_Auto { get; set; }

        // TODO: Introduction of auto properties.
        //[IntroduceProperty]
        //public static int IntroducedProperty_Auto_Static { get; }

        [Introduce]
        public int IntroducedProperty_Accessors
        {
            get
            {
                Console.WriteLine( "Get" );

                return 42;
            }

            set
            {
                Console.WriteLine( value );
            }
        }
    }

    // <target>
    [Introduction]
    internal partial class TargetClass { }
}