using System;
using Caravela.Framework.Aspects;
using static Caravela.Framework.Aspects.TemplateContext;

// ReSharper disable UnusedMember.Local

namespace Caravela.Framework.TestApp
{
    internal class ThisAccess
    {
        private static string StaticProperty => "SP";

        private string InstanceProperty => "IP";

        [ThisAccessAspect]
        private static string StaticMethod()
        {
            return "SM";
        }

        [ThisAccessAspect]
        private string InstanceMethod()
        {
            return "IM";
        }

        public override string ToString()
        {
            return "this";
        }

        public static void Run()
        {
            Console.WriteLine( StaticMethod() );
            Console.WriteLine( new ThisAccess().InstanceMethod() );
        }
    }

    internal class ThisAccessAspect : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            string result = proceed();

            result += ": ";

            result += target.This.StaticProperty;

            if ( !target.Method.IsStatic )
            {
                result += target.This.InstanceProperty;
                result += target.This;
            }

            return result;
        }
    }
}
