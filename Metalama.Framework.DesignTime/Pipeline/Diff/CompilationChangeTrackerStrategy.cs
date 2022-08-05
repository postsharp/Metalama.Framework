using K4os.Hash.xxHash;
using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

internal class CompilationChangeTrackerStrategy
{
    private bool _isTest;

    private readonly bool _detectCompileTimeCode;
    private readonly bool _detectPartialTypes;

    public CompilationChangeTrackerStrategy( IServiceProvider serviceProvider, bool detectCompileTimeCode, bool detectPartialTypes )
    {
        this._detectCompileTimeCode = detectCompileTimeCode;
        this._detectPartialTypes = detectPartialTypes;
        this._isTest = serviceProvider.GetService<TestMarkerService>() != null;
    }

    public CompileTimeChangeKind GetCompileTimeChangeKind( bool oldValue, bool newValue )
        => (oldValue, newValue) switch
        {
            (true, true) => CompileTimeChangeKind.None,
            (false, false) => CompileTimeChangeKind.None,
            (true, false) => CompileTimeChangeKind.NoLongerCompileTime,
            (false, true) => CompileTimeChangeKind.NewlyCompileTime
        };

    /// <summary>
    /// Determines whether two syntax trees are significantly different. This overload is called from tests.
    /// </summary>
    public bool IsDifferent( SyntaxTree oldSyntaxTree, SyntaxTree newSyntaxTree )
    {
        if ( this._detectPartialTypes )
        {
            throw new InvalidOperationException();
        }
        var oldSyntaxTreeVersion = this.GetSyntaxTreeVersion( oldSyntaxTree, null );

        return this.IsDifferent( oldSyntaxTreeVersion, newSyntaxTree, null, out _ );
    }

    public bool IsDifferent(
        in SyntaxTreeVersion oldSyntaxTreeVersion,
        SyntaxTree newSyntaxTree,
        Compilation? newCompilation,
        out SyntaxTreeVersion newSyntaxTreeVersion )
    {
        // Check if the source text has changed.
        if ( newSyntaxTree == oldSyntaxTreeVersion.SyntaxTree )
        {
            newSyntaxTreeVersion = oldSyntaxTreeVersion;

            return false;
        }
        else
        {
            var newSyntaxRoot = newSyntaxTree.GetRoot();
            var newHasCompileTimeCode = this._detectCompileTimeCode && CompileTimeCodeFastDetector.HasCompileTimeCode( newSyntaxRoot );
            var hhx64 = new XXH64();
            BaseCodeHasher hasher = newHasCompileTimeCode ? new CompileTimeCodeHasher( hhx64 ) : new RunTimeCodeHasher( hhx64 );
            hasher.Visit( newSyntaxRoot );
            var newSyntaxTreeHash = hhx64.Digest();

            if ( newSyntaxTreeHash == oldSyntaxTreeVersion.DeclarationHash )
            {
                newSyntaxTreeVersion = oldSyntaxTreeVersion;
                    
                return false;
            }
            else
            {
                var (partialTypes, partialTypesHash) = this.FindPartialTypes( newSyntaxTree, newCompilation, hhx64 );

                newSyntaxTreeVersion = new SyntaxTreeVersion( newSyntaxTree, newHasCompileTimeCode, newSyntaxTreeHash, partialTypes, partialTypesHash );

                return true;
            }
        }
    }

    private (ImmutableArray<TypeDependencyKey> partialTypes, ulong partialTypesHash) FindPartialTypes( SyntaxTree syntaxTree, Compilation? compilation, XXH64 hhx64 )
    {
        if ( !this._detectPartialTypes )
        {
            return default;
        }
        else if ( compilation == null )
        {
            throw new ArgumentNullException( nameof(compilation) );
        }
        var syntaxRoot = syntaxTree.GetRoot();
        var partialTypes = PartialTypesVisitor.Instance.Visit( syntaxRoot );
        var partialTypesHash = 0ul;

        ImmutableArray<TypeDependencyKey> partialTypeKeys;

        if (!partialTypes.IsDefaultOrEmpty)
        {
            var semanticModel = compilation.GetSemanticModel( syntaxTree );
            var partialTypeKeysBuilder = ImmutableArray.CreateBuilder<TypeDependencyKey>( partialTypes.Length );
            
            hhx64.Reset();

            foreach (var partialType in partialTypes)
            {
                var symbol = (INamedTypeSymbol) semanticModel.GetDeclaredSymbol( partialType ).AssertNotNull(  );
                partialTypeKeysBuilder.Add( new TypeDependencyKey( symbol, this._isTest ) );
            }

            partialTypesHash = hhx64.Digest();
            partialTypeKeys = partialTypeKeysBuilder.MoveToImmutable();
        }
        else
        {
            partialTypeKeys = ImmutableArray<TypeDependencyKey>.Empty;
        }

        return ( partialTypeKeys, partialTypesHash );
    }

    public SyntaxTreeVersion GetSyntaxTreeVersion( SyntaxTree syntaxTree, Compilation? compilation )
    {
        var syntaxRoot = syntaxTree.GetRoot();
        var hasCompileTimeCode = this._detectCompileTimeCode && CompileTimeCodeFastDetector.HasCompileTimeCode( syntaxRoot );
        var hhx64 = new XXH64();
        BaseCodeHasher hasher = hasCompileTimeCode ? new CompileTimeCodeHasher( hhx64 ) : new RunTimeCodeHasher( hhx64 );
        hasher.Visit( syntaxRoot );
        var declarationHash = hhx64.Digest();
        
        var (partialTypes, partialTypesHash) = this.FindPartialTypes( syntaxTree, compilation, hhx64 );

        
        return new SyntaxTreeVersion( syntaxTree, hasCompileTimeCode, declarationHash, partialTypes, partialTypesHash );
    }
}