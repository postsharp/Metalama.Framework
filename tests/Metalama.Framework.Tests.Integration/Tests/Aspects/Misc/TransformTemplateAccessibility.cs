﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS0067, CS0414

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.TransformTemplateAccessibility
{
    /*
     
    Tests that the visibility of all templates, including accessors, is not changed, and
    that an [Accessibility] attribute is added.

     */

    internal class MyAspect : Framework.Aspects.TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            base.BuildAspect(builder);
        }

        [Template]
        private void MethodTemplate() { }

        [Template]
        internal int AutomaticPropertyTemplate { get; private set; }

        [Template]
        internal int ExplicitPropertyTemplate { get => 5; private set { } }

        [Template]
        private int _fieldTemplate = 5;

        [Template]
        internal event EventHandler? EventTemplate;
    }
}
