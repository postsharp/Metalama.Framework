using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Initialize.LiveTemplateError
{
    class Aspect : Attribute, IAspect<IMethod>
    {
      public Aspect(int x) {}
      
      public void BuildAspectClass( IAspectClassBuilder builder )
      {
          // This should not be allowed because there is no default constructor.
          builder.IsLiveTemplate = true;
      }
      
      public void BuildAspect( IAspectBuilder<IMethod> builder )
      {
        // This should not be called.
        throw new Exception("Oops");
      }
    }
    
    // <target>
    class Target 
    {
    
        [Aspect(0)]
        void M() {}
    }

}