// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.Engine.Configuration;

namespace Metalama.DotNetTools
{
    internal static class Program
    {
        private static async Task<int> Main( string[] args )
        {
            var app = new CommandApp();
            var options = new BackstageCommandOptions( new ApplicationInfo() );
            options.AddConfigurationFileAdapter<UserDiagnosticsConfiguration>();
            options.AddConfigurationFileAdapter<DesignTimeConfiguration>();

            BackstageCommandFactory.ConfigureCommandApp(
                app,
                options,
                config => config.AddCommand<VersionCommand>( "version" )
                    .WithData( options )
                    .WithDescription( "Displays the version of the 'metalama' global tool." ) );

            return await app.RunAsync( args );
        }
    }
}