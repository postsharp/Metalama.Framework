using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using Accessibility = Caravela.Framework.Code.Accessibility;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class Member : CodeElement, IMember, IMemberLink<IMember>
    {
        public bool IsSealed => this.Symbol.IsSealed;

        public abstract bool IsReadOnly { get; }

        public bool IsOverride => this.Symbol.IsOverride;

        public bool IsNew
        {
            get
            {
                var syntaxReference = this.Symbol.DeclaringSyntaxReferences.FirstOrDefault();
                if ( syntaxReference == null )
                {
                    return false;
                }

                return ((MemberDeclarationSyntax) syntaxReference.GetSyntax()).Modifiers.Any( m => m.Kind() == SyntaxKind.NewKeyword );
            }
        }

        public abstract bool IsAsync { get; }

        public bool IsAbstract => this.Symbol.IsAbstract;

        public bool IsStatic => this.Symbol.IsStatic;

        public bool IsVirtual => this.Symbol.IsVirtual;

        [Memo]
        public NamedType DeclaringType => this.Compilation.Factory.GetNamedType( this.Symbol.ContainingType );

        protected Member( CompilationModel compilation ) : base( compilation )
        {
        }

        public Accessibility Accessibility => this.Symbol.DeclaredAccessibility switch
        {
            Microsoft.CodeAnalysis.Accessibility.NotApplicable => Accessibility.Private,
            Microsoft.CodeAnalysis.Accessibility.Private => Accessibility.Private,
            Microsoft.CodeAnalysis.Accessibility.ProtectedAndInternal => Accessibility.ProtectedAndInternal,
            Microsoft.CodeAnalysis.Accessibility.Protected => Accessibility.Protected,
            Microsoft.CodeAnalysis.Accessibility.Internal => Accessibility.Internal,
            Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal => Accessibility.ProtectedOrInternal,
            Microsoft.CodeAnalysis.Accessibility.Public => Accessibility.Public,
            _ => throw new ArgumentOutOfRangeException()
        };

        public string Name => this.Symbol.Name;

        INamedType IMember.DeclaringType => this.DeclaringType;
        
        IMember ICodeElementLink<IMember>.GetForCompilation( CompilationModel compilation ) => this.GetForCompilation<IMember>(compilation);
    }
}