// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Utilities;
using System;
using System.IO;

namespace Metalama.Framework.Engine.Utilities
{
    internal static class GlobalJsonWriter
    {
        /// <summary>
        /// Writes a global.json file to the specified directory.
        /// The file sets the .NET SDK version to the same as is used in the current environment. 
        /// </summary>
        /// <param name="targetDirectory">The directory where the globals.json file is written to.</param>
        /// <remarks>
        /// When the dotnet.exe command is executed from within the build process, certain .NET SDK version specific
        /// environment variables are passed to the new process. If the child process attempts to use
        /// a different .NET SDK version than the parent process, these environment variables could break
        /// the executed command. 
        /// </remarks>
        public static void WriteCurrentVersion( string targetDirectory )
        {
            var dotNetSdkDirectory = Environment.GetEnvironmentVariable( "MSBuildExtensionsPath" );

            if ( string.IsNullOrEmpty( dotNetSdkDirectory ) )
            {
                throw new InvalidOperationException( "The MSBuildExtensionsPath is undefined." );
            }

            var dotNetSdkVersion = PlatformUtilities.GetDotNetSdkVersion();

            var globalJsonText =
                $@"{{
  ""sdk"": {{
    ""version"": ""{dotNetSdkVersion}"",
    ""rollForward"": ""disable""
  }}
}}";

            File.WriteAllText( Path.Combine( targetDirectory, "global.json" ), globalJsonText );
        }
    }
}