// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Utilities;
using System;

namespace Metalama.Framework.Engine.CompileTime
{
    internal class TemplateInfo
    {
        /// <summary>
        /// Gets a magic value representing the fact that the member is not a template.
        /// </summary>
        public static TemplateInfo None { get; } = new();

        public bool IsNone => this.AttributeType == TemplateAttributeType.None;

        /// <summary>
        /// Gets a value indicating whether the template member is abstract.
        /// </summary>
        public bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the template member can be referenced from a template as run-time code,
        /// which is typically the case with introductions.
        /// </summary>
        public bool CanBeReferencedAsRunTimeCode => this.AttributeType is TemplateAttributeType.DeclarativeAdvice or TemplateAttributeType.InterfaceMember;

        /// <summary>
        /// Gets the <see cref="TemplateAttribute"/> if it could be instantiated by the <see cref="SymbolClassifier"/>, i.e.
        /// only if it is a system attribute but not if it is defined in user code.
        /// </summary>
        public Attribute? Attribute { get; }

        public TemplateAttributeType AttributeType { get; }

        /// <summary>
        /// Gets the <see cref="SymbolId"/> of the template member.
        /// </summary>
        public SymbolId SymbolId { get; }

        public TemplateInfo( SymbolId symbolId, TemplateAttributeType attributeType, Attribute? attribute )
        {
            this.AttributeType = attributeType;
            this.Attribute = attribute;
            this.SymbolId = symbolId;
        }

        private TemplateInfo()
        {
            this.Attribute = TemplateAttribute.Default;
            this.AttributeType = TemplateAttributeType.None;
        }

        private TemplateInfo( TemplateInfo prototype, bool isAbstract )
        {
            this.Attribute = prototype.Attribute;
            this.AttributeType = prototype.AttributeType;
            this.SymbolId = prototype.SymbolId;
            this.IsAbstract = isAbstract;
        }

        /// <summary>
        /// Returns a copy of the current <see cref="TemplateInfo"/>, but with the <see cref="IsAbstract"/> property set to <c>true</c>.
        /// </summary>
        /// <returns></returns>
        public TemplateInfo AsAbstract() => new( this, true );

        public override string ToString() => $"Type={this.AttributeType}, Attribute={this.Attribute}";
    }
}