using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Initialize.DuplicateMemberName
{
    class Aspect1 : Attribute, IAspect<IMethod>
    {
        // Error: two templates of the same name in the class.
        
       [Template]
       public void Template() {}
       [Template]
       public void Template(int x) {}
    }
    
    class TargetCode
    {
     [Aspect1]
      void M(){}
    }

   
}