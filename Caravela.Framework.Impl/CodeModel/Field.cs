using System.Collections.Generic;
using Caravela.Framework.Code;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class Field : Member, IProperty
    {
        public abstract RefKind RefKind { get; }

        public bool IsByRef => this.RefKind != RefKind.None;

        public bool IsRef => this.RefKind == RefKind.Ref;

        public bool IsRefReadonly => this.RefKind == RefKind.RefReadonly;

        public abstract IType Type { get; }

        public abstract IReadOnlyList<IParameter> Parameters { get; }

        public abstract IMethod? Getter { get; }

        public abstract IMethod? Setter { get; }
    }
}
