using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Constructor_Parameter_Out
{
    internal class NotNullAttribute : ContractAspect
    {
        public override void Validate(dynamic? value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(meta.Target.Parameter.Name);
            }
        }
    }

    // <target>
    internal class Target
    {
        public Target([NotNull]  out string m) 
        {
            m = "";

 }
    }
}
