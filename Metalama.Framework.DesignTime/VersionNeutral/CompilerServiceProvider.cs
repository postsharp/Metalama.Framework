// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.EntryPoint;
using Metalama.Framework.DesignTime.Contracts.Pipeline;
using Metalama.Framework.Engine.Services;
using System.Collections.Concurrent;

namespace Metalama.Framework.DesignTime.VersionNeutral;

public class CompilerServiceProvider : ICompilerServiceProvider
{
    private readonly ConcurrentDictionary<string, ICompilerService?> _services = new( StringComparer.Ordinal );
    private GlobalServiceProvider? _serviceProvider;

    public CompilerServiceProvider( ContractVersion[]? contractVersions = null )
    {
        this.ContractVersions = contractVersions ?? CurrentContractVersions.All;
        this.Version = this.GetType().Assembly.GetName().Version!;
    }

    public Version Version { get; }

    public ContractVersion[] ContractVersions { get; }

    public GlobalServiceProvider ServiceProvider
    {
        get => this._serviceProvider ?? throw new InvalidOperationException();
    }

    public void Initialize( GlobalServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
    }

    public ICompilerService? GetService( Type serviceType ) => this.GetService( serviceType.Name );

    private ICompilerService? GetService( string name ) => this._services.GetOrAdd( name, this.GetServiceCore );

    protected virtual ICompilerService? GetServiceCore( string name )
        => name switch
        {
            nameof(ITransitiveCompilationService) => new TransitiveCompilationService( this.ServiceProvider ),
            _ => null
        };
}