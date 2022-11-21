// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Introspection;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Introspection;

public class IntrospectionCompiler
{
    private readonly ServiceProvider _serviceProvider;
    private readonly CompileTimeDomain _domain;
    private readonly bool _isTest;
    private readonly IIntrospectionOptionsProvider? _options;

    public IntrospectionCompiler( CompileTimeDomain domain, ServiceProvider serviceProvider, IIntrospectionOptionsProvider? options = null ) : this(
        domain,
        serviceProvider,
        false,
        options ) { }

    internal IntrospectionCompiler( CompileTimeDomain domain, ServiceProvider serviceProvider, bool isTest, IIntrospectionOptionsProvider? options = null )
    {
        this._domain = domain;
        this._isTest = isTest;
        this._options = options;
        this._serviceProvider = serviceProvider;
    }

    public async Task<IIntrospectionCompilationResult> CompileAsync( ICompilation compilation )
    {
        var compilationModel = (CompilationModel) compilation;
        var pipeline = new IntrospectionAspectPipeline( this._serviceProvider, this._domain, this._isTest, this._options );

        return await pipeline.ExecuteAsync( compilationModel, TestableCancellationToken.None );
    }
}