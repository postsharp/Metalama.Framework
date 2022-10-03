// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// The base class for all custom attributes that mark a declaration as a template.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event )]
    public class TemplateAttribute : Attribute, ITemplateAttribute
    {
        internal static TemplateAttribute Default { get; } = new();

        private TemplateAttributeImpl _impl;

        public string? Name { get => this._impl.Name; set => this._impl.Name = value; }

        public Accessibility Accessibility
        {
            get => this._impl.Accessibility;
            set => this._impl.Accessibility = value;
        }

        public bool IsVirtual
        {
            get => this._impl.IsVirtual;

            set => this._impl.IsVirtual = value;
        }

        public bool IsSealed
        {
            get => this._impl.IsSealed;
            set => this._impl.IsSealed = value;
        }

        bool? ITemplateAttribute.IsVirtual => this._impl.GetIsVirtual();

        bool? ITemplateAttribute.IsSealed => this._impl.GetIsSealed();

        Accessibility? ITemplateAttribute.Accessibility => this._impl.GetAccessibility();
    }
}