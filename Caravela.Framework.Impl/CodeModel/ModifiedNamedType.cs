using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel
{

    internal sealed class ModifiedNamedType : NamedType
    {
        private NamedType _prototype;
        private CompilationModel _compilation;
        
        [Memo]
        public override IReadOnlyList<Member> Members =>
            this._prototype.Members.
                Union( this._compilation.IntroductionsByContainingElement[this].OfType<Member>() )
                .ToImmutableArray();

        public override bool IsAbstract => this._prototype.IsAbstract;

        public override bool IsSealed => this._prototype.IsSealed;

        public override NamedType? BaseType => this._prototype.BaseType;

        public override IReadOnlyList<NamedType> ImplementedInterfaces => this._prototype.ImplementedInterfaces;

        public override string Name => this._prototype.Name;

        public override string? Namespace => this._prototype.Namespace;

        public override string FullName => this._prototype.FullName;

        public override TypeKind TypeKind => this._prototype.TypeKind;

        public override IReadOnlyList<ITypeInternal> GenericArguments => this._prototype.GenericArguments;

        public override IReadOnlyList<GenericParameter> GenericParameters => this._prototype.GenericParameters;

        public override IReadOnlyList<NamedType> NestedTypes => this._prototype.NestedTypes;

        public override CodeElement? ContainingElement => this._prototype.ContainingElement;

        public override IReadOnlyList<Attribute> Attributes => this._prototype.Attributes;

        public override bool Is( IType other ) => this._prototype.Is(other);

        public override bool Is( Type other ) => this._prototype.Is( other );

        public override IArrayType MakeArrayType( int rank = 1 ) => this._prototype.MakeArrayType( rank );

        public override IPointerType MakePointerType() => this._prototype.MakePointerType();

        public override INamedType MakeGenericType( params IType[] genericArguments ) => this._prototype.MakeGenericType( genericArguments );

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();
        public override bool Equals( ICodeElement other ) => this._prototype.Equals( other );
    }
}