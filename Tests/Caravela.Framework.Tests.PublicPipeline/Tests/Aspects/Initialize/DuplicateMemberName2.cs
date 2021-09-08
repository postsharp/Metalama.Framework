using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Initialize.DuplicateMemberName2
{

    // Error: the base class already defines a template and this is not an override.

    class Aspect1 : Attribute, IAspect<IMethod>
    {
       [Template]
       public void Template() {}
    }
    
    class Aspect2 : Aspect1
    {
       [Template]
       public new void Template() {}
    }
    
    // <target>
    class Target {}
   
}