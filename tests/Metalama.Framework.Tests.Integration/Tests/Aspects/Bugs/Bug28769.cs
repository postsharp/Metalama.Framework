using System;
using System.Linq;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Aspects.Bug28769
{
 
    class ConvertToRunTimeAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var parameterNamesCompileTime = meta.Target.Parameters.Select(p => p.Name).ToArray();
            var parameterNames = meta.RunTime(parameterNamesCompileTime);
            return null;
        }
    }

    class TargetCode
    {
        // <target>
        [ConvertToRunTimeAspect]
        void Method(string a, int c, DateTime e) { }
        // </target>
    }
}
