// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TypedConstant = Caravela.Framework.Code.TypedConstant;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Attribute : IAttributeImpl
    {
        private readonly CompilationModel _compilation;

        public Attribute( AttributeData data, CompilationModel compilation, IDeclaration containingDeclaration )
        {
            this.AttributeData = data;
            this._compilation = compilation;
            this.ContainingDeclaration = containingDeclaration;
        }

        public AttributeData AttributeData { get; }

        IRef<IDeclaration> IDeclaration.ToRef() => new AttributeRef( this.AttributeData, ((IDeclarationImpl) this.ContainingDeclaration).ToRef() );

        public IAssembly DeclaringAssembly => this.ContainingDeclaration.DeclaringAssembly;

        DeclarationOrigin IDeclaration.Origin => DeclarationOrigin.Source;

        public IDeclaration ContainingDeclaration { get; }

        IAttributeList IDeclaration.Attributes => AttributeList.Empty;

        public DeclarationKind DeclarationKind => DeclarationKind.Attribute;

        public ICompilation Compilation => this.Constructor.Compilation;

        [Memo]
        public INamedType Type => this._compilation.Factory.GetNamedType( this.AttributeData.AttributeClass.AssertNotNull() );

        [Memo]
        public IConstructor Constructor => this._compilation.Factory.GetConstructor( this.AttributeData.AttributeConstructor.AssertNotNull() );

        [Memo]
        public ImmutableArray<TypedConstant> ConstructorArguments => this.AttributeData.ConstructorArguments.Select( this.Translate ).ToImmutableArray();

        [Memo]
        public ImmutableArray<KeyValuePair<string, TypedConstant>> NamedArguments
            => this.AttributeData.NamedArguments
                .Select( kvp => new KeyValuePair<string, TypedConstant>( kvp.Key, this.Translate( kvp.Value ) ) )
                .ToImmutableArray();

        private TypedConstant Translate( Microsoft.CodeAnalysis.TypedConstant constant )
        {
            var type = this._compilation.Factory.GetIType( constant.Type.AssertNotNull() );

            var value = constant.Kind switch
            {
                TypedConstantKind.Primitive or TypedConstantKind.Enum => constant.Value,
                TypedConstantKind.Type => constant.Value == null ? null : this._compilation.Factory.GetIType( (ITypeSymbol) constant.Value ),
                TypedConstantKind.Array => constant.Values.Select( this.Translate ).ToImmutableArray(),
                _ => throw new ArgumentException( nameof( constant ) )
            };

            return new TypedConstant( type, value );
        }

        public override string ToString() => this.AttributeData.ToString();

        public FormattableString FormatPredecessor( ICompilation compilation ) => $"the attribute of type '{this.Type}' on '{this.ContainingDeclaration}'";

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        IDeclaration? IDeclaration.ContainingDeclaration => this.ContainingDeclaration;

        Location? IAspectPredecessorImpl.GetDiagnosticLocation( Compilation compilation ) => this.DiagnosticLocation;

        IType IHasType.Type => this.Type;

        public Location? DiagnosticLocation => this.AttributeData.GetDiagnosticLocation();
    }
}