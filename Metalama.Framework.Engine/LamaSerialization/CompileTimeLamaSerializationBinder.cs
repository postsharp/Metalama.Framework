using Metalama.Framework.Engine.CompileTime;
using System;

namespace Metalama.Framework.Engine.LamaSerialization;

internal class CompileTimeLamaSerializationBinder : LamaSerializationBinder
{
    private readonly CompileTimeProject _project;

    public CompileTimeLamaSerializationBinder( CompileTimeProject project )
    {
        this._project = project;
    }

    public override Type BindToType( string typeName, string assemblyName )
    {
        if ( CompileTimeCompilationBuilder.TryParseCompileTimeAssemblyName( assemblyName, out var runTimeAssemblyName ) )
        {
            return this._project.GetType( typeName, runTimeAssemblyName );
        }
        else
        {
            return base.BindToType( typeName, assemblyName );
        }
    }
}