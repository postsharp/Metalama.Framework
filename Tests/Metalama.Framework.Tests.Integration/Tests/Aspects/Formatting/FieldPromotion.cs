using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting.FieldPromotion
{
    public class TestAspect : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        { 
            get
            {
                Console.WriteLine("Aspect code");
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine("Aspect code");
                meta.Proceed();
            }
        }
    }

    // <target>
    public class Target
    {
        [TestAspect]
        private int _myField;
    }
}
