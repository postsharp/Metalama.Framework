using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class Property : Member, IProperty, IEquatable<Property>
    {
        public abstract RefKind RefKind { get; }

        public bool IsByRef => this.RefKind != RefKind.None;

        public bool IsRef => this.RefKind == RefKind.Ref;

        public bool IsRefReadonly => this.RefKind == RefKind.RefReadonly;

        IType IProperty.Type => this.Type;

        public abstract ITypeInternal Type { get; }

        IReadOnlyList<IParameter> IProperty.Parameters => this.Parameters;

        public abstract IReadOnlyList<Parameter> Parameters { get; }

        IMethod? IProperty.Getter => this.Getter;

        public abstract Method? Getter { get; }

        IMethod? IProperty.Setter => this.Setter;

        public abstract Method? Setter { get; }
        
    }
}
