// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Accessibility = Microsoft.CodeAnalysis.Accessibility;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal class ExternalTypeCollection : INamedTypeCollection
{
    private readonly IAssemblySymbol _symbol;
    private readonly CompilationModel _compilation;
    private List<INamedTypeSymbol>? _types;

    public ExternalTypeCollection( IAssemblySymbol symbol, CompilationModel compilation )
    {
        this._symbol = symbol;
        this._compilation = compilation;
    }

    private List<INamedTypeSymbol> GetContent()
    {
        this._types ??= this._symbol.GetTypes().Where( t => !IsHidden( t ) ).ToList();

        return this._types;
    }

    private static bool IsHidden( INamedTypeSymbol type ) => type.DeclaredAccessibility == Accessibility.Private;

    public IEnumerable<INamedType> OfName( string name )
        => this.GetContent().Where( t => t.Name == name ).Select( x => this._compilation.Factory.GetNamedType( x ) );

    public IEnumerator<INamedType> GetEnumerator()
    {
        foreach ( var type in this.GetContent() )
        {
            yield return this._compilation.Factory.GetNamedType( type );
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public int Count => this.GetContent().Count;

    public IReadOnlyList<INamedType> DerivedFrom( Type type ) => throw new NotImplementedException();

    public IReadOnlyList<INamedType> DerivedFrom( INamedType type ) => throw new NotImplementedException();
}