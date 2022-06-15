// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime;
using System;

namespace Metalama.Framework.Engine.Advising
{
    internal static class TemplateMember
    {
        public static TemplateMember<T> Create<T>(
            T? implementation,
            TemplateClassMember? templateClassMember,
            TemplateAttribute templateAttribute,
            TemplateKind selectedKind,
            TemplateKind interpretedKind )
            where T : class, IMemberOrNamedType
            => new( implementation, templateClassMember, templateAttribute, selectedKind, interpretedKind );

        public static TemplateMember<T> Create<T>(
            T? implementation,
            TemplateClassMember? templateClassMember,
            TemplateAttribute templateAttribute,
            TemplateKind selectedKind = TemplateKind.Default )
            where T : class, IMemberOrNamedType
            => new( implementation, templateClassMember, templateAttribute, selectedKind );

        public static TemplateMember<T> Create<T>(
            T? implementation,
            TemplateClassMember templateClassMember,
            TemplateKind selectedKind = TemplateKind.Default )
            where T : class, IMemberOrNamedType
            => new( implementation, templateClassMember, templateClassMember.TemplateInfo.Attribute.AssertNotNull(), selectedKind );
    }

    internal readonly struct TemplateMember<T>
        where T : class, IMemberOrNamedType
    {
        private readonly TemplateClassMember? _templateClassMember;

        public T? Declaration { get; }

        public TemplateClassMember TemplateClassMember => this._templateClassMember ?? throw new InvalidOperationException();

        // Can be null in the default instance.
        public TemplateAttribute? TemplateAttribute { get; }

        public TemplateKind SelectedKind { get; }

        public TemplateKind InterpretedKind { get; }

        public bool IsNull => this.SelectedKind == TemplateKind.None;

        public bool IsNotNull => this.SelectedKind != TemplateKind.None;

        public TemplateMember(
            T? implementation,
            TemplateClassMember? templateClassMember,
            TemplateAttribute templateAttribute,
            TemplateKind selectedKind = TemplateKind.Default ) : this(
            implementation,
            templateClassMember,
            templateAttribute,
            selectedKind,
            selectedKind ) { }

        public TemplateMember(
            T? implementation,
            TemplateClassMember? templateClassMember,
            TemplateAttribute templateAttribute,
            TemplateKind selectedKind,
            TemplateKind interpretedKind )
        {
            this.Declaration = implementation;
            this._templateClassMember = templateClassMember;
            this.TemplateAttribute = templateAttribute.AssertNotNull();

            if ( implementation is IMethod { MethodKind: MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove }
                 && templateClassMember?.Parameters.Length != 1 )
            {
                throw new AssertionFailedException();
            }

            if ( implementation != null && templateClassMember != null )
            {
                this.SelectedKind = selectedKind;
                this.InterpretedKind = interpretedKind != TemplateKind.None ? interpretedKind : selectedKind;
            }
            else
            {
                this.SelectedKind = TemplateKind.None;
                this.InterpretedKind = TemplateKind.None;
            }
        }

        public TemplateMember<IMemberOrNamedType> Cast()
            => TemplateMember.Create<IMemberOrNamedType>(
                this.Declaration!,
                this.TemplateClassMember,
                this.TemplateAttribute.AssertNotNull(),
                this.SelectedKind,
                this.InterpretedKind );

        public override string ToString() => this.IsNull ? "null" : $"{this.Declaration!.Name}:{this.SelectedKind}";
    }
}