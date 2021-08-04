// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Advices
{
    internal readonly struct Template<T>
        where T : IMemberOrNamedType
    {
        public T? Declaration { get; }

        public TemplateKind SelectedKind { get; }

        public TemplateKind InterpretedKind { get; }

        public bool IsNull => this.SelectedKind == TemplateKind.None;

        public bool IsNotNull => this.SelectedKind != TemplateKind.None;

        public Template( T? implementation, TemplateKind selectedKind = TemplateKind.Default ) : this( implementation, selectedKind, selectedKind ) { }

        public Template( T? implementation, TemplateKind selectedKind, TemplateKind interpretedKind )
        {
            this.Declaration = implementation;

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

        public Template<IMemberOrNamedType> Cast() => new( this.Declaration!, this.SelectedKind, this.InterpretedKind );

        public override string ToString() => this.IsNull ? "null" : $"{this.Declaration!.Name}:{this.SelectedKind}";
    }
}