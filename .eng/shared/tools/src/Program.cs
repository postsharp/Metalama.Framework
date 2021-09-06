// Copyright (c) SharpCrafters s.r.o. All rights reserved.

using PostSharp.Engineering.BuildTools.Coverage;
using PostSharp.Engineering.BuildTools.MsBuild;
using PostSharp.Engineering.BuildTools.Nuget;
using System.CommandLine;

namespace PostSharp.Engineering.BuildTools
{
    internal class Program
    {
        private static int Main( string[] args )
        {
            var rootCommand = new RootCommand { };

            rootCommand.AddCommand( new MsBuildCommand() );
            rootCommand.AddCommand( new NuGetCommand() );
            rootCommand.AddCommand( new CoverageCommand() );

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync( args ).Result;
        }
    }
}