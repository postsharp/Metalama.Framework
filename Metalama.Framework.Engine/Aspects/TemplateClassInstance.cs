// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// An instance of a template class, i.e. the transformed class containing the compiled templates.
    /// For a normal template, the template <see cref="Instance"/> is the aspect instance itself. For fabrics,
    /// the template <see cref="Instance"/> is the transformed fabric class.
    /// </summary>
    internal class TemplateClassInstance
    {
        public object Instance { get; }

        public TemplateClass TemplateClass { get; }

        public TemplateClassInstance( object instance, TemplateClass templateClass )
        {
            this.Instance = instance;
            this.TemplateClass = templateClass;
        }
    }
}