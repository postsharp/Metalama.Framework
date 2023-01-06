﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.VisualStudio.Remoting.AnalysisProcess;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

internal sealed class DistributedDesignTimeTestContext : TestContext
{
    private UserProcessServiceHubEndpoint? _userProcessServiceHubEndpoint;
    private AnalysisProcessServiceHubEndpoint? _analysisProcessServiceHubEndpoint;
    private AnalysisProcessEndpoint? _analysisProcessEndpoint;
    private TestDesignTimeAspectPipelineFactory? _pipelineFactory;
    private Task? _whenInitialized;

    public DistributedDesignTimeTestContext( TestContextOptions contextOptions, IAdditionalServiceCollection additionalServices ) : base(
        contextOptions,
        additionalServices )
    {
        this.WorkspaceProvider = this.ServiceProvider.Global.GetRequiredService<TestWorkspaceProvider>();
    }

    public Task InitializeAsync( ServiceProviderBuilder<IGlobalService>? userProcessServices, ServiceProviderBuilder<IGlobalService>? analysisProcessServices )
    {
        var analysisProcessServiceProvider = this.ServiceProvider.Global;

        this._pipelineFactory = new TestDesignTimeAspectPipelineFactory( this, analysisProcessServiceProvider );
        analysisProcessServiceProvider = this._pipelineFactory.ServiceProvider;

        if ( analysisProcessServices != null )
        {
            analysisProcessServiceProvider = analysisProcessServices.Build( analysisProcessServiceProvider );
        }

        var userProcessServiceProvider = this.ServiceProvider.Global;

        if ( userProcessServices != null )
        {
            userProcessServiceProvider = userProcessServices.Build( userProcessServiceProvider );
        }

        // Start the hub service on both ends.
        var testGuid = Guid.NewGuid();
        var hubPipeName = $"Metalama_Hub_{testGuid}";
        var servicePipeName = $"Metalama_Analysis_{testGuid}";

        this._userProcessServiceHubEndpoint = new UserProcessServiceHubEndpoint( userProcessServiceProvider, hubPipeName );
        this._userProcessServiceHubEndpoint.Start();
        this._analysisProcessServiceHubEndpoint = new AnalysisProcessServiceHubEndpoint( analysisProcessServiceProvider, hubPipeName );
        var connectAnalysisProcessTask = this._analysisProcessServiceHubEndpoint.ConnectAsync(); // Do not await so we get more randomness.

        // Start the main services on both ends.
        this._analysisProcessEndpoint = new AnalysisProcessEndpoint(
            analysisProcessServiceProvider.WithService( this._analysisProcessServiceHubEndpoint ),
            servicePipeName );

        this._analysisProcessEndpoint.Start();

        this.UserProcessServiceProvider = userProcessServiceProvider.WithService( this._userProcessServiceHubEndpoint );

        this._whenInitialized = Task.WhenAll(
            this._userProcessServiceHubEndpoint.WaitUntilInitializedAsync( "Test", this.CancellationToken ).AsTask(),
            this._analysisProcessServiceHubEndpoint.WaitUntilInitializedAsync( "Test", this.CancellationToken ).AsTask(),
            connectAnalysisProcessTask );

        return this.WhenInitialized;
    }

    public Task WhenInitialized => this._whenInitialized ?? throw new InvalidOperationException();

    public TestWorkspaceProvider WorkspaceProvider { get; }

    public GlobalServiceProvider UserProcessServiceProvider { get; private set; }

    public UserProcessServiceHubEndpoint UserProcessServiceHubEndpoint => this._userProcessServiceHubEndpoint ?? throw new InvalidOperationException();

    public AnalysisProcessEndpoint AnalysisProcessEndpoint => this._analysisProcessEndpoint ?? throw new InvalidOperationException();

    public TestDesignTimeAspectPipelineFactory PipelineFactory => this._pipelineFactory ?? throw new InvalidOperationException();

    public override void Dispose()
    {
        this._userProcessServiceHubEndpoint?.Dispose();
        this._analysisProcessServiceHubEndpoint?.Dispose();
        this._analysisProcessEndpoint?.Dispose();
        base.Dispose();
    }
}