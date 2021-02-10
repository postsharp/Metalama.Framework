using System;
using Caravela.Framework.Code;

using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class ReturnParameter : IParameter
    {
        internal static RefKind MapRefKind( Microsoft.CodeAnalysis.RefKind roslynRefKind ) => roslynRefKind switch
        {
            Microsoft.CodeAnalysis.RefKind.None => RefKind.None,
            Microsoft.CodeAnalysis.RefKind.Ref => RefKind.Ref,
            Microsoft.CodeAnalysis.RefKind.RefReadOnly => RefKind.RefReadonly,
            _ => throw new InvalidOperationException( $"Roslyn RefKind {roslynRefKind} not recognized here." )
        };

        protected abstract Microsoft.CodeAnalysis.RefKind SymbolRefKind { get; }

        public RefKind RefKind => MapRefKind( this.SymbolRefKind );

        public bool IsByRef => this.RefKind != RefKind.None;

        public bool IsRef => this.RefKind == RefKind.Ref;

        public bool IsOut => false;

        public abstract IType Type { get; }

        public string? Name => null;

        public int Index => -1;

        public abstract CodeElement? ContainingElement { get; }

        public abstract IImmutableList Attributes { get; }

        public CodeElementKind ElementKind => CodeElementKind.Parameter;

        public bool HasDefaultValue => false;

        public object? DefaultValue => throw new InvalidOperationException();

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();
    }
}
