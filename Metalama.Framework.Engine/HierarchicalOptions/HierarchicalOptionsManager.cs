// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Options;
using Metalama.Framework.Project;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;

namespace Metalama.Framework.Engine.HierarchicalOptions;

public sealed partial class HierarchicalOptionsManager : IHierarchicalOptionsManager
{
    private readonly ConcurrentDictionary<string, OptionTypeNode> _optionTypes = new();
    private readonly ConcurrentDictionary<string, IHierarchicalOptions> _defaultOptions = new();
    private readonly ProjectServiceProvider _serviceProvider;
    private IExternalHierarchicalOptionsProvider? _externalOptionsProvider;

    private ProjectSpecificCompileTimeTypeResolver? _typeResolver;

    internal HierarchicalOptionsManager( ProjectServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
    }

    internal void Initialize(
        ImmutableArray<IHierarchicalOptionsSource> sources,
        IExternalHierarchicalOptionsProvider? externalOptionsProvider,
        CompilationModel compilationModel,
        IDiagnosticAdder diagnosticAdder )
    {
        if ( externalOptionsProvider != null )
        {
            this._externalOptionsProvider = externalOptionsProvider;

            // We have to create OptionType nodes now while we have an IDiagnosticAdder. 
            foreach ( var optionType in externalOptionsProvider.GetOptionTypes() )
            {
                _ = this.GetOptionTypeNode( compilationModel, diagnosticAdder, optionType );
            }
        }

        foreach ( var source in sources )
        {
            this.AddSource( source, compilationModel, diagnosticAdder );
        }
    }

    internal void AddSource( IHierarchicalOptionsSource source, CompilationModel compilationModel, IDiagnosticAdder diagnosticAdder )
    {
        foreach ( var configurator in source.GetOptions( compilationModel, diagnosticAdder ) )
        {
            var optionTypeName = configurator.Options.GetType().FullName.AssertNotNull();

            var optionTypeNode = this.GetOptionTypeNode( compilationModel, diagnosticAdder, optionTypeName );

            optionTypeNode.AddOptionsInstance( configurator, diagnosticAdder );
        }
    }

    private OptionTypeNode GetOptionTypeNode( CompilationModel compilationModel, IDiagnosticAdder diagnosticAdder, string optionTypeName )
    {
        if ( !this._optionTypes.TryGetValue( optionTypeName, out var optionTypeNode ) )
        {
            // We get the type resolver lazily because several tests do not supply it.
            this._typeResolver = this._serviceProvider.GetRequiredService<ProjectSpecificCompileTimeTypeResolver>();

            var optionType =
                this._typeResolver.GetCompileTimeType(
                        compilationModel.Factory.GetTypeByReflectionName( optionTypeName ).GetSymbol().AssertNotNull(),
                        false )
                    .AssertNotNull();

            optionTypeNode = this._optionTypes.GetOrAdd(
                optionTypeName,
                _ => new OptionTypeNode( this, optionType, diagnosticAdder, this.GetDefaultOptions( optionType, compilationModel.Project ) ) );
        }

        return optionTypeNode;
    }

    internal IHierarchicalOptions GetOptions( IDeclaration declaration, Type optionsType )
    {
        if ( this._optionTypes.TryGetValue( optionsType.FullName.AssertNotNull(), out var node ) )
        {
            return node.GetOptions( declaration ).AssertNotNull();
        }
        else
        {
            return this.GetDefaultOptions( optionsType, declaration.Compilation.Project );
        }
    }

    public TOptions GetOptions<TOptions>( IDeclaration declaration )
        where TOptions : class, IHierarchicalOptions, new()
        => (TOptions) this.GetOptions( declaration, typeof(TOptions) );

    internal static TOptions GetOptions<TOptions>( IAspectInstance aspectInstance )
        where TOptions : class, IHierarchicalOptions, new()
    {
        // We require a UserCodeExecutionContext to execute this method because requiring an explicit ICompilation would
        // make the API cumbersome. In case of need we can have another overload accepting the ICompilation.
        var compilation = UserCodeExecutionContext.Current.Compilation.AssertNotNull();

        var declaration = aspectInstance.TargetDeclaration.GetTarget( compilation );

        var inheritedOptions =
            declaration.GetCompilationModel().HierarchicalOptionsManager.GetOptions<TOptions>( declaration );

        if ( aspectInstance.Aspect is IHierarchicalOptionsProvider<TOptions> hierarchicalOptionsProvider )
        {
            var aspectOptions = hierarchicalOptionsProvider.GetOptions();

            var combinedOptions = inheritedOptions.OverrideWithSafe(
                    aspectOptions,
                    new OverrideContext( OverrideAxis.Aspect, declaration ) )
                .AssertNotNull();

            return combinedOptions;
        }
        else
        {
            return inheritedOptions;
        }
    }

    private IHierarchicalOptions GetDefaultOptions( Type type, IProject project )
    {
        var optionTypeName = type.FullName.AssertNotNull();

        if ( !this._defaultOptions.TryGetValue( optionTypeName, out var options ) )
        {
            var empty = (IHierarchicalOptions) FormatterServices.GetUninitializedObject( type ).AssertNotNull();

            options = empty.GetDefaultOptions( project )
                      ?? throw new ArgumentNullException( $"{type.Name}.{nameof(empty.GetDefaultOptions)} returned null." );

            this._defaultOptions.TryAdd( optionTypeName, options );
        }

        return options;
    }

    public IEnumerable<KeyValuePair<HierarchicalOptionsKey, IHierarchicalOptions>>
        GetInheritableOptions( ICompilation compilation, bool withSyntaxTree )
        => this._optionTypes.Where( s => s.Value.Metadata is { InheritedByDerivedTypes: true } or { InheritedByOverridingMembers: true } )
            .SelectMany( s => s.Value.GetInheritableOptions( compilation, withSyntaxTree ) );
}