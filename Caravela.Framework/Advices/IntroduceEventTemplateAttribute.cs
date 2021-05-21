// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Advices
{
    /// <summary>
    /// Custom attribute that marks the target method as a template for <see cref="IIntroduceMethodAdvice"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Event )]
    public class IntroduceEventTemplateAttribute : IntroduceMemberTemplateAttribute
    {
        private bool? _isVirtual;
        private bool? _isSealed;

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