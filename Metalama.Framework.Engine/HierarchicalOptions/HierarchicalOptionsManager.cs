// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Options;
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
    private readonly ProjectServiceProvider _serviceProvider;
    private readonly UserCodeInvoker _userCodeInvoker;
    private IExternalHierarchicalOptionsProvider? _externalOptionsProvider;

    private ProjectSpecificCompileTimeTypeResolver? _typeResolver;

    internal HierarchicalOptionsManager( ProjectServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
        this._userCodeInvoker = serviceProvider.GetRequiredService<UserCodeInvoker>();
    }

    private Type GetOptionType( string typeName, CompilationModel compilationModel )
    {
        // We get the type resolver lazily because several tests do not supply it.

        this._typeResolver ??= this._serviceProvider.GetRequiredService<ProjectSpecificCompileTimeTypeResolver>();

        return
            this._typeResolver.GetCompileTimeType(
                    compilationModel.Factory.GetTypeByReflectionName( typeName ).GetSymbol().AssertNotNull(),
                    false )
                .AssertNotNull();
    }

    internal void Initialize(
        CompileTimeProject project,
        ImmutableArray<IHierarchicalOptionsSource> sources,
        IExternalHierarchicalOptionsProvider? externalOptionsProvider,
        CompilationModel compilationModel,
        IUserDiagnosticSink diagnosticSink )
    {
        // Initialize all default options. We need to do this during initialization because we need a diagnostic sink and won't have it later.

        foreach ( var optionTypeName in project.ClosureOptionTypes )
        {
            var optionType = this.GetOptionType( optionTypeName, compilationModel );

            var (defaultOptions, emptyOptions) = ComputeDefaultOptions( optionType, compilationModel, this._userCodeInvoker, this._serviceProvider, diagnosticSink );

            this._optionTypes.TryAdd(
                optionTypeName,
                new OptionTypeNode( this, optionType, diagnosticSink, defaultOptions, emptyOptions ) );
        }

        if ( externalOptionsProvider != null )
        {
            this._externalOptionsProvider = externalOptionsProvider;

            // We have to create OptionType nodes now while we have an IDiagnosticAdder. 
            foreach ( var optionType in externalOptionsProvider.GetOptionTypes() )
            {
                _ = this.GetOptionTypeNode( optionType );
            }
        }

        foreach ( var source in sources )
        {
            this.AddSource( source, compilationModel, diagnosticSink );
        }
    }

    internal static (IHierarchicalOptions DefaultOptions, IHierarchicalOptions EmptyOptions) ComputeDefaultOptions(
        Type optionType,
        CompilationModel compilationModel,
        UserCodeInvoker userCodeInvoker,
        ProjectServiceProvider serviceProvider,
        IUserDiagnosticSink diagnosticSink )
    {
        var userCodeExecutionContext = new UserCodeExecutionContext(
            serviceProvider,
            diagnosticSink,
            UserCodeDescription.Create( "Initializing options '{0}'", optionType ),
            compilationModel: compilationModel );

        if ( !userCodeInvoker.TryInvoke(
                () => (IHierarchicalOptions) Activator.CreateInstance( optionType ).AssertNotNull(),
                userCodeExecutionContext,
                out var emptyOptions ) )
        {
            // If we fail to construct the options, we will continue with the non-initialized options.
            emptyOptions = (IHierarchicalOptions) FormatterServices.GetUninitializedObject( optionType ).AssertNotNull();
        }

        var context = new OptionsInitializationContext(
            compilationModel.Project,
            new ScopedDiagnosticSink( diagnosticSink, new GetDefaultOptionsDiagnosticSourceDescription( optionType ), null, null ) );

        if ( !userCodeInvoker.TryInvoke(
                () => emptyOptions!.GetDefaultOptions( context ),
                userCodeExecutionContext,
                out var defaultOptions ) )
        {
            // If we fail to get the default options, we will continue with the non-initialized options.
            defaultOptions = emptyOptions;
        }

        return (defaultOptions!, emptyOptions!);
    }

    internal void AddSource( IHierarchicalOptionsSource source, CompilationModel compilationModel, IUserDiagnosticSink diagnosticSink )
    {
        foreach ( var configurator in source.GetOptions( compilationModel, diagnosticSink ) )
        {
            var optionTypeName = configurator.Options.GetType().FullName.AssertNotNull();

            var optionTypeNode = this.GetOptionTypeNode( optionTypeName );

            optionTypeNode.AddOptionsInstance( configurator, diagnosticSink );
        }
    }

    private OptionTypeNode GetOptionTypeNode( string optionTypeName )
    {
        if ( !this._optionTypes.TryGetValue( optionTypeName, out var optionTypeNode ) )
        {
            throw new AssertionFailedException( $"The option type '{optionTypeName}' is not a part of the current project." );
        }

        return optionTypeNode;
    }

    internal IHierarchicalOptions GetOptions( IDeclaration declaration, Type optionsType )
    {
        var optionTypeNode = this.GetOptionTypeNode( optionsType.FullName.AssertNotNull() );

        return optionTypeNode.GetOptions( declaration ).AssertNotNull();
    }

    public TOptions GetOptions<TOptions>( IDeclaration declaration )
        where TOptions : class, IHierarchicalOptions, new()
        => (TOptions) this.GetOptions( declaration, typeof(TOptions) );

    public IEnumerable<KeyValuePair<HierarchicalOptionsKey, IHierarchicalOptions>>
        GetInheritableOptions( ICompilation compilation, bool withSyntaxTree )
        => this._optionTypes.Where( s => s.Value.Metadata is { InheritedByDerivedTypes: true } or { InheritedByOverridingMembers: true } )
            .SelectMany( s => s.Value.GetInheritableOptions( compilation, withSyntaxTree ) );

    private sealed class GetDefaultOptionsDiagnosticSourceDescription : IDiagnosticSource
    {
        private readonly Type _type;

        public GetDefaultOptionsDiagnosticSourceDescription( Type type )
        {
            this._type = type;
        }

        public string DiagnosticSourceDescription => $"{this._type.Name}.GetDefaultOptions";
    }

    public void SetAspectOptions( IDeclaration declaration, IHierarchicalOptions options )
    {
        this.GetOptionTypeNode( options.GetType().FullName.AssertNotNull() ).SetAspectOptions( declaration, options );
    }
}