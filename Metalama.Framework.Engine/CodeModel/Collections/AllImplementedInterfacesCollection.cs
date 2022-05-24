// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal class AllImplementedInterfacesCollection : IImplementedInterfaceCollection
{
    private readonly HashSet<INamedTypeSymbol> _implementedInterfacesSymbols = new( SymbolEqualityComparer.Default );
    private readonly CompilationModel _compilation;
    private List<INamedType>? _implementedInterfaces;
    
    public AllImplementedInterfacesCollection( INamedType type )
    {
        var compilation = this._compilation = type.GetCompilationModel();

        void ProcessInterface( INamedTypeSymbol interfaceType )
        {
            if ( this._implementedInterfacesSymbols.Add( interfaceType ) )
            {
                foreach ( var child in interfaceType.AllInterfaces )
                {
                    ProcessInterface( child );
                }
            }
        }

        for ( var t = type.GetSymbol(); t != null; t = t.BaseType )
        {
            var implementedInterfaces = compilation.GetInterfaceImplementationCollection( t, false );

            foreach ( var i in implementedInterfaces )
            {
                ProcessInterface( (INamedTypeSymbol) i.GetSymbol( compilation.RoslynCompilation ) );
            }
        }
    }

    public bool Contains( Type type ) => this._implementedInterfacesSymbols.Contains( this._compilation.ReflectionMapper.GetTypeSymbol( type ) );

    public IEnumerator<INamedType> GetEnumerator()
    {
        this._implementedInterfaces ??= this._implementedInterfacesSymbols.Select( x => this._compilation.Factory.GetNamedType( x ) ).ToList();

        return this._implementedInterfaces.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public int Count => this._implementedInterfacesSymbols.Count;
}