// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Options;
using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.UserOptions;

public partial class AspectOptionsManager : IAspectOptionsManager
{
    private readonly ConcurrentDictionary<string, OptionTypeNode> _sources = new();
    private readonly ConcurrentDictionary<string, AspectOptions> _defaultOptions = new();
    private readonly ProjectServiceProvider _serviceProvider;

    public AspectOptionsManager( ProjectServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
    }

    internal void AddSource( IConfiguratorSource source, CompilationModel compilationModel, IDiagnosticAdder diagnosticAdder )
    {
        foreach ( var optionTypeName in source.OptionTypes )
        {
            if ( !this._sources.TryGetValue( optionTypeName, out var optionTypeNode ) )
            {
                var optionType = compilationModel.Factory.GetTypeByReflectionName( optionTypeName ).ToType();
                optionTypeNode = this._sources.GetOrAdd( optionTypeName, _ => new OptionTypeNode( this, optionType, diagnosticAdder ) );
            }

            optionTypeNode.AddSource( source, compilationModel, diagnosticAdder );
        }
    }

    public TOptions GetOptions<TOptions>( IDeclaration declaration )
        where TOptions : AspectOptions, new()
    {
        if ( this._sources.TryGetValue( typeof(TOptions).FullName.AssertNotNull(), out var node ) )
        {
            return node.GetOptions<TOptions>( declaration ).AssertNotNull();
        }
        else
        {
            return this.GetDefaultOptions<TOptions>( declaration.Compilation );
        }
    }

    private T GetDefaultOptions<T>( ICompilation compilation ) 
        where T : AspectOptions, new()
    {
        var optionTypeName = typeof(T).FullName.AssertNotNull();

        if ( !this._defaultOptions.TryGetValue( optionTypeName, out var options ) )
        {
            var empty = new T();
            options = empty.GetDefaultOptions( compilation.Project ) ?? empty;
            this._defaultOptions.TryAdd( optionTypeName, options );
        }

        return (T) options;
    }
}