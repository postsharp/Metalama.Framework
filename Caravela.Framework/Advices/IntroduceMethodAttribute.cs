// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Code;

namespace Caravela.Framework.Advices
{
    [AttributeUsage( AttributeTargets.Method, Inherited = true )]
    public class IntroduceMethodAttribute : IntroduceMethodTemplateAttribute, IAdviceAttribute<IIntroduceMethodAdvice>
    {
        public IntroductionScope? Scope { get; set; }

        public string? Name { get; set; }

        public bool IsStatic { get; set; }

        public bool IsVirtual { get; set; }

        public Accessibility? Visibility { get; set; }

        public bool IsSealed { get; set; }
    }
}
