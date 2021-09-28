// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Aspects
{
    internal class TemplateClassInstance
    {
        public object Instance { get; }

        public TemplateClass TemplateClass { get; }

        public IDeclaration TargetDeclaration { get; }

        public TemplateClassInstance( object instance, TemplateClass templateClass, IDeclaration targetDeclaration )
        {
            this.Instance = instance;
            this.TemplateClass = templateClass;
            this.TargetDeclaration = targetDeclaration;
        }
    }
}