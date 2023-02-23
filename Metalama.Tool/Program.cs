﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.Engine.Configuration;
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
                            "credits",
                            credits =>
                            {
                                credits.SetDescription( "Analyzes license credits required to build your projects." );

                                credits.AddCommand<PrintTotalCreditsCommand>( "summary" )
                                    .WithData( options )
                                    .WithDescription( "Prints an overall summary of required credits for recent builds." );

                                credits.AddCommand<PrintProjectCreditsCommand>( "projects" )
                                    .WithData( options )
                                    .WithDescription( "Prints a by-project summary of required credits for recent builds." );

                                credits.AddCommand<PrintCreditDetailsCommand>( "details" )
                                    .WithData( options )
                                    .WithDescription( "Prints the detail of how aspect classes or libraries consumed credits during recent builds." );

                                credits.AddCommand<ResetCreditsCommands>( "reset" )
                                    .WithData( options )
                                    .WithDescription( "Resets the license consumption data gathered for the past builds." );
                            } );

                        break;
                }
            }
        }
    }
}