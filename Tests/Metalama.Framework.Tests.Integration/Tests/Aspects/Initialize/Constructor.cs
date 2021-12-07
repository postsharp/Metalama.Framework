using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialize.Constructor
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