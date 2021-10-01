using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.CodeModel.ConvertToReflection
{

    // This tests serialization to reflection with types that cannot be constructed using a typeof.

    class Aspect : Attribute, IAspect<INamedType>
    {
    
        [Introduce]
        public void Run()
        {
         foreach ( var method in meta.Target.Type.Methods )
            {
                foreach ( var parameter in method.Parameters )
                {
                    var type = meta.RunTime( parameter.Type.ToType() );
                }
            }
        }
    }

    // <target>
    [Aspect]
    unsafe class TargetCode
    {
        void SystemTypesOnly( dynamic dyn, dynamic[] dynArray, List<dynamic> dynGeneric ) {}
    }
}