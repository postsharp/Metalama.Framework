// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Advices
{
    /// <summary>
    /// Custom attribute that marks the target method as a template for <see cref="IOverrideMethodAdvice"/> and results in creation of the advice.
    /// </summary>
    [AttributeUsage( AttributeTargets.Field )]
    public class IntroduceFieldAttribute : IntroduceMemberTemplateAttribute, IAdviceAttribute<IIntroduceMethodAdvice> { }
}