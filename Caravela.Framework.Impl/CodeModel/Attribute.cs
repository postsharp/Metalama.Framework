// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using TypedConstant = Caravela.Framework.Code.TypedConstant;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Attribute : IAttribute, IHasDiagnosticLocation
    {
        private readonly AttributeData _data;
        private readonly CompilationModel _compilation;

        public Attribute( AttributeData data, CompilationModel compilation, ICodeElement containingElement )
        {
            this._data = data;
            this._compilation = compilation;
            this.ContainingElement = containingElement;
        }

        CodeOrigin ICodeElement.Origin => CodeOrigin.Source;

        public ICodeElement ContainingElement { get; }

        IAttributeList ICodeElement.Attributes => AttributeList.Empty;

        public CodeElementKind ElementKind => CodeElementKind.Attribute;

        public ICompilation Compilation => this.Constructor.Compilation;

        [Memo]
        public INamedType Type => this._compilation.Factory.GetNamedType( this._data.AttributeClass.AssertNotNull() );

        [Memo]
        public IConstructor Constructor => this._compilation.Factory.GetConstructor( this._data.AttributeConstructor.AssertNotNull() );

        [Memo]
        public IReadOnlyList<TypedConstant> ConstructorArguments => this._data.ConstructorArguments.Select( this.Translate ).ToImmutableArray();

        [Memo]
        public INamedArgumentList NamedArguments =>
            new NamedArgumentsList(
                this._data.NamedArguments
                    .Select( kvp => new KeyValuePair<string, TypedConstant>( kvp.Key, this.Translate( kvp.Value ) ) ) );

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

        public bool Equals( ICodeElement other ) => throw new NotImplementedException();

        public override string ToString() => this._data.ToString();

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        ICodeElement? ICodeElement.ContainingElement => this.ContainingElement;

        IDiagnosticLocation? IDiagnosticScope.DiagnosticLocation => this.DiagnosticLocation.ToDiagnosticLocation();

        public Location? DiagnosticLocation => DiagnosticLocationHelper.GetDiagnosticLocation( this._data );
    }
}
