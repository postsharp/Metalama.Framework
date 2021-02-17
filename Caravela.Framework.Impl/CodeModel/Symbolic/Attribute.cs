using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
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

        public ICodeElement ContainingElement { get; }

        IReadOnlyList<IAttribute> ICodeElement.Attributes => Array.Empty<IAttribute>();

        public CodeElementKind ElementKind => CodeElementKind.Attribute;

        public ICompilation Compilation => this.Constructor.Compilation;

        [Memo]
        public INamedType Type => this._compilation.Factory.GetNamedType( this._data.AttributeClass! );

        [Memo]
        public IMethod Constructor => this._compilation.Factory.GetMethod( this._data.AttributeConstructor! );

        [Memo]
        public IReadOnlyList<object?> ConstructorArguments => this._data.ConstructorArguments.Select( this.Translate ).ToImmutableArray();

        [Memo]
        public IReadOnlyList<KeyValuePair<string, object?>> NamedArguments =>
            this._data.NamedArguments
                .Select( kvp => new KeyValuePair<string, object?>( kvp.Key, this.Translate( kvp.Value ) ) )
                .ToImmutableArray();

        private object? Translate( TypedConstant constant ) =>
            constant.Kind switch
            {
                TypedConstantKind.Primitive or TypedConstantKind.Enum => constant.Value,
                TypedConstantKind.Type => constant.Value == null ? null : this._compilation.Factory.GetIType( (ITypeSymbol) constant.Value ),
                TypedConstantKind.Array => constant.Values.Select( this.Translate ).ToImmutableArray(),
                _ => throw new ArgumentException( nameof( constant ) )
            };

        public bool Equals( ICodeElement other ) => throw new NotImplementedException();

        public override string ToString() => this._data.ToString();

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        ICodeElement? ICodeElement.ContainingElement => this.ContainingElement;

        IDiagnosticLocation? IDiagnosticTarget.DiagnosticLocation => this.DiagnosticLocation.ToDiagnosticLocation();

        public Location? DiagnosticLocation => DiagnosticLocationHelper.GetDiagnosticLocation( this._data );
    }
}
