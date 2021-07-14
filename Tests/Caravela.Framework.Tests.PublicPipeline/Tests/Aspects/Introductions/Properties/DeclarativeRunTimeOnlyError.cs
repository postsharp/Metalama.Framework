using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.DeclarativeRunTimeOnly.Virtual
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
       
        // TODO: Indexers.    

        [Introduce]
        public virtual RunTimeOnlyClass? IntroducedProperty_Accessors
        {
            get 
            { 
                Console.WriteLine("Get"); 
                return null; 
            }

            set 
            { 
                Console.WriteLine(value); 
            }
        }
        
        
    }
    
    public class RunTimeOnlyClass {}

    // <target>
    [Introduction]
    internal class TargetClass
    {
    }
}
