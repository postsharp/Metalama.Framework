// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Engine.CompileTime
{
    internal class TemplateInfo
    {
        public static TemplateInfo None { get; } = new();

        public bool IsNone => this.AttributeType == TemplateAttributeType.None;

        public bool IsAbstract { get; }

        public TemplateAttribute Attribute { get; }

        public TemplateAttributeType AttributeType { get; }

        public TemplateInfo( TemplateAttributeType attributeType, TemplateAttribute attribute )
        {
            this.AttributeType = attributeType;
            this.Attribute = attribute;
        }

        private TemplateInfo()
        {
            this.Attribute = new TemplateAttribute();
            this.AttributeType = TemplateAttributeType.None;
        }

        private TemplateInfo( TemplateInfo prototype, bool isAbstract )
        {
            this.Attribute = prototype.Attribute;
            this.AttributeType = prototype.AttributeType;
            this.IsAbstract = isAbstract;
        }

        public TemplateInfo AsAbstract() => new( this, true );
    }
}