// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Introspection;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Introspection;

public class IntrospectionCompiler
{
    private readonly ProjectServiceProvider _serviceProvider;
    private readonly CompileTimeDomain _domain;
    private readonly IIntrospectionOptionsProvider? _options;

    public IntrospectionCompiler( ProjectServiceProvider serviceProvider, CompileTimeDomain domain, IIntrospectionOptionsProvider? options = null )
    {
        this._serviceProvider = serviceProvider;
        this._domain = domain;
        this._options = options;
    }

    public async Task<IIntrospectionCompilationResult> CompileAsync( ICompilation compilation )
    {
        var compilationModel = (CompilationModel) compilation;
        var pipeline = new IntrospectionAspectPipeline( this._serviceProvider, this._domain, this._options );

        return await pipeline.ExecuteAsync( compilationModel, TestableCancellationToken.None );
    }
}