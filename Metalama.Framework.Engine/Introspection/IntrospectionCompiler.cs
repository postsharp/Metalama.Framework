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
    private readonly CompileTimeDomain _domain;
    private readonly bool _isTest;
    private readonly IIntrospectionOptionsProvider? _options;

    public IntrospectionCompiler( CompileTimeDomain domain, IIntrospectionOptionsProvider? options = null ) : this( domain, false, options ) { }

    internal IntrospectionCompiler( CompileTimeDomain domain, bool isTest, IIntrospectionOptionsProvider? options = null )
    {
        this._domain = domain;
        this._isTest = isTest;
        this._options = options;
    }

    public async Task<IIntrospectionCompilationResult> CompileAsync( ICompilation compilation, ServiceProvider serviceProvider )
    {
        var compilationModel = (CompilationModel) compilation;
        var pipeline = new IntrospectionAspectPipeline( serviceProvider, this._domain, this._isTest, this._options );

        return await pipeline.ExecuteAsync( compilationModel, TestableCancellationToken.None );
    }
}