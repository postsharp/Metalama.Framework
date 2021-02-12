// unset

using System;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class Member : CodeElement, IMember
    {
        public bool IsSealed => this.Symbol.IsSealed;

        [Memo]
        public NamedType DeclaringType => this.Compilation.GetNamedType( this.Symbol.ContainingType );

        protected Member( CompilationModel compilation ) : base( compilation )
        {
        }

        public Visibility Visibility => this.Symbol.DeclaredAccessibility switch
        {
            Accessibility.NotApplicable => Visibility.Private,
            Accessibility.Private => Visibility.Private,
            Accessibility.ProtectedAndInternal => Visibility.ProtectedAndInternal,
            Accessibility.Protected => Visibility.Protected,
            Accessibility.Internal => Visibility.Internal,
            Accessibility.ProtectedOrInternal => Visibility.ProtectedOrInternal,
            Accessibility.Public => Visibility.Public,
            _ => throw new ArgumentOutOfRangeException()
        };

        public string Name => this.Symbol.Name;

        public bool IsStatic => this.Symbol.IsStatic;

        public bool IsVirtual => this.Symbol.IsVirtual;

        public sealed override ICodeElement? ContainingElement => this.Compilation.GetNamedTypeOrMethod( this.Symbol.ContainingSymbol );

        INamedType IMember.DeclaringType => this.DeclaringType;
    }
}