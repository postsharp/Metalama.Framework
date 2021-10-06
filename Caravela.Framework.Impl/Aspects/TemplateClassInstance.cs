// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Aspects
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