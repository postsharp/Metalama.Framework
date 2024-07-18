// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using System;

namespace Metalama.Framework.Engine.CompileTime.Serialization;

internal sealed class CompileTimeCompileTimeSerializationBinder : CompileTimeSerializationBinder
{
    private readonly CompileTimeProject _project;
    private static readonly string _systemAssemblyName = typeof(object).Assembly.FullName.AssertNotNull();

    public CompileTimeCompileTimeSerializationBinder( in ProjectServiceProvider serviceProvider, CompileTimeProject project ) : base( serviceProvider )
    {
        this._project = project;
    }

    public override Type? BindToType( string typeName, string assemblyName )
    {
        if ( assemblyName.Equals( "mscorlib", StringComparison.Ordinal )
             || assemblyName.Equals( "System.Private.CoreLib", StringComparison.Ordinal ) )
        {
            // We have a reference to a system assembly, which is different under .NET Framework and .NET Core.
            // Replace by the current system assembly.
            assemblyName = _systemAssemblyName;
        }

        if ( CompileTimeCompilationBuilder.IsCompileTimeAssemblyName( assemblyName ) )
        {
            return this._project.GetType( typeName, assemblyName );
        }
        else
        {
            return base.BindToType( typeName, assemblyName );
        }
    }
}