using Caravela.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Advices
{ 
    public class OverrideMethodAttribute : OverrideMethodTemplateAttribute, IAdviceAttribute<IOverrideMethodAdvice>
    {
    }
}
