// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CompileTime;
using System;

namespace Metalama.Framework.Engine.LamaSerialization;

internal class CompileTimeLamaSerializationBinder : LamaSerializationBinder
{
    private readonly CompileTimeProject _project;
    private static readonly string _systemAssemblyName = typeof(object).Assembly.FullName;

    public CompileTimeLamaSerializationBinder( CompileTimeProject project )
    {
        this._project = project;
    }

    public override Type BindToType( string typeName, string assemblyName )
    {
        if ( assemblyName.StartsWith( "mscorlib,", StringComparison.Ordinal )
             || assemblyName.StartsWith( "System.Private.CoreLib,", StringComparison.Ordinal ) )
        {
            // We have a reference to a system assembly, which is different under .NET Framework and .NET Core.
            // Replace by the current system assembly.
            assemblyName = _systemAssemblyName;
        }
        
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