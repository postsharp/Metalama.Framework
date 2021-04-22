﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Advices
{
    /// <summary>
    /// Custom attribute that marks the target method as a template for <see cref="IIntroduceMethodAdvice"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Event, Inherited = true )]
    public class IntroduceEventTemplateAttribute : TemplateAttribute
    {
        private Accessibility? _accessibility;
        private bool? _isVirtual;
        private bool? _isSealed;

        public string? Name { get; set; }

        public Accessibility Accessibility
        {
            get
                => this._accessibility
                   ?? throw new InvalidOperationException( $"Visibility was not set, use {nameof(this.GetAccessibility)} to get nullable value." );
            set => this._accessibility = value;
        }

        public bool IsVirtual
        {
            get => this._isVirtual ?? throw new InvalidOperationException( $"Visibility was not set, use {nameof(this.GetIsVirtual)} to get nullable value." );
            set => this._isVirtual = value;
        }

        public bool IsSealed
        {
            get => this._isSealed ?? throw new InvalidOperationException( $"Visibility was not set, use {nameof(this.GetIsSealed)} to get nullable value." );
            set => this._isSealed = value;
        }

        public IntroductionScope Scope { get; set; }

        public ConflictBehavior ConflictBehavior { get; set; }

        public Accessibility? GetAccessibility()
        {
            return this._accessibility;
        }

        public bool? GetIsVirtual()
        {
            return this._isVirtual;
        }

        public bool? GetIsSealed()
        {
            return this._isSealed;
        }
    }
}