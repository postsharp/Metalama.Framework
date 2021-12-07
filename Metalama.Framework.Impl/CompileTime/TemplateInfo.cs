// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Accessibility = Caravela.Framework.Code.Accessibility;

namespace Caravela.Framework.Impl.CompileTime
{
    internal class TemplateInfo
    {
        public static TemplateInfo None { get; } = new();

        public bool IsNone => this.AttributeType == TemplateAttributeType.None;

        public bool IsAbstract { get; }

        public TemplateAttribute Attribute { get; }

        public TemplateAttributeType AttributeType { get; }

        public TemplateInfo( TemplateAttributeType attributeType, AttributeData attributeData )
        {
            this.AttributeType = attributeType;
            this.Attribute = Parse( attributeType, attributeData );
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

        private static TemplateAttribute Parse( TemplateAttributeType attributeType, AttributeData attributeData )
        {
            var attribute = attributeType switch
            {
                TemplateAttributeType.Introduction => new IntroduceAttribute(),
                TemplateAttributeType.Template => new TemplateAttribute(),
                TemplateAttributeType.InterfaceMember => new InterfaceMemberAttribute(),
                _ => throw new AssertionFailedException()
            };

            var namedArguments = attributeData.NamedArguments.ToDictionary( p => p.Key, p => p.Value );

            bool TryGetNamedArgument<TArg>( string argumentName, [NotNullWhen( true )] out TArg? value )
            {
                if ( namedArguments.TryGetValue( argumentName, out var objectValue ) && objectValue.Value != null )
                {
                    value = (TArg) objectValue.Value;

                    return true;
                }

                value = default;

                return false;
            }

            if ( TryGetNamedArgument<string>( nameof(TemplateAttribute.Name), out var name ) )
            {
                attribute.Name = name;
            }

            if ( TryGetNamedArgument<IntroductionScope>( nameof(TemplateAttribute.Scope), out var scope ) )
            {
                attribute.Scope = scope;
            }

            if ( TryGetNamedArgument<OverrideStrategy>( nameof(TemplateAttribute.WhenExists), out var overrideStrategy ) )
            {
                attribute.WhenExists = overrideStrategy;
            }

            if ( TryGetNamedArgument<bool>( nameof(TemplateAttribute.IsVirtual), out var isVirtual ) )
            {
                attribute.IsVirtual = isVirtual;
            }

            if ( TryGetNamedArgument<bool>( nameof(TemplateAttribute.IsSealed), out var isSealed ) )
            {
                attribute.IsSealed = isSealed;
            }

            if ( TryGetNamedArgument<Accessibility>( nameof(TemplateAttribute.Accessibility), out var accessibility ) )
            {
                attribute.Accessibility = accessibility;
            }

            if ( attributeType == TemplateAttributeType.InterfaceMember )
            {
                if ( TryGetNamedArgument<bool>( nameof(InterfaceMemberAttribute.IsExplicit), out var isExplicit ) )
                {
                    ((InterfaceMemberAttribute) attribute).IsExplicit = isExplicit;
                }
            }

            return attribute;
        }
    }
}