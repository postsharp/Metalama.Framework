// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
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
        private TemplateAttributeProperties _properties = new();

        internal static TemplateAttribute Default { get; } = new();

        public string? Name
        {
            get => this._properties.Name;
            set => this._properties = this._properties with { Name = value };
        }

        public Accessibility Accessibility
        {
            get => this._properties.Accessibility.GetValueOrDefault();
            set => this._properties = this._properties with { Accessibility = value };
        }

        public bool IsVirtual
        {
            get => this._properties.IsVirtual.GetValueOrDefault();

            set => this._properties = this._properties with { IsVirtual = value };
        }

        public bool IsSealed
        {
            get => this._properties.IsSealed.GetValueOrDefault();
            set => this._properties = this._properties with { IsSealed = value };
        }

        public bool IsRequired
        {
            get => this._properties.IsRequired.GetValueOrDefault();
            set => this._properties = this._properties with { IsRequired = value };
        }

        /// <summary>
        /// Gets or sets a value indicating whether the template is an empty implementation, which means that the framework will consider the template
        /// to be undefined unless it is overridden in a derived class. It is similar to an abstract template implementation, but aspect deriving
        /// from the abstract class is not obliged to provide an implementation for the empty but non-abstract template.
        /// </summary>
        public bool IsEmpty { get; set; }

        TemplateAttributeProperties? ITemplateAttribute.Properties => this._properties;
    }
}