using System;
using Caravela.Framework.Aspects;


// ReSharper disable UnusedMember.Local

namespace Caravela.Framework.TestApp
{
    internal class ThisAccess
    {
        private static string StaticProperty => "SP";

        private string InstanceProperty => "IP";

        public static void Run()
        {
            Console.WriteLine( StaticMethod() );
            Console.WriteLine( new ThisAccess().InstanceMethod() );
        }

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
    }

    internal class ThisAccessAspect : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            string result = proceed();

            result += ": ";

            result += meta.This.StaticProperty;

            if ( !meta.CurrentMethod.IsStatic )
            {
                result += meta.This.InstanceProperty;
                result += meta.This;
            }

            return result;
        }
    }
}
