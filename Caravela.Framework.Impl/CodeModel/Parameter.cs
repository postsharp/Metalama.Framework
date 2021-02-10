using System;
using Caravela.Framework.Code;
using System.Collections.Generic;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class Parameter : IParameter, IEquatable<Parameter>
    {

        public abstract RefKind RefKind { get; }
        public bool IsByRef => this.RefKind != RefKind.None;

        public bool IsRef => this.RefKind == RefKind.Ref;

        public bool IsOut => this.RefKind == RefKind.Out;
        public abstract IType Type { get; }
        public abstract string? Name { get; }
        public abstract int Index { get; }
        public abstract bool HasDefaultValue { get; }

        public abstract object? DefaultValue { get; }


        public abstract CodeElement? ContainingElement { get; }
        
        IReadOnlyList<IAttribute> ICodeElement.Attributes => this.Attributes;

        ICodeElement? ICodeElement.ContainingElement => this.ContainingElement;

        public abstract IReadOnlyList<Attribute> Attributes { get; }

        public CodeElementKind ElementKind => CodeElementKind.Parameter;



        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );
        bool IEquatable<ICodeElement>.Equals( ICodeElement other ) => other is Parameter p && this.Equals( p );
        public abstract bool Equals( Parameter other );
    }
}
