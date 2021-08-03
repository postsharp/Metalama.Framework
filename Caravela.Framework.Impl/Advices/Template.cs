// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Advices
{
    internal readonly struct Template<T>
        where T : IMemberOrNamedType
    {
        public T? Declaration { get; }

        public TemplateSelectionKind SelectedKind { get; }

        public TemplateSelectionKind InterpretedKind { get; }

        public bool IsNull => this.SelectedKind == TemplateSelectionKind.None;

        public bool IsNotNull => this.SelectedKind != TemplateSelectionKind.None;

        public Template( T implementation, TemplateSelectionKind selectedKind ) : this( implementation, selectedKind, selectedKind ) { }

        public Template( T implementation, TemplateSelectionKind selectedKind, TemplateSelectionKind interpretedKind )
        {
            this.Declaration = implementation;
            this.SelectedKind = selectedKind;
            this.InterpretedKind = interpretedKind != TemplateSelectionKind.None ? interpretedKind : selectedKind;
        }

        public override string ToString() => this.IsNull ? "null" : $"{this.Declaration!.Name}:{this.SelectedKind}";
    }
}