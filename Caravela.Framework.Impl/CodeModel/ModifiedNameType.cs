// unset

using Caravela.Framework.Code;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    
    
    internal sealed class ModifiedNamedType : NamedType
    {
        private NamedType _previousLayerType;
        private CompilationModel _compilation;
        
        public override IImmutableList<IMemberInternal> Members => 
            this._previousLayerType.Members.
                Union( this._compilation.IntroductionsByContainingElement[this].OfType<IMemberInternal>() )
                .ToImmutableList();

        public override bool IsAbstract => this._previousLayerType.IsAbstract;

        public override bool IsSealed => this._previousLayerType.IsSealed;

        public override NamedType? BaseType => this._previousLayerType.BaseType;

        public override IImmutableList<NamedType> ImplementedInterfaces => this._previousLayerType.ImplementedInterfaces;

        public override string Name => this._previousLayerType.Name;

        public override string? Namespace => throw new NotImplementedException();

        public override string FullName => throw new NotImplementedException();

        public override TypeKind TypeKind => throw new NotImplementedException();

        public override bool Is( IType other ) => throw new NotImplementedException();

        public override bool Is( Type other ) => throw new NotImplementedException();

        public override IArrayType MakeArrayType( int rank = 1 ) => throw new NotImplementedException();

        public override IPointerType MakePointerType() => throw new NotImplementedException();

        public override INamedType MakeGenericType( params IType[] genericArguments ) => throw new NotImplementedException();

        public override IReadOnlyList<IType> GenericArguments => throw new NotImplementedException();

        public override IImmutableList<GenericParameter> GenericParameters => throw new NotImplementedException();

        public override IImmutableList<NamedType> NestedTypes => throw new NotImplementedException();

    

        public override CodeElement? ContainingElement => throw new NotImplementedException();

        public override IImmutableList<Attribute> Attributes => throw new NotImplementedException();

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();
    }
}