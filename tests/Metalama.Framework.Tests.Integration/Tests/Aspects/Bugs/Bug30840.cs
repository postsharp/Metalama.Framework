#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug30840
{
    public class TrackedObjectAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var fieldOrProperty in builder.Target.FieldsAndProperties.Where( x => !x.IsImplicitlyDeclared ))
            {
                builder.Advice.OverrideAccessors( fieldOrProperty, null, nameof(OverrideSetter) );
            }
        }

        [Template]
        private dynamic OverrideSetter( dynamic value )
        {
            meta.Proceed();
            Console.WriteLine( "Overridden setter" );

            return value;
        }
    }

    // <target>
    [TrackedObject]
    public struct TrackedClass
    {
        public int i { get; set; }
    }
}