using PostSharp.Engineering.BuildTools.Build;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Diagnostics;
using System.Linq;

#pragma warning disable 8765

namespace PostSharp.Engineering.BuildTools
{
    public abstract class BaseCommand<T> : Command<T>
        where T : BaseCommandSettings
    {
    
        public sealed override int Execute( CommandContext context, T settings )
        {
            try
            {
                
                var stopwatch = Stopwatch.StartNew();

                if ( !BuildContext.TryCreate( context, out var buildContext ) )
                {
                    return 1;
                }
                else
                {
                    if ( settings.ListProperties )
                    {
                        if ( buildContext.Product.SupportedProperties.Count > 0 )
                        {
                            buildContext.Console.WriteImportantMessage( "The following properties are supported by this product:" );
                            foreach ( var property in buildContext.Product.SupportedProperties )
                            {
                                buildContext.Console.WriteImportantMessage( $"\t{property.Key}: {property.Value}" );
                            }
                        }
                        else
                        {
                            buildContext.Console.WriteImportantMessage( "The current product does not support any property." );
                        }

                        return 1;
                    }

                    var unsupportedProperties = settings.Properties.Keys.Where( name => !buildContext.Product.SupportedProperties.ContainsKey( name ) ).ToList();
                    if ( unsupportedProperties.Count > 0 )
                    {
                        buildContext.Console.WriteError( $"The following properties are not supported: {string.Join( ", ", unsupportedProperties)}. Use --list-properties to list the supported properties." );
                        return 1;
                    }


                    buildContext.Console.Out.Write( new FigletText( buildContext.Product.ProductName )
                        .LeftAligned()
                        .Color( Color.Purple ) );


                    var success = this.ExecuteCore( buildContext, settings );

                    buildContext.Console.WriteMessage( $"Finished at {DateTime.Now} after {stopwatch.Elapsed}." );

                    return success ? 0 : 1;
                }
            }
            catch ( Exception ex )
            {
                AnsiConsole.WriteException( ex );
                return 10;
            }
        }

        protected abstract bool ExecuteCore( BuildContext context, T options );
    }
}