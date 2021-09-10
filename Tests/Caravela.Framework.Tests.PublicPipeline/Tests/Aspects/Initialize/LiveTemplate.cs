using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Initialize.LiveTemplate
{
    class Aspect : Attribute, IAspect<IMethod>
    {
      public void BuildAspectClass( IAspectClassBuilder builder )
      {
          // This should be allowed.
          builder.IsLiveTemplate = true;
      }
    }
    
    
    // <target>
    class T {}

}