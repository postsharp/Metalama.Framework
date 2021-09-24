// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace Caravela.Framework.Impl.Utilities
{
    internal static class GlobalJsonWriter
    {
        /// <summary>
        /// Tries to write a global.json file to the specified directory.
        /// The file sets the .NET SDK version to the same as is used in the current environment. 
        /// </summary>
        /// <param name="targetDirectory">The directory where the globals.json file is written to.</param>
        /// <returns><code>false</code> when the current .NET SDK version could not be determined.</returns>
        /// <remarks>
        /// When the dotnet.exe command is executed from withing a build process, certain .NET SDK version specific
        /// environment variables are passed to the new process. If the child process attempts to use
        /// a different .NET SDK version than the parent process, these environment variables could break
        /// the executed command. 
        /// </remarks>
        public static bool TryWriteCurrentVersion( string targetDirectory )
        {
            var dotNetSdkDirectory = Environment.GetEnvironmentVariable( "MSBuildExtensionsPath" );

            if ( string.IsNullOrEmpty( dotNetSdkDirectory ) )
            {
                return false;
            }

            var dotNetSdkVersion = Path.GetFileName( dotNetSdkDirectory.TrimEnd( Path.DirectorySeparatorChar ) );

            var globalJsonText =
                $@"{{
  ""sdk"": {{
    ""version"": ""{dotNetSdkVersion}"",
    ""rollForward"": ""disable""
  }}
}}";

            File.WriteAllText( Path.Combine( targetDirectory, "global.json" ), globalJsonText );

            return true;
        }
    }
}