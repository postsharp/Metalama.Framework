﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#pragma warning disable IDE0073 // The file header does not match the required text

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using EnumerableExtensions = Metalama.Framework.Engine.Collections.EnumerableExtensions;
using RefKind = Metalama.Framework.Code.RefKind;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
    /// <summary>
    /// APIs for constructing documentation comment ids, and finding declarations that match ids.
    /// </summary>
    internal static class DeclarationDocumentationCommentId
    {
        /// <summary>
        /// Creates an id string used by external documentation comment files to identify declarations
        /// of types, namespaces, methods, properties, etc.
        /// </summary>
        public static string CreateDeclarationId( IDeclaration declaration )
        {
            if ( declaration == null )
            {
                throw new ArgumentNullException( nameof(declaration) );
            }

            using var builder = StringBuilderPool.Default.Allocate();

            var generator = new DeclarationGenerator( builder.Value );
            generator.Visit( declaration );

            return builder.Value.ToString();
        }

        /// <summary>
        /// Creates an id string used to reference type symbols (not strictly declarations, includes
        /// arrays, pointers, type parameters, etc.)
        /// </summary>
        public static string CreateReferenceId( IType type )
        {
            if ( type == null )
            {
                throw new ArgumentNullException( nameof(type) );
            }

            var builder = new StringBuilder();
            var generator = new ReferenceGenerator( builder, typeParameterContext: null );
            generator.Visit( type );

            return builder.ToString();
        }

        /// <summary>
        /// Creates an id string used to reference type symbols (not strictly declarations, includes
        /// arrays, pointers, type parameters, etc.)
        /// </summary>
        public static string CreateReferenceId( INamespace ns )
        {
            if ( ns == null )
            {
                throw new ArgumentNullException( nameof(ns) );
            }

            var builder = new StringBuilder();
            var generator = new ReferenceGenerator( builder, typeParameterContext: null );
            generator.Visit( ns );

            return builder.ToString();
        }

        /// <summary>
        /// Gets the first declaration that matches the declaration id string, order undefined.
        /// </summary>
        public static IDeclaration? GetFirstDeclarationForDeclarationId( string id, CompilationModel compilation )
        {
            if ( id == null )
            {
                throw new ArgumentNullException( nameof(id) );
            }

            if ( compilation == null )
            {
                throw new ArgumentNullException( nameof(compilation) );
            }

            var results = new List<IDeclaration>();

            Parser.ParseDeclaredSymbolId( id, compilation, results );

            return results.Count == 0 ? null : results[0];
        }

        private static int GetTotalTypeParameterCount( INamedType? namedType )
        {
            var n = 0;

            while ( namedType != null )
            {
                n += namedType.TypeParameters.Count;
                namedType = namedType.ContainingDeclaration as INamedType;
            }

            return n;
        }

        // encodes dots with alternate # character
        private static string EncodeName( string name ) => name.Replace( '.', '#' );

        private static string EncodePropertyName( string name )
        {
            // convert C# indexer names to 'Item'
            if ( name == "this[]" )
            {
                name = "Item";
            }
            else if ( name.EndsWith( ".this[]", StringComparison.Ordinal ) )
            {
                name = name.Substring( 0, name.Length - 6 ) + "Item";
            }

            return name;
        }

        private static string DecodePropertyName( string name )
        {
            // special case, csharp names indexers 'this[]', not 'Item'
            if ( name == "Item" )
            {
                name = "this[]";
            }
            else if ( name.EndsWith( ".Item", StringComparison.Ordinal ) )
            {
                name = name.Substring( 0, name.Length - 4 ) + "this[]";
            }

            return name;
        }

        private sealed class DeclarationGenerator
        {
            private readonly StringBuilder _builder;
            private readonly Generator _generator;

            public DeclarationGenerator( StringBuilder builder )
            {
                this._builder = builder;
                this._generator = new Generator( builder );
            }

            public void Visit( IDeclaration declaration )
            {
                switch ( declaration )
                {
                    case IEvent @event:
                        this._builder.Append( "E:" );
                        this._generator.Visit( @event );

                        break;

                    case IField field:
                        this._builder.Append( "F:" );
                        this._generator.Visit( field );

                        break;

                    case IPropertyOrIndexer property:
                        this._builder.Append( "P:" );
                        this._generator.Visit( property );

                        break;

                    case IMethodBase method:
                        this._builder.Append( "M:" );
                        this._generator.Visit( method );

                        break;

                    case INamespace ns:
                        this._builder.Append( "N:" );
                        this._generator.Visit( ns );

                        break;

                    case INamedType namedType:
                        this._builder.Append( "T:" );
                        this._generator.Visit( namedType );

                        break;

                    default:
                        throw new InvalidOperationException( $"Cannot generate a documentation comment id for symbol '{declaration}'." );
                }
            }

            private sealed class Generator
            {
                private readonly StringBuilder _builder;
                private ReferenceGenerator? _referenceGenerator;

                public Generator( StringBuilder builder )
                {
                    this._builder = builder;
                }

                private ReferenceGenerator GetReferenceGenerator( IDeclaration typeParameterContext )
                {
                    if ( this._referenceGenerator == null || !ReferenceEquals( this._referenceGenerator.TypeParameterContext, typeParameterContext ) )
                    {
                        this._referenceGenerator = new ReferenceGenerator( this._builder, typeParameterContext );
                    }

                    return this._referenceGenerator;
                }

                public void Visit( IEvent @event )
                {
                    if ( this.Visit( @event.DeclaringType ) )
                    {
                        this._builder.Append( '.' );
                    }

                    this._builder.Append( EncodeName( @event.Name ) );
                }

                public void Visit( IField field )
                {
                    if ( this.Visit( field.DeclaringType ) )
                    {
                        this._builder.Append( '.' );
                    }

                    this._builder.Append( EncodeName( field.Name ) );
                }

                public void Visit( IPropertyOrIndexer propertyOrIndexer )
                {
                    if ( this.Visit( propertyOrIndexer.DeclaringType ) )
                    {
                        this._builder.Append( '.' );
                    }

                    var name = EncodePropertyName( propertyOrIndexer.Name );
                    this._builder.Append( EncodeName( name ) );

                    if ( propertyOrIndexer is IIndexer indexer )
                    {
                        this.AppendParameters( indexer.Parameters );
                    }
                }

                public void Visit( IMethodBase methodBase )
                {
                    if ( this.Visit( methodBase.DeclaringType ) )
                    {
                        this._builder.Append( '.' );
                    }

                    this._builder.Append( EncodeName( methodBase.Name ) );

                    if ( methodBase is IMethod { TypeParameters.Count: > 0 } method )
                    {
                        this._builder.Append( "``" );
                        this._builder.Append( method.TypeParameters.Count );
                    }

                    this.AppendParameters( methodBase.Parameters );

                    if ( methodBase is IMethod method2 && !method2.ReturnType.Equals( SpecialType.Void ) )
                    {
                        this._builder.Append( '~' );
                        this.GetReferenceGenerator( method2 ).Visit( method2.ReturnType );
                    }
                }

                private void AppendParameters( IParameterList parameters )
                {
                    if ( parameters.Count > 0 )
                    {
                        this._builder.Append( '(' );

                        for ( int i = 0, n = parameters.Count; i < n; i++ )
                        {
                            if ( i > 0 )
                            {
                                this._builder.Append( ',' );
                            }

                            var p = parameters[i];
                            this.GetReferenceGenerator( p.DeclaringMember ).Visit( p.Type );

                            if ( p.RefKind != RefKind.None )
                            {
                                this._builder.Append( '@' );
                            }
                        }

                        this._builder.Append( ')' );
                    }
                }

                public bool Visit( INamespace ns )
                {
                    if ( ns.IsGlobalNamespace )
                    {
                        return false;
                    }

                    if ( this.Visit( ns.ParentNamespace! ) )
                    {
                        this._builder.Append( '.' );
                    }

                    this._builder.Append( EncodeName( ns.Name ) );

                    return true;
                }

                public bool Visit( INamedType namedType )
                {
                    var success = namedType.ContainingDeclaration is INamedType containingType
                        ? this.Visit( containingType )
                        : this.Visit( namedType.Namespace );

                    if ( success )
                    {
                        this._builder.Append( '.' );
                    }

                    this._builder.Append( EncodeName( namedType.Name ) );

                    if ( namedType.TypeParameters.Count > 0 )
                    {
                        this._builder.Append( '`' );
                        this._builder.Append( namedType.TypeParameters.Count );
                    }

                    return true;
                }
            }
        }

        private sealed class ReferenceGenerator
        {
            private readonly StringBuilder _builder;

            public ReferenceGenerator( StringBuilder builder, IDeclaration? typeParameterContext )
            {
                this._builder = builder;
                this.TypeParameterContext = typeParameterContext;
            }

            public IDeclaration? TypeParameterContext { get; }

            private void BuildDottedName( INamedType namedType )
            {
                var success = namedType.ContainingDeclaration is INamedType containingType
                    ? this.Visit( containingType )
                    : this.Visit( namedType.Namespace );

                if ( success )
                {
                    this._builder.Append( '.' );
                }

                this._builder.Append( EncodeName( namedType.Name ) );
            }

            private void BuildDottedName( INamespace ns )
            {
                if ( this.Visit( ns.ParentNamespace.AssertNotNull() ) )
                {
                    this._builder.Append( '.' );
                }

                this._builder.Append( EncodeName( ns.Name ) );
            }

            public bool Visit( INamespace ns )
            {
                if ( ns.IsGlobalNamespace )
                {
                    return false;
                }

                this.BuildDottedName( ns );

                return true;
            }

            public void Visit( IType type )
            {
                switch ( type )
                {
                    case INamedType namedType:
                        this.Visit( namedType );

                        return;

                    case IDynamicType dynamicType:
                        this.Visit( dynamicType );

                        return;

                    case IArrayType arrayType:
                        this.Visit( arrayType );

                        return;

                    case IPointerType pointerType:
                        this.Visit( pointerType );

                        return;

                    case ITypeParameter parameter:
                        this.Visit( parameter );

                        return;

                    default:
                        throw new NotSupportedException( $"The type '{type}' is not supported" );
                }
            }

            private bool Visit( INamedType namedType )
            {
                this.BuildDottedName( namedType );

                if ( namedType.IsGeneric )
                {
                    if ( namedType.IsCanonicalGenericInstance )
                    {
                        this._builder.Append( '`' );
                        this._builder.Append( namedType.TypeParameters.Count );
                    }
                    else if ( namedType.TypeArguments.Count > 0 )
                    {
                        this._builder.Append( '{' );

                        for ( int i = 0, n = namedType.TypeArguments.Count; i < n; i++ )
                        {
                            if ( i > 0 )
                            {
                                this._builder.Append( ',' );
                            }

                            this.Visit( namedType.TypeArguments[i] );
                        }

                        this._builder.Append( '}' );
                    }
                }

                return true;
            }

            private void Visit( IDynamicType dynamicType )
            {
                _ = dynamicType;

                this._builder.Append( "System.Object" );
            }

            private void Visit( IArrayType arrayType )
            {
                this.Visit( arrayType.ElementType );

                this._builder.Append( '[' );

                for ( int i = 0, n = arrayType.Rank; i < n; i++ )
                {
                    if ( i > 0 )
                    {
                        this._builder.Append( ',' );
                    }
                }

                this._builder.Append( ']' );
            }

            private void Visit( IPointerType pointerType )
            {
                this.Visit( pointerType.PointedAtType );
                this._builder.Append( '*' );
            }

            private void Visit( ITypeParameter typeParameter )
            {
                if ( !this.IsInScope( typeParameter ) )
                {
                    // reference to type parameter not in scope, make explicit scope reference
                    var declarer = new DeclarationGenerator( this._builder );
                    declarer.Visit( typeParameter.ContainingDeclaration.AssertNotNull() );
                    this._builder.Append( ':' );
                }

                if ( typeParameter.ContainingDeclaration is IMethod )
                {
                    this._builder.Append( "``" );
                    this._builder.Append( typeParameter.Index );
                }
                else
                {
                    // get count of all type parameter preceding the declaration of the type parameters containing symbol.
                    var container = typeParameter.ContainingDeclaration?.ContainingDeclaration;
                    var b = GetTotalTypeParameterCount( container as INamedType );
                    this._builder.Append( '`' );
                    this._builder.Append( b + typeParameter.Index );
                }
            }

            private bool IsInScope( ITypeParameter typeParameter )
            {
                // determine if the type parameter is declared in scope defined by the typeParameterContext symbol
                var typeParameterDeclarer = typeParameter.ContainingDeclaration;

                for ( var scope = this.TypeParameterContext; scope != null; scope = scope.ContainingDeclaration )
                {
                    if ( scope.Equals( typeParameterDeclarer ) )
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private static class Parser
        {
            public static void ParseDeclaredSymbolId( string? id, CompilationModel compilation, List<IDeclaration> results )
            {
                if ( id == null )
                {
                    return;
                }

                if ( id.Length < 2 )
                {
                    return;
                }

                var index = 0;
                results.Clear();
                ParseDeclaredId( id, ref index, compilation, results );
            }

            private static void ParseDeclaredId( string id, ref int index, CompilationModel compilation, List<IDeclaration> results )
            {
                var kindChar = PeekNextChar( id, index );
                SymbolKind kind;

                switch ( kindChar )
                {
                    case 'E':
                        kind = SymbolKind.Event;

                        break;

                    case 'F':
                        kind = SymbolKind.Field;

                        break;

                    case 'M':
                        kind = SymbolKind.Method;

                        break;

                    case 'N':
                        kind = SymbolKind.Namespace;

                        break;

                    case 'P':
                        kind = SymbolKind.Property;

                        break;

                    case 'T':
                        kind = SymbolKind.NamedType;

                        break;

                    default:
                        // Documentation comment id must start with E, F, M, N, P or T
                        return;
                }

                index++;

                if ( PeekNextChar( id, index ) == ':' )
                {
                    index++;
                }

                var containers = new List<IDeclaration> { compilation.GetMergedGlobalNamespace() };

                string name;
                int arity;

                // process dotted names
                while ( true )
                {
                    name = ParseName( id, ref index );
                    arity = 0;

                    // has type parameters?
                    if ( PeekNextChar( id, index ) == '`' )
                    {
                        index++;

                        // method type parameters?
                        if ( PeekNextChar( id, index ) == '`' )
                        {
                            index++;
                        }

                        arity = ReadNextInteger( id, ref index );
                    }

                    if ( PeekNextChar( id, index ) == '.' )
                    {
                        // must be a namespace or type since name continues after dot
                        index++;

                        if ( arity > 0 )
                        {
                            // only types have arity
                            var types = new List<INamedType>();
                            GetMatchingTypes( containers, name, arity, types );
                            results.AddRange( types );
                        }
                        else if ( kind == SymbolKind.Namespace )
                        {
                            // if the results kind is namespace, then all dotted names must be namespaces
                            GetMatchingNamespaces( containers, name, results );
                        }
                        else
                        {
                            // could be either
                            GetMatchingNamespaceOrTypes( containers, name, results );
                        }

                        if ( results.Count == 0 )
                        {
                            // no matches found before dot, cannot continue.
                            return;
                        }

                        // results become the new containers
                        containers.Clear();
                        containers.AddRange( results );
                        results.Clear();
                    }
                    else
                    {
                        // no more dots, so don't loop any more
                        break;
                    }
                }

                switch ( kind )
                {
                    case SymbolKind.Method:
                        GetMatchingMethods( id, ref index, containers, name, arity, compilation, results );

                        break;

                    case SymbolKind.NamedType:
                        var resultTypes = new List<INamedType>();
                        GetMatchingTypes( containers, name, arity, resultTypes );
                        results.AddRange( resultTypes );

                        break;

                    case SymbolKind.Property:
                        GetMatchingProperties( id, ref index, containers, name, compilation, results );

                        break;

                    case SymbolKind.Event:
                        GetMatchingEvents( containers, name, results );

                        break;

                    case SymbolKind.Field:
                        GetMatchingFields( containers, name, results );

                        break;

                    case SymbolKind.Namespace:
                        GetMatchingNamespaces( containers, name, results );

                        break;
                }
            }

            private static IType? ParseTypeSymbol( string id, ref int index, CompilationModel compilation, IDeclaration? typeParameterContext )
            {
                var results = new List<IType>();

                ParseTypeSymbol( id, ref index, compilation, typeParameterContext, results );

                if ( results.Count == 0 )
                {
                    return null;
                }
                else
                {
                    return results[0];
                }
            }

            private static void ParseTypeSymbol(
                string id,
                ref int index,
                CompilationModel compilation,
                IDeclaration? typeParameterContext,
                List<IType> results )
            {
                var ch = PeekNextChar( id, index );

                // context expression embedded in reference => <context-definition>:<type-parameter>
                // note: this is a deviation from the language spec
                if ( ch is 'M' or 'T' && PeekNextChar( id, index + 1 ) == ':' )
                {
                    var contexts = new List<IDeclaration>();

                    ParseDeclaredId( id, ref index, compilation, contexts );

                    if ( contexts.Count == 0 )
                    {
                        // context cannot be bound, so abort
                        return;
                    }

                    if ( PeekNextChar( id, index ) == ':' )
                    {
                        index++;

                        // try parsing following in all contexts
                        var startIndex = index;

                        foreach ( var context in contexts )
                        {
                            index = startIndex;
                            ParseTypeSymbol( id, ref index, compilation, context, results );
                        }
                    }
                    else
                    {
                        // this was a definition where we expected a reference?
                        results.AddRange( contexts.OfType<INamedType>() );
                    }
                }
                else
                {
                    if ( ch == '`' )
                    {
                        ParseTypeParameterSymbol( id, ref index, typeParameterContext, results );
                    }
                    else
                    {
                        var namedTypes = new List<INamedType>();
                        ParseNamedTypeSymbol( id, ref index, compilation, typeParameterContext, namedTypes );
                        results.AddRange( namedTypes );
                    }

                    // apply any array or pointer constructions to results
                    var startIndex = index;
                    var endIndex = index;

                    for ( var i = 0; i < results.Count; i++ )
                    {
                        index = startIndex;
                        var type = results[i];

                        while ( true )
                        {
                            if ( PeekNextChar( id, index ) == '[' )
                            {
                                var bounds = ParseArrayBounds( id, ref index );
                                type = type.MakeArrayType( bounds );

                                continue;
                            }

                            if ( PeekNextChar( id, index ) == '*' )
                            {
                                index++;
                                type = type.MakePointerType();

                                continue;
                            }

                            break;
                        }

                        results[i] = type;
                        endIndex = index;
                    }

                    index = endIndex;
                }
            }

            private static void ParseTypeParameterSymbol( string id, ref int index, IDeclaration? typeParameterContext, List<IType> results )
            {
                // skip the first `
                Invariant.Assert( PeekNextChar( id, index ) == '`' );
                index++;

                if ( PeekNextChar( id, index ) == '`' )
                {
                    // `` means this is a method type parameter
                    index++;
                    var methodTypeParameterIndex = ReadNextInteger( id, ref index );

                    if ( typeParameterContext is IMethod methodContext )
                    {
                        var count = methodContext.TypeParameters.Count;

                        if ( count > 0 && methodTypeParameterIndex < count )
                        {
                            results.Add( methodContext.TypeParameters[methodTypeParameterIndex] );
                        }
                    }
                }
                else
                {
                    // regular type parameter
                    var typeParameterIndex = ReadNextInteger( id, ref index );

                    var typeContext = typeParameterContext is IMethod methodContext ? methodContext.DeclaringType : typeParameterContext as INamedType;

                    if ( typeContext != null && GetNthTypeParameter( typeContext, typeParameterIndex ) is { } typeParameter )
                    {
                        results.Add( typeParameter );
                    }
                }
            }

            private static void ParseNamedTypeSymbol(
                string id,
                ref int index,
                CompilationModel compilation,
                IDeclaration? typeParameterContext,
                List<INamedType> results )
            {
                var containers = new List<IDeclaration> { compilation.GetMergedGlobalNamespace() };

                // loop for dotted names
                while ( true )
                {
                    var name = ParseName( id, ref index );

                    List<IType>? typeArguments = null;
                    var arity = 0;

                    // type arguments
                    if ( PeekNextChar( id, index ) == '{' )
                    {
                        typeArguments = new List<IType>();

                        if ( !ParseTypeArguments( id, ref index, compilation, typeParameterContext, typeArguments ) )
                        {
                            // if no type arguments are found then the type cannot be identified
                            continue;
                        }

                        arity = typeArguments.Count;
                    }
                    else if ( PeekNextChar( id, index ) == '`' )
                    {
                        index++;
                        arity = ReadNextInteger( id, ref index );
                    }

                    if ( arity != 0 || PeekNextChar( id, index ) != '.' )
                    {
                        GetMatchingTypes( containers, name, arity, results );

                        if ( arity != 0 && typeArguments != null && typeArguments.Count != 0 )
                        {
                            var typeArgs = typeArguments.ToArray();

                            for ( var i = 0; i < results.Count; i++ )
                            {
                                results[i] = results[i].WithTypeArguments( typeArgs );
                            }
                        }

                        if ( PeekNextChar( id, index ) == '.' )
                        {
                            index++;
                            containers.Clear();
                            CopyTo( results, containers );
                            results.Clear();

                            continue;
                        }
                    }
                    else
                    {
                        var newContainers = new List<IDeclaration>();
                        GetMatchingNamespaceOrTypes( containers, name, newContainers );

                        Invariant.Assert( PeekNextChar( id, index ) == '.' );
                        index++;
                        containers = newContainers;

                        continue;
                    }

                    break;
                }
            }

            private static int ParseArrayBounds( string id, ref int index )
            {
                index++; // skip '['

                var bounds = 0;

                while ( true )
                {
                    if ( char.IsDigit( PeekNextChar( id, index ) ) )
                    {
                        ReadNextInteger( id, ref index );
                    }

                    if ( PeekNextChar( id, index ) == ':' )
                    {
                        index++;

                        if ( char.IsDigit( PeekNextChar( id, index ) ) )
                        {
                            ReadNextInteger( id, ref index );
                        }
                    }

                    bounds++;

                    if ( PeekNextChar( id, index ) == ',' )
                    {
                        index++;

                        continue;
                    }

                    break;
                }

                if ( PeekNextChar( id, index ) == ']' )
                {
                    index++;
                }

                return bounds;
            }

            private static bool ParseTypeArguments(
                string id,
                ref int index,
                CompilationModel compilation,
                IDeclaration? typeParameterContext,
                List<IType> typeArguments )
            {
                index++; // skip over {

                while ( true )
                {
                    var type = ParseTypeSymbol( id, ref index, compilation, typeParameterContext );

                    if ( type == null )
                    {
                        // if a type argument cannot be identified, argument list is no good
                        return false;
                    }

                    // add first one
                    typeArguments.Add( type );

                    if ( PeekNextChar( id, index ) == ',' )
                    {
                        index++;

                        continue;
                    }

                    break;
                }

                if ( PeekNextChar( id, index ) == '}' )
                {
                    index++;
                }

                return true;
            }

            private static void GetMatchingTypes( List<IDeclaration> containers, string memberName, int arity, List<INamedType> results )
            {
                for ( int i = 0, n = containers.Count; i < n; i++ )
                {
                    GetMatchingTypes( containers[i], memberName, arity, results );
                }
            }

            private static void GetMatchingTypes( IDeclaration container, string memberName, int arity, List<INamedType> results )
            {
                var types = container switch
                {
                    INamespace ns => ns.Types.OfName( memberName ),
                    INamedType namedType => namedType.NestedTypes.OfName( memberName ),
                    _ => Enumerable.Empty<INamedType>()
                };

                foreach ( var type in types )
                {
                    if ( type.TypeParameters.Count == arity )
                    {
                        results.Add( type );
                    }
                }
            }

            private static void GetMatchingNamespaceOrTypes( List<IDeclaration> containers, string memberName, List<IDeclaration> results )
            {
                for ( int i = 0, n = containers.Count; i < n; i++ )
                {
                    GetMatchingNamespaceOrTypes( containers[i], memberName, results );
                }
            }

            private static void GetMatchingNamespaceOrTypes( IDeclaration container, string memberName, List<IDeclaration> results )
            {
                var members = container switch
                {
                    INamespace ns => ns.Types.OfName( memberName )
                        .ConcatNotNull<IDeclaration>( ns.Namespaces.OfName( memberName ) ),
                    INamedType namedType => namedType.NestedTypes.OfName( memberName ),
                    _ => Enumerable.Empty<IDeclaration>()
                };

                foreach ( var member in members )
                {
                    if ( member.DeclarationKind == DeclarationKind.Namespace
                         || (member.DeclarationKind == DeclarationKind.NamedType && ((INamedType) member).TypeParameters.Count == 0) )
                    {
                        results.Add( member );
                    }
                }
            }

            private static void GetMatchingNamespaces( List<IDeclaration> containers, string memberName, List<IDeclaration> results )
            {
                for ( int i = 0, n = containers.Count; i < n; i++ )
                {
                    GetMatchingNamespaces( containers[i], memberName, results );
                }
            }

            private static void GetMatchingNamespaces( IDeclaration container, string memberName, List<IDeclaration> results )
            {
                if ( container is not INamespace ns )
                {
                    return;
                }

                if ( memberName == string.Empty )
                {
                    Invariant.Assert( ns.IsGlobalNamespace );
                    results.Add( ns );

                    return;
                }

                var member = ns.Namespaces.OfName( memberName );

                if ( member != null )
                {
                    results.Add( member );
                }
            }

            private static void GetMatchingMethods(
                string id,
                ref int index,
                List<IDeclaration> containers,
                string memberName,
                int arity,
                CompilationModel compilation,
                List<IDeclaration> results )
            {
                var parameters = new List<ParameterInfo>();

                var startIndex = index;
                var endIndex = index;

                foreach ( var container in containers )
                {
                    if ( container is not INamedType type )
                    {
                        continue;
                    }

                    IEnumerable<IMethodBase> members;

                    if ( memberName == ".ctor" )
                    {
                        members = type.Constructors;
                    }
                    else if ( memberName == ".cctor" )
                    {
                        members = type.StaticConstructor == null ? Enumerable.Empty<IMethodBase>() : new[] { type.StaticConstructor };
                    }
                    else
                    {
                        var methods = type.Methods.OfName( memberName );

                        var accessors = EnumerableExtensions.Concat<IHasAccessors>( type.Properties, type.Indexers, type.Events )
                            .SelectMany( p => p.Accessors )
                            .Where( a => a.Name == memberName );

                        members = methods.Concat( accessors );

                        if ( memberName == "Finalize" )
                        {
                            members = members.ConcatNotNull( type.Finalizer );
                        }
                    }

                    foreach ( var member in members )
                    {
                        index = startIndex;

                        var memberArity = member is IMethod method ? method.TypeParameters.Count : 0;

                        if ( memberArity == arity )
                        {
                            parameters.Clear();

                            if ( PeekNextChar( id, index ) == '(' )
                            {
                                if ( !ParseParameterList( id, ref index, compilation, member, parameters ) )
                                {
                                    // if the parameters cannot be identified (some error), then the symbol cannot match, try next method symbol
                                    continue;
                                }
                            }

                            if ( !AllParametersMatch( member.Parameters, parameters ) )
                            {
                                // parameters don't match, try next method symbol
                                continue;
                            }

                            if ( PeekNextChar( id, index ) == '~' )
                            {
                                index++;
                                var returnType = ParseTypeSymbol( id, ref index, compilation, member );

                                var memberReturnType = member is IMethod method2 ? method2.ReturnType : null;

                                // if return type is specified, then it must match
                                if ( returnType != null && returnType.Equals( memberReturnType, TypeComparison.Default ) )
                                {
                                    // return type matches
                                    results.Add( member );
                                    endIndex = index;
                                }
                            }
                            else
                            {
                                // no return type specified, then any matches
                                results.Add( member );
                                endIndex = index;
                            }
                        }
                    }
                }

                index = endIndex;
            }

            private static void GetMatchingProperties(
                string id,
                ref int index,
                List<IDeclaration> containers,
                string memberName,
                CompilationModel compilation,
                List<IDeclaration> results )
            {
                var startIndex = index;
                var endIndex = index;

                List<ParameterInfo>? parameters = null;

                foreach ( var container in containers )
                {
                    if ( container is not INamedType type )
                    {
                        continue;
                    }

                    memberName = DecodePropertyName( memberName );
                    var members = type.Properties.OfName( memberName ).Concat<IPropertyOrIndexer>( type.Indexers.OfName( memberName ) );

                    foreach ( var member in members )
                    {
                        index = startIndex;

                        var memberParameters = member is IIndexer indexer ? (IReadOnlyList<IParameter>) indexer.Parameters : Array.Empty<IParameter>();

                        if ( PeekNextChar( id, index ) == '(' )
                        {
                            if ( parameters == null )
                            {
                                parameters = new List<ParameterInfo>();
                            }
                            else
                            {
                                parameters.Clear();
                            }

                            if ( ParseParameterList( id, ref index, compilation, member.DeclaringType, parameters )
                                 && AllParametersMatch( memberParameters, parameters ) )
                            {
                                results.Add( member );
                                endIndex = index;
                            }
                        }
                        else if ( memberParameters.Count == 0 )
                        {
                            results.Add( member );
                            endIndex = index;
                        }
                    }
                }

                index = endIndex;
            }

            private static void GetMatchingFields( List<IDeclaration> containers, string memberName, List<IDeclaration> results )
            {
                foreach ( var container in containers )
                {
                    if ( container is not INamedType type )
                    {
                        continue;
                    }

                    var fields = type.Fields.OfName( memberName );

                    results.AddRange( fields );
                }
            }

            private static void GetMatchingEvents( List<IDeclaration> containers, string memberName, List<IDeclaration> results )
            {
                foreach ( var container in containers )
                {
                    if ( container is not INamedType type )
                    {
                        continue;
                    }

                    var events = type.Events.OfName( memberName );

                    results.AddRange( events );
                }
            }

            private static bool AllParametersMatch( IReadOnlyList<IParameter> declarationParameters, List<ParameterInfo> expectedParameters )
            {
                if ( declarationParameters.Count != expectedParameters.Count )
                {
                    return false;
                }

                for ( var i = 0; i < expectedParameters.Count; i++ )
                {
                    if ( !ParameterMatches( declarationParameters[i], expectedParameters[i] ) )
                    {
                        return false;
                    }
                }

                return true;
            }

            private static bool ParameterMatches( IParameter parameter, ParameterInfo parameterInfo )
            {
                // both by ref or both not by ref
                var isRefOrOut = parameter.RefKind != RefKind.None;

                if ( isRefOrOut != parameterInfo.IsRefOrOut )
                {
                    return false;
                }

                return parameter.Type.Equals( parameterInfo.Type, TypeComparison.Default );
            }

            private static ITypeParameter? GetNthTypeParameter( INamedType type, int n )
            {
                var containingTypeParameterCount = GetTypeParameterCount( type.DeclaringType );

                if ( n < containingTypeParameterCount )
                {
                    return GetNthTypeParameter( type.DeclaringType!, n );
                }

                var index = n - containingTypeParameterCount;
                var typeParameters = type.TypeParameters;

                if ( index < typeParameters.Count )
                {
                    return typeParameters[index];
                }

                return null;
            }

            private static int GetTypeParameterCount( INamedType? type )
            {
                if ( type == null )
                {
                    return 0;
                }

                return type.TypeParameters.Count + GetTypeParameterCount( type.DeclaringType );
            }

            [StructLayout( LayoutKind.Auto )]
            private readonly struct ParameterInfo
            {
                internal readonly IType Type;
                internal readonly bool IsRefOrOut;

                public ParameterInfo( IType type, bool isRefOrOut )
                {
                    this.Type = type;
                    this.IsRefOrOut = isRefOrOut;
                }
            }

            private static bool ParseParameterList(
                string id,
                ref int index,
                CompilationModel compilation,
                IDeclaration typeParameterContext,
                List<ParameterInfo> parameters )
            {
                index++; // skip over '('

                if ( PeekNextChar( id, index ) == ')' )
                {
                    index++;

                    return true;
                }

                var parameter = ParseParameter( id, ref index, compilation, typeParameterContext );

                if ( parameter == null )
                {
                    return false;
                }

                parameters.Add( parameter.Value );

                while ( PeekNextChar( id, index ) == ',' )
                {
                    index++;

                    parameter = ParseParameter( id, ref index, compilation, typeParameterContext );

                    if ( parameter == null )
                    {
                        return false;
                    }

                    parameters.Add( parameter.Value );
                }

                if ( PeekNextChar( id, index ) == ')' )
                {
                    index++;
                }

                return true;
            }

            private static ParameterInfo? ParseParameter( string id, ref int index, CompilationModel compilation, IDeclaration? typeParameterContext )
            {
                var isRefOrOut = false;

                var type = ParseTypeSymbol( id, ref index, compilation, typeParameterContext );

                if ( type == null )
                {
                    // if no type can be identified, then there is no parameter
                    return null;
                }

                if ( PeekNextChar( id, index ) == '@' )
                {
                    index++;
                    isRefOrOut = true;
                }

                return new ParameterInfo( type, isRefOrOut );
            }

            private static char PeekNextChar( string id, int index )
            {
                return index >= id.Length ? '\0' : id[index];
            }

            private static readonly char[] _nameDelimiters = { ':', '.', '(', ')', '{', '}', '[', ']', ',', '\'', '@', '*', '`', '~' };

            private static string ParseName( string id, ref int index )
            {
                string name;

                var delimiterOffset = id.IndexOfAny( _nameDelimiters, index );

                if ( delimiterOffset >= 0 )
                {
                    name = id.Substring( index, delimiterOffset - index );
                    index = delimiterOffset;
                }
                else
                {
                    name = id.Substring( index );
                    index = id.Length;
                }

                return DecodeName( name );
            }

            // undoes dot encodings within names...
            private static string DecodeName( string name ) => name.Replace( '#', '.' );

            private static int ReadNextInteger( string id, ref int index )
            {
                var n = 0;

                while ( index < id.Length && char.IsDigit( id[index] ) )
                {
                    n = (n * 10) + (id[index] - '0');
                    index++;
                }

                return n;
            }

            private static void CopyTo<TSource, TDestination>( List<TSource> source, List<TDestination> destination )
                where TSource : class
                where TDestination : class
            {
                if ( destination.Count + source.Count > destination.Capacity )
                {
                    destination.Capacity = destination.Count + source.Count;
                }

                for ( int i = 0, n = source.Count; i < n; i++ )
                {
                    destination.Add( (TDestination) (object) source[i] );
                }
            }
        }
    }
}