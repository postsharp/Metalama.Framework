// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Maintenance;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.Versioning;

namespace Metalama.Framework.Engine.CompileTime;

internal sealed class OutputPathHelper
{
    private readonly ITempFileManager _tempFileManager;

    public const int MaxOutputFilenameLength = 24;

    public OutputPathHelper( ITempFileManager tempFileManager )
    {
        this._tempFileManager = tempFileManager;
    }

    public OutputPaths GetOutputPaths( string runTimeAssemblyName, FrameworkName? targetFramework, ulong projectHash )
    {
        if ( runTimeAssemblyName.StartsWith( CompileTimeCompilationBuilder.CompileTimeAssemblyPrefix, StringComparison.Ordinal ) )
        {
            throw new ArgumentOutOfRangeException( nameof(runTimeAssemblyName) );
        }

        // Note: we must generate file paths that are smaller than 256 characters.

        // Get a shorter name for the target framework.
        string targetFrameworkName;

        if ( targetFramework != null )
        {
            targetFrameworkName = "";

            var splitByComma = targetFramework.FullName.Split( ',' );

            for ( var partIndex = 0; partIndex < splitByComma.Length; partIndex++ )
            {
                var namePart = splitByComma[partIndex];
                var splitByEqual = namePart.Split( '=' );

                string part;

                if ( splitByEqual.Length == 1 )
                {
                    part = splitByEqual[0].ToLowerInvariant();
                }
                else
                {
                    part = splitByEqual[1].ToLowerInvariant();
                }

                if ( partIndex == 1 )
                {
                    part = part.TrimStart( 'v' );
                }

                if ( partIndex > 1 )
                {
                    targetFrameworkName += "-";
                }

                targetFrameworkName += part;
            }
        }
        else
        {
            targetFrameworkName = "unspecified";
        }

        // Get a shorter assembly name.
        string shortAssemblyName;

        if ( runTimeAssemblyName.Length > 32 )
        {
            // It does not matter if we put several assemblies in the same directory because we include a unique hash of the full name in a subdirectory anyway.
            shortAssemblyName = runTimeAssemblyName.Substring( 0, 32 );
        }
        else
        {
            shortAssemblyName = runTimeAssemblyName;
        }

        // Get the directory name.
        var hash = projectHash.ToString( "x16", CultureInfo.InvariantCulture );

        var directory = this._tempFileManager.GetTempDirectory(
            Path.Combine( "CompileTime", shortAssemblyName, targetFrameworkName, hash ),
            CleanUpStrategy.WhenUnused );

        // Make sure that the base path is short enough. There should be 16 characters left.
        var remainingPathLength = 256 - directory.Length;

        if ( remainingPathLength < MaxOutputFilenameLength )
        {
            throw new InvalidOperationException( $"The temporary path '{directory}' is too long." );
        }

        var baseCompileTimeAssemblyName = $"{CompileTimeCompilationBuilder.CompileTimeAssemblyPrefix}{runTimeAssemblyName}";
        var maxLength = remainingPathLength - 16 /* hash */ - 1 /* _ */ - 4 /* .dll */ - 1 /* backslash */ - 1 /* end of string */;

        if ( baseCompileTimeAssemblyName.Length > maxLength )
        {
            baseCompileTimeAssemblyName = baseCompileTimeAssemblyName.Substring( 0, maxLength );
        }

        var compileTimeAssemblyName = baseCompileTimeAssemblyName + "_" + hash;

        var outputPaths = new OutputPaths( directory, compileTimeAssemblyName, null );

        if ( outputPaths.Pe.Length > 255 )
        {
            throw new AssertionFailedException( $"The path '{outputPaths.Pe}' is too long: {outputPaths.Pe.Length} characters." );
        }

        if ( outputPaths.Manifest.Length > 255 )
        {
            throw new AssertionFailedException( $"The path '{outputPaths.Manifest}' is too long: {outputPaths.Manifest.Length} characters." );
        }

        return outputPaths;
    }
}