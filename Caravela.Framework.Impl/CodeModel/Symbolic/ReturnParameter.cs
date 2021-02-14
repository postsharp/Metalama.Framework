using System;
using System.Collections.Generic;
using Caravela.Framework.Code;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class ReturnParameter : IParameter
    {
    
        protected abstract Microsoft.CodeAnalysis.RefKind SymbolRefKind { get; }

        public RefKind RefKind => this.SymbolRefKind.ToOurRefKind();

        public abstract IType Type { get; }

        public string? Name => null;

        public int Index => -1;

        OptionalValue IParameter.DefaultValue => default;

        public bool IsParams => false;

        public abstract ICodeElement? ContainingElement { get; }

        public abstract IReadOnlyList<IAttribute> Attributes { get; }

        public CodeElementKind ElementKind => CodeElementKind.Parameter;

        public ICompilation Compilation => this.ContainingElement?.Compilation ?? throw new AssertionFailedException();

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        public abstract bool Equals( ICodeElement other );
    }
}
