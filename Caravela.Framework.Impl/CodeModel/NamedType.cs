using System;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using MethodKind = Caravela.Framework.Code.MethodKind;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal interface ITypeInternal : IType
    {
        
    }
    
    
    internal abstract class NamedType : CodeElement,  ITypeInternal, INamedType
    {
 
        
        
        INamedType? INamedType.BaseType => this.BaseType;

        IReadOnlyList<INamedType> INamedType.ImplementedInterfaces => this.ImplementedInterfaces;

        IReadOnlyList<IGenericParameter> INamedType.GenericParameters => this.GenericParameters;

        IReadOnlyList<INamedType> INamedType.NestedTypes => this.NestedTypes;

        IReadOnlyList<IProperty> INamedType.Properties => this.Properties;

        IReadOnlyList<IEvent> INamedType.Events => this.Events;

        IReadOnlyList<IMethod> INamedType.Methods => this.Methods;

        IReadOnlyList<IMethod> INamedType.InstanceConstructors => this.InstanceConstructors;

        public abstract bool IsAbstract { get; }
        public abstract bool IsSealed { get; }

        public bool HasDefaultConstructor =>
            this.TypeKind== TypeKind.Struct ||
            (this.TypeKind == TypeKind.Class && !this.IsAbstract && this.InstanceConstructors.Any( ctor => ctor.Parameters.Count == 0 ));

        [Memo]
        public abstract IImmutableList<NamedType> NestedTypes { get; }

        [Memo]
        public IImmutableList<Property> Properties => this.Members.OfType<Property>().ToImmutableList();
            


        public abstract IImmutableList<IMemberInternal> Members { get; }


        [Memo] public IImmutableList<Event> Events => this.Members.OfType<Event>().ToImmutableList();

        [Memo]
        public IImmutableList<Method> Methods
            => this.Members.OfType<Method>()
                .Where( m => m.MethodKind != MethodKind.Constructor && m.MethodKind != MethodKind.StaticConstructor )
                .ToImmutableList();

        [Memo]
        public IImmutableList<Method> InstanceConstructors
            => this.Members.OfType<Method>()
                .Where( m => m.MethodKind == MethodKind.Constructor )
                .ToImmutableList();

        [Memo]
        public IMethod StaticConstructor => this.Members.OfType<IMethod>()
            .Where( m => m.MethodKind == MethodKind.StaticConstructor )
            .SingleOrDefault();


        public abstract IReadOnlyList<IType> GenericArguments { get; }
        public abstract IImmutableList<GenericParameter> GenericParameters { get; }

        

     
        public override CodeElementKind ElementKind => CodeElementKind.Type;

        public abstract NamedType? BaseType { get; }

        public abstract IImmutableList<NamedType> ImplementedInterfaces { get; }

        public abstract string Name { get; }
        public abstract string? Namespace { get; }
        public abstract string FullName { get; }

        public abstract TypeKind TypeKind { get; }

        public abstract bool Is( IType other );

        public abstract bool Is( Type other );

        public abstract IArrayType MakeArrayType( int rank = 1 );

        public abstract  IPointerType MakePointerType();

        public abstract  INamedType MakeGenericType( params IType[] genericArguments );

    }
}
