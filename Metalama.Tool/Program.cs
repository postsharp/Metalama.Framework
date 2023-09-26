﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.Engine.Configuration;
using Metalama.Tool.Divorce;
using Metalama.Tool.Licensing;
using Spectre.Console.Cli;
using System.Threading.Tasks;

namespace Metalama.Tool
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
                ConfigureMoreCommands,
                ConfigureBranch );

            return await app.RunAsync( args );

            void ConfigureMoreCommands( IConfigurator config )
            {
                config.AddCommand<DivorceCommand>( "divorce" )
                    .WithData( options )
                    .WithDescription( "Copies code generated by Metalama back into your source code, in case you wanted to stop using Metalama." );

                config.AddCommand<VersionCommand>( "version" )
                    .WithData( options )
                    .WithDescription( "Displays the version of the 'metalama' global tool." );
            }

            void ConfigureBranch( string branch, IConfigurator<CommandSettings> builder )
            {
                switch ( branch )
                {
                    case "license":
                        builder.AddBranch(
                            "usage",
                            usage =>
                            {
                                usage.SetDescription( "Analyzes usage of license used to build your projects." );

                                usage.AddCommand<PrintTotalLicenseUsageCommand>( "summary" )
                                    .WithData( options )
                                    .WithDescription( "Prints an overall summary of aspect classes used in recently built projects." );

                                usage.AddCommand<PrintProjectLicenseUsageCommand>( "projects" )
                                    .WithData( options )
                                    .WithDescription( "Prints a by-project summary of aspect classes used in recently built projects." );

                                usage.AddCommand<PrintLicenseUsageDetailsCommand>( "details" )
                                    .WithData( options )
                                    .WithDescription( "Prints the list of aspect classes used in recently built projects." );

                                usage.AddCommand<ResetLicenseUsageCommands>( "reset" )
                                    .WithData( options )
                                    .WithDescription( "Resets the license consumption data gathered for the past builds." );
                            } );

                        break;
                }
            }
        }
    }
}