// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Accessibility = Microsoft.CodeAnalysis.Accessibility;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class BaseDeclarationCollection
{
    protected BaseDeclarationCollection( CompilationModel compilation )
    {
        this.Compilation = compilation;
    }

    public CompilationModel Compilation { get; protected set; }

    protected virtual bool IsSymbolIncluded( ISymbol symbol ) => !this.IsHidden( symbol );

    private bool IsHidden( ISymbol symbol )
    {
        // Private symbols of external assemblies must be hidden because these references are not available in a PE reference (i.e. at compile time)
        // but are available in a CompilationReference (i.e. at design time, if both projects are in the same solution).
        if ( symbol.DeclaredAccessibility == Accessibility.Private
             && !this.Compilation.Options.ShowExternalPrivateMembers
             && !SymbolEqualityComparer.Default.Equals( symbol.ContainingAssembly, this.Compilation.RoslynCompilation.Assembly ) )
        {
            return true;
        }

        // Compile-time-only symbols are hidden.
        if ( this.Compilation.Project.CompileTimeProject?.Manifest?.Templates?.GetExecutionScope( symbol ) == ExecutionScope.CompileTime )
        {
            return true;
        }

        // Symbols defined by a our own source generator must be hidden.
        if ( SourceGeneratorHelper.IsGeneratedSymbol( symbol ) )
        {
            return true;
        }

        return false;
    }
}