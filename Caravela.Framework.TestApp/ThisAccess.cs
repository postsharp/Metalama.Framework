using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.TestApp
{
    class ThisAccess
    {
        static string StaticProperty => "SP";
        string InstanceProperty => "IP";

        [ThisAccessAspect]
        static string StaticMethod()
        {
            return "SM";
        }

        [ThisAccessAspect]
        string InstanceMethod()
        {
            return "IM";
        }

        public override string ToString()
        {
            return "this";
        }

        public static void Run()
        {
            Console.WriteLine(StaticMethod());
            Console.WriteLine(new ThisAccess().InstanceMethod());
        }
    }

    class ThisAccessAspect : Attribute, IAspect<IMethod>
    {
        public void Initialize( IAspectBuilder<IMethod> aspectBuilder ) { }

        [OverrideMethod]
        dynamic ThisAccess()
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
