// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Advices
{
    public abstract class IntroduceMemberTemplateAttribute : TemplateAttribute
    {
        private Accessibility? _accessibility;

        public string? Name { get; set; }

        public Accessibility Accessibility
        {
            get
                => this._accessibility
                   ?? throw new InvalidOperationException(
                       $"{nameof(this.Accessibility)} was not set, use {nameof(this.GetAccessibility)} to get nullable value." );
            set => this._accessibility = value;
        }

        public IntroductionScope Scope { get; set; }

        public ConflictBehavior ConflictBehavior { get; set; }

        public Accessibility? GetAccessibility() => this._accessibility;
    }
}