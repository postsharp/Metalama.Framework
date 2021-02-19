using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Transformations
{
    internal sealed class IntroductionContext
    {
        public TemplateExpansionContext TemplateExpansionContext { get; }

        public IntroductionContext( TemplateExpansionContext templateExpansionContext )
        {
            this.TemplateExpansionContext = templateExpansionContext;
        }
    }
}
