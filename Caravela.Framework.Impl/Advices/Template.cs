// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Advices
{
    internal readonly struct Template<T>
        where T : IMemberOrNamedType
    {
        public T? Declaration { get; }

        public TemplateKind Kind { get; }

        public bool IsNull => this.Kind == TemplateKind.None;

        public bool IsNotNull => this.Kind != TemplateKind.None;

        public Template( T implementation, TemplateKind kind ) : this()
        {
            this.Declaration = implementation;
            this.Kind = kind;
        }

        public override string ToString() => this.IsNull ? "null" : $"{this.Declaration!.Name}:{this.Kind}";
    }
}