// Copyright (c) SharpCrafters s.r.o. All rights reserved.

using System.CommandLine;
using PostSharp.Engineering.BuildTools.MsBuild;
using PostSharp.Engineering.BuildTools.Nuget;

namespace PostSharp.Engineering.BuildTools
{
    internal class Program
    {
        private static int Main( string[] args )
        {
            var rootCommand = new RootCommand { };

            rootCommand.AddCommand( new MsBuildCommand() );
            rootCommand.AddCommand( new NuGetCommand() );

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync( args ).Result;
        }
    }
}
