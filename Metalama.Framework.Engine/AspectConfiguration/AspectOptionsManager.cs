// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Options;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.AspectConfiguration;

public partial class AspectOptionsManager : IAspectOptionsManager
{
    private readonly ConcurrentDictionary<string, OptionTypeNode> _sources = new();
    private readonly ConcurrentDictionary<string, Framework.Options.AspectOptions> _defaultOptions = new();
    private readonly ProjectServiceProvider _serviceProvider;
    private ProjectSpecificCompileTimeTypeResolver? _typeResolver;

    public AspectOptionsManager( ProjectServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
    }

    internal void AddSources( ImmutableArray<IConfiguratorSource> sources, CompilationModel compilationModel, IDiagnosticAdder diagnosticAdder )
    {
        foreach ( var source in sources )
        {
            this.AddSource( source, compilationModel, diagnosticAdder );
        }
    }

    internal void AddSource( IConfiguratorSource source, CompilationModel compilationModel, IDiagnosticAdder diagnosticAdder )
    {
        foreach ( var configurator in source.GetConfigurators( compilationModel, diagnosticAdder ) )
        {
            var optionTypeName = configurator.Options.GetType().FullName.AssertNotNull();

            if ( !this._sources.TryGetValue( optionTypeName, out var optionTypeNode ) )
            {
                // We get the type resolver lazily because several tests do not supply it.
                this._typeResolver = this._serviceProvider.GetRequiredService<ProjectSpecificCompileTimeTypeResolver>();

                var optionType =
                    this._typeResolver.GetCompileTimeType(
                            compilationModel.Factory.GetTypeByReflectionName( optionTypeName ).GetSymbol().AssertNotNull(),
                            false )
                        .AssertNotNull();

                optionTypeNode = this._sources.GetOrAdd( optionTypeName, _ => new OptionTypeNode( this, optionType, diagnosticAdder ) );
            }

            optionTypeNode.AddConfigurator( configurator, diagnosticAdder );
        }
    }

    public TOptions GetOptions<TOptions>( IDeclaration declaration )
        where TOptions : Framework.Options.AspectOptions, new()
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
        where T : Framework.Options.AspectOptions, new()
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