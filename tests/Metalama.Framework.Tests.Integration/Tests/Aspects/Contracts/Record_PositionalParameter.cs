using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Report_PositionalParameter
{
    internal class NotNullAttribute : ContractAspect
    {
        public override void Validate(dynamic? value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(meta.Target.Property.Name);
            }
        }
    }

    // <target>
    internal record Target([NotNull] string m);
}
