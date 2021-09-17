using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Misc.CompileTimeRecord
{
    internal class Aspect : Attribute, IAspect<IMethod>
    {
        public void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            // This is just to test that it builds.
            CompileTimeRecord r = new( 0, "" );
        }
    }

    [CompileTimeOnly]
    internal record CompileTimeRecord( int a, string b );

    internal record RunTimeRecord( int a, string b );

    // <target>
    internal class TargetCode
    {
        [Aspect]
        private int Method( int a )
        {
            return a;
        }
    }
}