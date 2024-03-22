using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AddAspect.Constructor
{
    class Aspect : OverrideMethodAspect
    {
        private string _value;

        public Aspect( string value )
        {
            _value = value;
        }

        public override dynamic? OverrideMethod()
        {
            Console.WriteLine(_value);
            return meta.Proceed();
        }
    }

    class TargetCode
    {
        // <target>
        [Aspect("The Value")]
        int Method(int a)
        {
            return a;
        }
    }
}