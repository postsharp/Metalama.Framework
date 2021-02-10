using System;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using MethodKind = Caravela.Framework.Code.MethodKind;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class NamedType : CodeElement, ITypeInternal, INamedType
    {
        public abstract string Name { get; }

        public abstract string? Namespace { get; }

        public abstract string FullName { get; }

        public abstract TypeKind TypeKind { get; }

        public abstract bool IsAbstract { get; }
        
        public abstract bool IsSealed { get; }

        INamedType? INamedType.BaseType => this.BaseType;

        public abstract NamedType? BaseType { get; }

        [Memo]
        public bool HasDefaultConstructor =>
            this.TypeKind== TypeKind.Struct ||
            (this.TypeKind == TypeKind.Class && !this.IsAbstract && this.InstanceConstructors.Any( ctor => ctor.Parameters.Count == 0 ));

        public abstract IReadOnlyList<Member> Members { get; }

        IReadOnlyList<INamedType> INamedType.NestedTypes => this.NestedTypes;

        public abstract IReadOnlyList<NamedType> NestedTypes { get; }

        IReadOnlyList<IProperty> INamedType.Properties => this.Properties;

        [Memo]
        public IReadOnlyList<Property> Properties => this.Members.OfType<Property>().ToImmutableArray();

        IReadOnlyList<IEvent> INamedType.Events => this.Events;

        [Memo] public IReadOnlyList<Event> Events => this.Members.OfType<Event>().ToImmutableArray();

        IReadOnlyList<IMethod> INamedType.Methods => this.Methods;

        [Memo]
        public IImmutableList<Method> Methods
            => this.Members.OfType<Method>()
                .Where( m => m.MethodKind != MethodKind.Constructor && m.MethodKind != MethodKind.StaticConstructor )
                .ToImmutableList();

        IReadOnlyList<IMethod> INamedType.InstanceConstructors => this.InstanceConstructors;

        [Memo]
        public IImmutableList<Method> InstanceConstructors
            => this.Members.OfType<Method>()
                .Where( m => m.MethodKind == MethodKind.Constructor )
                .ToImmutableList();

        [Memo]
        public IMethod StaticConstructor => this.Members.OfType<IMethod>()
            .Where( m => m.MethodKind == MethodKind.StaticConstructor )
            .SingleOrDefault();

        IReadOnlyList<IType> INamedType.GenericArguments => this.GenericArguments;

        public abstract IReadOnlyList<ITypeInternal> GenericArguments { get; }

        IReadOnlyList<IGenericParameter> INamedType.GenericParameters => this.GenericParameters;

        public abstract IReadOnlyList<GenericParameter> GenericParameters { get; }

        IReadOnlyList<INamedType> INamedType.ImplementedInterfaces => this.ImplementedInterfaces;

        public abstract IReadOnlyList<NamedType> ImplementedInterfaces { get; }

        public override CodeElementKind ElementKind => CodeElementKind.Type;

        public abstract bool Is( IType other );

        public abstract bool Is( Type other );

        public abstract IArrayType MakeArrayType( int rank = 1 );

        public abstract IPointerType MakePointerType();

        public abstract INamedType MakeGenericType( params IType[] genericArguments );
    }
}
