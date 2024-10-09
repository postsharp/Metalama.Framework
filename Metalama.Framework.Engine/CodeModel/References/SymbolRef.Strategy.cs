// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using Accessibility = Microsoft.CodeAnalysis.Accessibility;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed partial class SymbolRef<T>
{
    public override void EnumerateAttributes( CompilationModel compilation, Action<AttributeRef> add )
    {
        Invariant.Assert( this is IRef<IDeclaration> );

        IEnumerable<AttributeData> attributes = this.TargetKind switch
        {
            RefTargetKind.Return => ((IMethodSymbol) this.Symbol).GetReturnTypeAttributes(),
            RefTargetKind.Default => this.Symbol.GetAttributes(),
            _ => throw new AssertionFailedException()
        };

        if ( this.Symbol is ISourceAssemblySymbol sourceAssemblySymbol )
        {
            // Also add [module:*] attributes.
            attributes = attributes.Concat( sourceAssemblySymbol.Modules.SelectMany( m => m.GetAttributes() ) );
        }

        foreach ( var attribute in attributes )
        {
            if ( !attribute.IsValid() )
            {
                continue;
            }

            // Note that Roslyn can return an AttributeData that does not belong to the same compilation
            // as the this symbol, probably because of some bug or optimisation.

            add( new SymbolAttributeRef( attribute, this.As<IDeclaration>(), compilation.CompilationContext ) );
        }

        if ( compilation.TryGetRedirectedDeclaration( this, out var redirectedDeclaration ) )
        {
            // If the declaration was redirected, we need to add the attributes from the builder.
            foreach ( var attribute in redirectedDeclaration.Attributes )
            {
                add( attribute.ToRef() );
            }
        }
    }

    public override void EnumerateAllImplementedInterfaces( CompilationModel compilation, Action<IFullRef<INamedType>> add )
    {
        Invariant.Assert( this is IRef<INamedType> );
        var namedTypeSymbol = (INamedTypeSymbol) this.Symbol;

        foreach ( var i in namedTypeSymbol.AllInterfaces )
        {
            if ( !SymbolValidator.Instance.Visit( i ) )
            {
                continue;
            }

            add( this.CompilationContext.RefFactory.FromSymbol<INamedType>( i ) );
        }
    }

    public override void EnumerateImplementedInterfaces( CompilationModel compilation, Action<IFullRef<INamedType>> add )
    {
        Invariant.Assert( this is IRef<INamedType> );

        var namedTypeSymbol = (INamedTypeSymbol) this.Symbol;

        foreach ( var i in namedTypeSymbol.Interfaces )
        {
            if ( !SymbolValidator.Instance.Visit( i ) )
            {
                continue;
            }

            add( this.CompilationContext.RefFactory.FromSymbol<INamedType>( i ) );
        }
    }

    public override IEnumerable<IFullRef> GetMembersOfName(
        string name,
        DeclarationKind kind,
        CompilationModel compilation )
    {
        switch ( kind )
        {
            case DeclarationKind.Namespace:
                {
                    var symbol = (INamespaceSymbol) this.Symbol;

                    return symbol.GetNamespaceMembers()
                        .Where( ns => ns.Name == name && IsValidNamespace( ns, compilation ) )
                        .Select( s => this.CompilationContext.RefFactory.FromAnySymbol( s ) );
                }

            case DeclarationKind.NamedType when this.DeclarationKind == DeclarationKind.Compilation:
                return compilation.PartialCompilation.Types
                    .Where( t => t.Name == name && IsValidNamedType( t, compilation ) )
                    .Select( s => this.CompilationContext.RefFactory.FromSymbol<INamedType>( s ) );

            default:
                {
                    var symbol = (INamespaceOrTypeSymbol) this.Symbol;

                    var predicate = GetSymbolPredicate( kind );

                    return symbol.GetMembers( name )
                        .Where( x => predicate( x, compilation ) )
                        .Select( s => this.CompilationContext.RefFactory.FromAnySymbol( s ) );
                }
        }
    }

    public override IEnumerable<IFullRef> GetMembers( DeclarationKind kind, CompilationModel compilation )
    {
        var parentSymbol = this.Symbol;

        switch ( kind )
        {
            case DeclarationKind.Namespace:
                {
                    var parentNs = (INamespaceSymbol) parentSymbol;

                    return parentNs.GetNamespaceMembers()
                        .Where( ns => IsValidNamespace( ns, compilation ) )
                        .Select( s => this.CompilationContext.RefFactory.FromAnySymbol( s ) );
                }

            case DeclarationKind.NamedType when this.DeclarationKind is DeclarationKind.Namespace or DeclarationKind.NamedType:
                {
                    var parentScope = (INamespaceOrTypeSymbol) parentSymbol;

                    return parentScope.GetTypeMembers()
                        .Where( t => IsValidNamedType( t, compilation ) )
                        .Select( t => this.CompilationContext.RefFactory.FromAnySymbol( t ) );
                }

            case DeclarationKind.NamedType when this.DeclarationKind is DeclarationKind.Compilation:
                {
                    return compilation.PartialCompilation.Types
                        .Where( t => IsValidNamedType( t, compilation ) )
                        .Select( s => this.CompilationContext.RefFactory.FromSymbol<INamedType>( s ) );
                }

            default:
                {
                    var parentScope = (INamespaceOrTypeSymbol) parentSymbol;

                    var predicate = GetSymbolPredicate( kind );

                    return parentScope.GetMembers()
                        .Where( m => predicate( m, compilation ) )
                        .Select( m => this.CompilationContext.RefFactory.FromAnySymbol( m ) );
                }
        }
    }

    public override bool IsConvertibleTo( IRef<IType> right, ConversionKind kind = default, TypeComparison typeComparison = TypeComparison.Default )
    {
        Invariant.Assert( this is IRef<IType> );

        if ( right is not ISymbolRef rightSymbolRef )
        {
            throw new ArgumentOutOfRangeException( nameof(right), "Introduced types on the left side of a comparison are not supported." );
        }

        if ( typeComparison != TypeComparison.Default )
        {
            throw new NotImplementedException( "Only TypeComparison.Default implemented on references." );
        }

        var leftSymbol = (ITypeSymbol) this.Symbol;
        var rightSymbol = (ITypeSymbol) rightSymbolRef.Symbol;

        switch ( kind )
        {
            // TODO: Process Default separately from Implicit.
            case ConversionKind.Implicit or ConversionKind.Default:
                return this.CompilationContext.Compilation.HasImplicitConversion( leftSymbol, rightSymbol );

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

    public override IAssemblySymbol GetAssemblySymbol( CompilationContext compilationContext )
        => this.Symbol.ContainingAssembly.AssertBelongsToCompilationContext( compilationContext );

    public override bool IsStatic => this.Symbol.IsStatic;

    public override IFullRef<IMember> TypeMember
    {
        get
        {
            Invariant.Assert( this is IRef<IMember> );

            return this.Symbol switch
            {
                IMethodSymbol
                    {
                        MethodKind: RoslynMethodKind.EventAdd or RoslynMethodKind.EventRaise or RoslynMethodKind.EventRemove
                    } accessor
                    => this.CompilationContext.RefFactory.FromSymbol<IEvent>( accessor.AssociatedSymbol.AssertSymbolNotNull() ),
                IMethodSymbol
                    {
                        MethodKind: RoslynMethodKind.PropertyGet or RoslynMethodKind.PropertySet
                    } accessor
                    => this.CompilationContext.RefFactory.FromSymbol<IProperty>( accessor.AssociatedSymbol.AssertSymbolNotNull() ),
                _ => this.As<IMember>()
            };
        }
    }

    public override MethodKind MethodKind
    {
        get
        {
            Invariant.Assert( this is IRef<IMethod> );

            return ((IMethodSymbol) this.Symbol).MethodKind.ToOurMethodKind();
        }
    }

    public override bool MethodBodyReturnsVoid
    {
        get
        {
            Invariant.Assert( this is IRef<IMethod> or IRef<IConstructor> );

            var methodSymbol = (IMethodSymbol) this.Symbol;

            if ( methodSymbol.ReturnType.SpecialType == SpecialType.System_Void )
            {
                return true;
            }

            if ( !AsyncHelper.TryGetAsyncInfo( methodSymbol.ReturnType, out var returnArgumentType, out _ ) )
            {
                return false;
            }

            return returnArgumentType.SpecialType == SpecialType.System_Void;
        }
    }

    public override IFullRef<INamedType> DeclaringType => this.CompilationContext.RefFactory.FromSymbol<INamedType>( this.Symbol.ContainingType );

    public override bool IsPrimaryConstructor
    {
        get
        {
            Invariant.Assert( this is IRef<IConstructor> );

            return ((IMethodSymbol) this.Symbol).IsPrimaryConstructor();
        }
    }

    public override bool IsValid => this.Symbol is ITypeSymbol { TypeKind: TypeKind.Error };

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
           ((IMethodSymbol) symbol).MethodKind is RoslynMethodKind.Constructor && IsValidSymbol( symbol, compilation );

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
                    { MethodKind: RoslynMethodKind.Ordinary, CanBeReferencedByName: false } => false,
                    { MethodKind: RoslynMethodKind.Constructor or RoslynMethodKind.StaticConstructor } => false,
                    { MethodKind: RoslynMethodKind.PropertyGet or RoslynMethodKind.PropertySet } => false,
                    { MethodKind: RoslynMethodKind.EventAdd or RoslynMethodKind.EventRemove or RoslynMethodKind.EventRaise } => false,
                    { MethodKind: RoslynMethodKind.Destructor } => false,
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