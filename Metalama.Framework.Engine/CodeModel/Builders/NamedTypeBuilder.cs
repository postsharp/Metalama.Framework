// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class NamedTypeBuilder : MemberOrNamedTypeBuilder, ITypeBuilder
    {
        public NamedTypeBuilder( Advice advice, INamespace declaringNamespace, string name) : base(advice, null, name)
        {
            this.Namespace = declaringNamespace;
        }
        public NamedTypeBuilder( Advice advice, INamedType declaringType, string name ) : base( advice, declaringType, name )
        {
            this.Namespace = this.DeclaringType!.Namespace;
        }

        public override IDeclaration ContainingDeclaration => (IDeclaration?)this.DeclaringType ?? this.Namespace;

        public bool IsPartial => false;

        public bool HasDefaultConstructor => true;

        [Memo]
        public INamedType? BaseType => ((CompilationModel) this.Namespace.Compilation).Factory.GetSpecialType( SpecialType.Object );

        INamedType? INamedType.BaseType => this.BaseType;

        [Memo]
        public IImplementedInterfaceCollection AllImplementedInterfaces => new EmptyImplementedInterfaceCollection();

        [Memo]
        public IImplementedInterfaceCollection ImplementedInterfaces => new EmptyImplementedInterfaceCollection();

        public INamespace Namespace { get; }

        INamespace INamedType.Namespace => this.Namespace;

        public string FullName => $"{this.Namespace.FullName}.{this.Name}";

        [Memo]
        public INamedTypeCollection NestedTypes => new EmptyNamedTypeCollection();

        [Memo]
        public IPropertyCollection Properties => new EmptyPropertyCollection( this );

        [Memo]
        public IPropertyCollection AllProperties => new EmptyPropertyCollection( this );

        [Memo]
        public IIndexerCollection Indexers => new EmptyIndexerCollection( this );

        [Memo]
        public IIndexerCollection AllIndexers => new EmptyIndexerCollection( this );

        [Memo]
        public IFieldCollection Fields => new EmptyFieldCollection( this );

        [Memo]
        public IFieldCollection AllFields => new EmptyFieldCollection( this );

        [Memo]
        public IFieldOrPropertyCollection FieldsAndProperties => new EmptyFieldOrPropertyCollection( this );

        [Memo]
        public IFieldOrPropertyCollection AllFieldsAndProperties => new EmptyFieldOrPropertyCollection( this );

        [Memo]
        public IEventCollection Events => new EmptyEventCollection( this );

        [Memo]
        public IEventCollection AllEvents => new EmptyEventCollection( this );

        [Memo]
        public IMethodCollection Methods => new EmptyMethodCollection( this );

        [Memo]
        public IMethodCollection AllMethods => new EmptyMethodCollection( this );

        public IConstructor? PrimaryConstructor => null;

        [Memo]
        public IConstructorCollection Constructors => new EmptyConstructorCollection( this );

        public IConstructor? StaticConstructor => null;

        public IMethod? Finalizer => null;

        public bool IsReadOnly => false;

        public bool IsRef => false;

        public INamedType TypeDefinition => this;

        public INamedType Definition => this;

        public INamedType UnderlyingType => this;

        public TypeKind TypeKind => TypeKind.Class;

        public SpecialType SpecialType => SpecialType.None;

        public bool? IsReferenceType => true;

        public bool? IsNullable => false;

        [Memo]
        public IGenericParameterList TypeParameters => new EmptyGenericParameterList(this);

        [Memo]
        public IReadOnlyList<IType> TypeArguments => Array.Empty<IType>();

        public bool IsGeneric => false;

        public bool IsCanonicalGenericInstance => false;

        Accessibility IMemberOrNamedType.Accessibility => this.Accessibility;

        [Memo]
        public IAttributeCollection Attributes => new AttributeBuilderCollection();

        public int Depth => this.Namespace.Depth + 1;

        public bool BelongsToCurrentProject => true;

        public ImmutableArray<SourceReference> Sources => ImmutableArray<SourceReference>.Empty;

        IMemberOrNamedType IMemberOrNamedType.Definition => this;

        public override DeclarationKind DeclarationKind => DeclarationKind.NamedType;

        public override bool CanBeInherited => false;

        public bool Equals( SpecialType specialType ) => false;

        public bool Equals( IType? otherType, TypeComparison typeComparison ) => this.Compilation.Comparers.GetTypeComparer( typeComparison ).Equals( this, otherType );

        public bool Equals( IType? other ) => this.Compilation.Comparers.Default.Equals( this, other );

        public bool Equals( INamedType? other ) => this.Compilation.Comparers.Default.Equals( this, other );

        public bool IsSubclassOf( INamedType type ) => false;

        public Type ToType()
        {
            throw new NotImplementedException();
        }

        public bool TryFindImplementationForInterfaceMember( IMember interfaceMember, [NotNullWhen( true )] out IMember? implementationMember )
        {
            implementationMember = null;
            return false;
        }

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            return this.FullName;
        }

        public IntroduceTypeTransformation ToTransformation()
        {
            return new IntroduceTypeTransformation( this.ParentAdvice, this );
        }
    }
}