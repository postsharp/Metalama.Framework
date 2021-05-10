using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.Declarative
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
        }

        // TODO: Indexers.    

        [IntroduceProperty]
        public int IntroducedProperty_Auto { get; set; }

        [IntroduceProperty]
        public static int IntroducedProperty_Auto_Static { get; }

        public int IntroducedProperty_Accessors
        {
            get { Console.WriteLine("Get"); return 42; }
            set { Console.WriteLine($"Set {value}"); }
        }

        public int IntroducedProperty_AccessorExpressions
        {
            get => 42;
            set => Console.WriteLine($"Set {value}");
        }
    }

    [TestOutput]
    [Introduction]
    internal class TargetClass
    {
        public int IntroducedProperty_Auto { get; set; }

        public static int IntroducedProperty_Auto_Static { get; }

        public int IntroducedProperty_Accessors
        {
            get { Console.WriteLine("Get"); return 42; }
            set { Console.WriteLine($"Set {value}"); }
        }

        public int IntroducedProperty_AccessorExpressions
        {
            get => 42;
            set => Console.WriteLine($"Set {value}");
        }
    }
}
