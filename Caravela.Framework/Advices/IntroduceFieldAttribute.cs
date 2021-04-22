// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Project;
using System;

namespace Caravela.Framework.Advices
{
    /// <summary>
    /// Custom attribute that marks the target method as a template for <see cref="IOverrideMethodAdvice"/> and results in creation of the advice.
    /// </summary>
    [AttributeUsage( AttributeTargets.Field, Inherited = true )]
    public class IntroduceFieldAttribute : CompileTimeOnlyAttribute, IAdviceAttribute<IIntroduceMethodAdvice>
    {
        private Accessibility? _accessibility;

        public string? Name { get; set; }

        public Accessibility Accessibility
        {
            get
                => this._accessibility
                   ?? throw new InvalidOperationException( $"Visibility was not set, use {nameof(this.GetAccessibility)} to get nullable value." );
            set => this._accessibility = value;
        }

        public IntroductionScope Scope { get; set; }

        public ConflictBehavior ConflictBehavior { get; set; }

        public Accessibility? GetAccessibility()
        {
            return this._accessibility;
        }
    }
}