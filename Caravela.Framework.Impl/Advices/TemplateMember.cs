// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CompileTime;

namespace Caravela.Framework.Impl.Advices
{
    internal static class TemplateMember
    {
        public static TemplateMember<T> Create<T>( T? implementation, TemplateInfo templateInfo, TemplateKind selectedKind, TemplateKind interpretedKind )
            where T : class, IMemberOrNamedType
            => new( implementation, templateInfo, selectedKind, interpretedKind );

        public static TemplateMember<T> Create<T>( T? implementation, TemplateInfo templateInfo, TemplateKind selectedKind = TemplateKind.Default )
            where T : class, IMemberOrNamedType
            => new( implementation, templateInfo, selectedKind );
    }

    internal readonly struct TemplateMember<T>
        where T : class, IMemberOrNamedType
    {
        public T? Declaration { get; }

        public TemplateInfo TemplateInfo { get; }

        public TemplateKind SelectedKind { get; }

        public TemplateKind InterpretedKind { get; }

        public bool IsNull => this.SelectedKind == TemplateKind.None;

        public bool IsNotNull => this.SelectedKind != TemplateKind.None;

        public TemplateMember( T? implementation, TemplateInfo templateInfo, TemplateKind selectedKind = TemplateKind.Default ) : this(
            implementation,
            templateInfo,
            selectedKind,
            selectedKind )
        { }

        public TemplateMember( T? implementation, TemplateInfo templateInfo, TemplateKind selectedKind, TemplateKind interpretedKind )
        {
            this.Declaration = implementation;
            this.TemplateInfo = templateInfo;

            if ( implementation != null )
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
            => TemplateMember.Create<IMemberOrNamedType>( this.Declaration!, this.TemplateInfo, this.SelectedKind, this.InterpretedKind );

        public override string ToString() => this.IsNull ? "null" : $"{this.Declaration!.Name}:{this.SelectedKind}";
    }
}