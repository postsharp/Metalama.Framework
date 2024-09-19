// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using Accessibility = Microsoft.CodeAnalysis.Accessibility;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.References;

internal class SymbolRefStrategy : IRefStrategy
{
    private readonly CompilationContext _compilationContext;

    public SymbolRefStrategy( CompilationContext compilationContext )
    {
        this._compilationContext = compilationContext;
    }

    public void EnumerateAttributes( IRef<IDeclaration> declaration, CompilationModel compilation, Action<IRef<IAttribute>> add )
    {
        var symbolRef = (ISymbolRef) declaration;

        var attributes = symbolRef.TargetKind switch
        {
            RefTargetKind.Return => ((IMethodSymbol) symbolRef.Symbol).GetReturnTypeAttributes(),
            RefTargetKind.Default => symbolRef.Symbol.GetAttributes(),
            _ => throw new NotImplementedException()
        };

        foreach ( var attribute in attributes )
        {
            if ( !attribute.IsValid() )
            {
                continue;
            }

            // Note that Roslyn can return an AttributeData that does not belong to the same compilation
            // as the parent symbol, probably because of some bug or optimisation.

            add( new SymbolAttributeRef( attribute, declaration, compilation.CompilationContext ) );
        }

        if ( compilation.TryGetRedirectedDeclaration( declaration, out var redirectedDeclaration ) )
        {
            // If the declaration was redirected, we need to add the attributes from the builder.
            if ( redirectedDeclaration is IBuilderRef { Builder: { } redirectedBuilder } )
            {
                foreach ( var attribute in redirectedBuilder.Attributes )
                {
                    add( attribute.ToRef() );
                }
            }
            else
            {
                Invariant.Assert( false );
            }
        }
    }

    public void EnumerateAllImplementedInterfaces( IRef<INamedType> namedType, CompilationModel compilation, Action<IRef<INamedType>> add )
    {
        var namedTypeSymbol = (INamedTypeSymbol) ((ISymbolRef) namedType).Symbol;

        foreach ( var i in namedTypeSymbol.AllInterfaces )
        {
            if ( !SymbolValidator.Instance.Visit( i ) )
            {
                continue;
            }

            add( this._compilationContext.RefFactory.FromSymbol<INamedType>( i ) );
        }
    }

    public void EnumerateImplementedInterfaces( IRef<INamedType> namedType, CompilationModel compilation, Action<IRef<INamedType>> add )
    {
        var namedTypeSymbol = (INamedTypeSymbol) ((ISymbolRef) namedType).Symbol;

        foreach ( var i in namedTypeSymbol.Interfaces )
        {
            if ( !SymbolValidator.Instance.Visit( i ) )
            {
                continue;
            }

            add( this._compilationContext.RefFactory.FromSymbol<INamedType>( i ) );
        }
    }

    public IEnumerable<IRef<T>> GetMembersOfName<T>( IRef<INamespaceOrNamedType> parent, string name, DeclarationKind kind, CompilationModel compilation )
        where T : class, INamedDeclaration
    {
        switch ( kind )
        {
            case DeclarationKind.Namespace:
                {
                    var symbol = (INamespaceSymbol) ((ISymbolRef) parent).Symbol;

                    return symbol.GetNamespaceMembers()
                        .Where( ns => ns.Name == name && IsValidNamespace( ns, compilation ) )
                        .Select( s => this._compilationContext.RefFactory.FromSymbol<T>( s ) );
                }

            default:
                {
                    var symbol = (INamespaceOrTypeSymbol) ((ISymbolRef) parent).Symbol;

                    var predicate = GetSymbolPredicate( kind );

                    return symbol.GetMembers( name )
                        .Where( x => predicate( x, compilation ) )
                        .Select( s => this._compilationContext.RefFactory.FromSymbol<T>( s ) );
                }
        }
    }

    public IEnumerable<IRef<T>> GetMembers<T>( IRef<INamespaceOrNamedType> parent, DeclarationKind kind, CompilationModel compilation )
        where T : class, INamedDeclaration
    {
        var parentSymbol = ((ISymbolRef) parent).Symbol;

        switch ( kind )
        {
            case DeclarationKind.Namespace:
                {
                    var parentNs = (INamespaceSymbol) parentSymbol;

                    return parentNs.GetNamespaceMembers()
                        .Where( ns => IsValidNamespace( ns, compilation ) )
                        .Select( s => this._compilationContext.RefFactory.FromSymbol<T>( s ) );
                }

            case DeclarationKind.NamedType:
                {
                    var parentScope = (INamespaceOrTypeSymbol) parentSymbol;

                    return parentScope.GetTypeMembers()
                        .Where( t => IsValidNamedType( t, compilation ) )
                        .Select( t => this._compilationContext.RefFactory.FromSymbol<T>( t ) );
                }

            default:
                {
                    var parentScope = (INamespaceOrTypeSymbol) parentSymbol;

                    var predicate = GetSymbolPredicate( kind );

                    return parentScope.GetMembers()
                        .Where( m => predicate( m, compilation ) )
                        .Select( m => this._compilationContext.RefFactory.FromSymbol<T>( m ) );
                }
        }
    }

    public bool Is( IRef<IType> left, IRef<IType> right, ConversionKind kind = default, TypeComparison typeComparison = TypeComparison.Default )
    {
        if ( right is not ISymbolRef rightSymbolRef )
        {
            throw new ArgumentOutOfRangeException( nameof(right), "Introduced types on the left side of a comparison are not supported." );
        }

        if ( typeComparison != TypeComparison.Default )
        {
            throw new NotImplementedException( "Only TypeComparison.Default implemented on references." );
        }

        var leftSymbol = (ITypeSymbol) ((ISymbolRef) left).Symbol;
        var rightSymbol = (ITypeSymbol) rightSymbolRef.Symbol;

        switch ( kind )
        {
            case ConversionKind.Implicit:
                return this._compilationContext.Compilation.HasImplicitConversion( leftSymbol, rightSymbol );

            case ConversionKind.TypeDefinition:

                bool IsOfTypeDefinitionRecursive( INamedTypeSymbol t )
                {
                    if ( t.OriginalDefinition == rightSymbol )
                    {
                        return true;
                    }

                    if ( t.BaseType != null && IsOfTypeDefinitionRecursive( t.BaseType ) )
                    {
                        return true;
                    }

                    foreach ( var i in t.AllInterfaces )
                    {
                        if ( IsOfTypeDefinitionRecursive( i ) )
                        {
                            return true;
                        }
                    }

                    return false;
                }

                if ( leftSymbol is INamedTypeSymbol leftNamedType && rightSymbol is INamedTypeSymbol rightNamedType )
                {
                    return IsOfTypeDefinitionRecursive( leftNamedType );
                }
                else
                {
                    return false;
                }

            default:
                throw new NotImplementedException( "This ConversionKind is not implemented." );
        }
    }

    private static bool IsValidSymbol( ISymbol symbol, CompilationModel compilation )
    {
        // Private symbols of external assemblies must be hidden because these references are not available in a PE reference (i.e. at compile time)
        // but are available in a CompilationReference (i.e. at design time, if both projects are in the same solution).
        if ( symbol.DeclaredAccessibility == Accessibility.Private
             && !compilation.Options.ShowExternalPrivateMembers
             && !compilation.CompilationContext.SymbolComparer.Equals( symbol.ContainingAssembly, compilation.RoslynCompilation.Assembly ) )
        {
            return false;
        }

        // Compile-time-only symbols are hidden.
        if ( compilation.Project.ClassificationService?.GetExecutionScope( symbol ) == ExecutionScope.CompileTime )
        {
            return false;
        }

        // Symbols defined by a our own source generator must be hidden.
        if ( SourceGeneratorHelper.IsGeneratedSymbol( symbol ) )
        {
            return false;
        }

        if ( !SymbolValidator.Instance.Visit( symbol ) )
        {
            return false;
        }

        return true;
    }

    private static bool IsEventSymbolIncluded( ISymbol symbol, CompilationModel compilation )
        => symbol.Kind == SymbolKind.Event && IsValidSymbol( symbol, compilation );

    private static bool IsValidConstructor( ISymbol symbol, CompilationModel compilation )
        => symbol.Kind == SymbolKind.Method &&
           ((IMethodSymbol) symbol).MethodKind is MethodKind.Constructor && IsValidSymbol( symbol, compilation );

    private static bool IsValidField( ISymbol symbol, CompilationModel compilation ) => symbol.Kind == SymbolKind.Field && IsValidSymbol( symbol, compilation );

    private static bool IsValidIndexer( ISymbol symbol, CompilationModel compilation )
        => symbol.Kind == SymbolKind.Property && ((IPropertySymbol) symbol).Parameters.Length > 0 && IsValidSymbol( symbol, compilation );

    private static bool IsValidMethod( ISymbol symbol, CompilationModel compilation )
        => symbol.Kind == SymbolKind.Method && IsValidSymbol( symbol, compilation ) && symbol switch
        {
            IMethodSymbol method =>
                method switch
                {
                    // Metalama code model represents what can be seen from C#, so it hides "unspeakable" methods, namely Program.<Main>$ and SomeRecord.<Clone>$
                    { MethodKind: MethodKind.Ordinary, CanBeReferencedByName: false } => false,
                    { MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor } => false,
                    { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet } => false,
                    { MethodKind: MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.EventRaise } => false,
                    { MethodKind: MethodKind.Destructor } => false,
                    _ => true
                },
            _ => false
        };

    private static bool IsValidNamespace( INamespaceSymbol symbol, CompilationModel compilation )
    {
        if ( symbol.Kind != SymbolKind.Namespace )
        {
            return false;
        }

        if ( !IsValidSymbol( symbol, compilation ) )
        {
            return false;
        }

        if ( symbol.ContainingAssembly == compilation.RoslynCompilation.Assembly )
        {
            // For types defined in the current assembly, we need to take partial compilations into account.

            return IsIncludedInPartialCompilation( symbol );

            bool IsIncludedInPartialCompilation( INamespaceSymbol t )
            {
                return t switch
                {
                    { ContainingNamespace: { IsGlobalNamespace: false } containingNamespace } => IsIncludedInPartialCompilation( containingNamespace ),
                    _ => compilation.PartialCompilation.Namespaces.Contains( t.OriginalDefinition )
                };
            }
        }
        else
        {
            return true;
        }
    }

    private static bool IsValidProperty( ISymbol symbol, CompilationModel compilation )
        => symbol.Kind == SymbolKind.Property
           && IsValidSymbol( symbol, compilation ) && ((IPropertySymbol) symbol).Parameters.Length == 0;

    private static bool IsValidNamedType( ISymbol symbol, CompilationModel compilation )
        => symbol is INamedTypeSymbol namedTypeSymbol && IsValidNamedType( namedTypeSymbol, compilation );

    private static bool IsValidNamedType( INamedTypeSymbol symbol, CompilationModel compilation )
    {
        if ( !IsValidSymbol( symbol, compilation ) )
        {
            return false;
        }

        if ( compilation.Project.ClassificationService?.GetExecutionScope( symbol ) == ExecutionScope.CompileTime )
        {
            return false;
        }

        if ( symbol.ContainingAssembly == compilation.RoslynCompilation.Assembly )
        {
            // For types defined in the current assembly, we need to take partial compilations into account.

            return IsIncludedInPartialCompilation( symbol );

            bool IsIncludedInPartialCompilation( INamedTypeSymbol t )
            {
                return t switch
                {
                    { ContainingType: { } containingType } => IsIncludedInPartialCompilation( containingType ),
                    _ => compilation.PartialCompilation.Types.Contains( t.OriginalDefinition )
                };
            }
        }
        else
        {
            return true;
        }
    }

    private static Func<ISymbol, CompilationModel, bool> GetSymbolPredicate( DeclarationKind kind )
        => kind switch
        {
            DeclarationKind.Event => IsEventSymbolIncluded,
            DeclarationKind.Constructor => IsValidConstructor,
            DeclarationKind.Field => IsValidField,
            DeclarationKind.Indexer => IsValidIndexer,
            DeclarationKind.Method => IsValidMethod,
            DeclarationKind.Property => IsValidProperty,
            DeclarationKind.NamedType => IsValidNamedType,
            _ => throw new NotImplementedException()
        };
}