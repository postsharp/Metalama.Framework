using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Properties.Record_Positional_Attribute
{
    internal class MyAspect : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get {
                Console.WriteLine("Sob");
                return meta.Proceed(); }
            set
            {
                meta.Proceed();
            }
        }
    }

    // <target>
    internal record MyRecord( int A, [property: MyAspect] int B );


}
