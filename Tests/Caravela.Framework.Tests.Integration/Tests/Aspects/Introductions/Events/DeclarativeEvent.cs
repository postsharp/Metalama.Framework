#pragma warning disable CS0067

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;
using System;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.DeclarativeEvent
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce]
        public event EventHandler? Event
        {
            add 
            { 
                Console.WriteLine("Original add accessor."); 
            }

            remove 
            { 
                Console.WriteLine("Original remove accessor."); 
            }
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
    }
}
