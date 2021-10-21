using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Fabrics.TypeFabricAddAdvice
{

    // <target>
    class TargetCode
    {
        int Method1(int a) => a;
        string Method2(string s) => s;
        
        
        class Fabric : ITypeFabric
        {
            public void AmendType( ITypeAmender amender )
            {
                foreach ( var method in amender.Target.Methods )
                {
                    amender.Advices.OverrideMethod( method, nameof(Template) );
                }
            }
            
            [Template]
            dynamic? Template()
            {
                Console.WriteLine("overridden");
                return meta.Proceed();
            }
        
        }
        
    }
}