using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS0067,CS0414

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.TransformTemplateAccessibility
{

#pragma warning disable CS0067, CS8618, CA1822, CS0162, CS0169, CS0414
    /*
     
    Tests that the visibility of all templates, including accessors, is set to 'public', and
    that an [Accessibility] attribute is added.

     */

    internal class MyAspect : Framework.Aspects.TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");


        [Template]
[global::Metalama.Framework.Aspects.CompiledTemplateAttribute(Accessibility=global::Metalama.Framework.Code.Accessibility.Private)]
public void MethodTemplate() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");


        [Template]
[global::Metalama.Framework.Aspects.CompiledTemplateAttribute(Accessibility=global::Metalama.Framework.Code.Accessibility.Internal)]
public int AutomaticPropertyTemplate { [global::Metalama.Framework.Aspects.CompiledTemplateAttribute(Accessibility=global::Metalama.Framework.Code.Accessibility.Internal)] get; [global::Metalama.Framework.Aspects.CompiledTemplateAttribute(Accessibility=global::Metalama.Framework.Code.Accessibility.Private)] set; }

        [Template]
[global::Metalama.Framework.Aspects.CompiledTemplateAttribute(Accessibility=global::Metalama.Framework.Code.Accessibility.Internal)]
public int ExplicitPropertyTemplate { [global::Metalama.Framework.Aspects.CompiledTemplateAttribute(Accessibility=global::Metalama.Framework.Code.Accessibility.Internal)] get => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time."); [global::Metalama.Framework.Aspects.CompiledTemplateAttribute(Accessibility=global::Metalama.Framework.Code.Accessibility.Private)] set => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time."); }


        [Template]
        private int _fieldTemplate ;

        [Template]
        internal event EventHandler? EventTemplate;
    }


#pragma warning restore CS0067, CS8618, CA1822, CS0162, CS0169, CS0414
}
