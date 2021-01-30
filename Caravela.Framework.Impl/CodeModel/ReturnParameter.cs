using System;
using Caravela.Framework.Code;
using Caravela.Reactive;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    abstract class ReturnParameter : IParameter
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

        public abstract ICodeElement? ContainingElement { get; }

        public abstract IReactiveCollection<IAttribute> Attributes { get; }

        public CodeElementKind ElementKind => CodeElementKind.Parameter;

        public bool HasDefaultValue => false;

        public object? DefaultValue => throw new InvalidOperationException( "Return parameter can't have default value." );

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext context = null ) => throw new NotImplementedException();
    }
}
