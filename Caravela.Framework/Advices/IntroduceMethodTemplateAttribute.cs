// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Advices
{
    /// <summary>
    /// Custom attribute that marks the target method as a template for <see cref="IIntroduceMethodAdvice"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public class IntroduceMethodTemplateAttribute : TemplateAttribute
    {
        private Accessibility? _accessibility;
        private bool? _isVirtual;
        private bool? _isSealed;

        public string? Name { get; set; }

        public Accessibility Accessibility
        {
            get
                => this._accessibility
                   ?? throw new InvalidOperationException(
                       $"{nameof(this.Accessibility)} was not set, use {nameof(this.GetAccessibility)} to get nullable value." );
            set => this._accessibility = value;
        }

        public bool IsVirtual
        {
            get
                => this._isVirtual ?? throw new InvalidOperationException(
                    $"{nameof(this.IsVirtual)} was not set, use {nameof(this.GetIsVirtual)} to get nullable value." );
            set => this._isVirtual = value;
        }

        public bool IsSealed
        {
            get
                => this._isSealed
                   ?? throw new InvalidOperationException( $"{nameof(this.IsSealed)} was not set, use {nameof(this.GetIsSealed)} to get nullable value." );
            set => this._isSealed = value;
        }

        public IntroductionScope Scope { get; set; }

        public ConflictBehavior ConflictBehavior { get; set; }

        public Accessibility? GetAccessibility() => this._accessibility;

        public bool? GetIsVirtual() => this._isVirtual;

        public bool? GetIsSealed() => this._isSealed;
    }
}