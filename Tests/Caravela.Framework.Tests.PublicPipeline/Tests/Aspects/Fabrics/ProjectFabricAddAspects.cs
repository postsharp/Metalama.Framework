using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Fabrics.ProjectFabricAddAspects
{
    class Fabric : IProjectFabric
    {
        public void BuildProject( IProjectFabricBuilder builder )
        {
            builder
            .WithTypes( c => c.Types )
            .WithMethods( t => t.Methods.Where( m => m.ReturnType.Is( typeof(string) ) ) )
            .AddAspect<Aspect>();
        }
    }

    class Aspect : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            Console.WriteLine("overridden");
            return meta.Proceed();
        }
    }

    // <target>
    class TargetCode
    {
        int Method1(int a) => a;
        string Method2(string s) => s;
        
    }
}