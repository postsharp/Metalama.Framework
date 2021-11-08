using Spectre.Console;
using System.CommandLine;

namespace PostSharp.Engineering.BuildTools.Console
{
    public class ConsoleHelper
    {
        public IAnsiConsole Out { get; }
        public IAnsiConsole Error { get; }

        public void WriteError( string format, params object[] args ) =>
            this.WriteError( string.Format( format, args ) );

        public void WriteError( string message )
        {
            this.Error.MarkupLine( $"[red]{message}[/]" );
        }

        public void WriteWarning( string message )
        {
            this.Out.MarkupLine( $"[warning]{message}[/]" );
        }

        public void WriteWarning( string format, params object[] args ) =>
            this.WriteWarning( string.Format( format, args ) );

        public void WriteMessage( string message )
        {
            this.Out.WriteLine( message );
        }

        public void WriteMessage( string format, params object[] args ) =>
            this.WriteMessage( string.Format( format, args ) );


        public void WriteImportantMessage( string message )
        {
            this.Out.MarkupLine( "[bold]" + message + "[/]" );
        }

        public void WriteImportantMessage( string format, params object[] args ) =>
            this.WriteImportantMessage( string.Format( format, args ) );


        public void WriteSuccess( string message )
        {
            this.Out.MarkupLine( $"[green]{message}[/]" );
        }

        public void WriteHeading( string message )
        {
            this.Out.MarkupLine( $"[bold underline cyan]{message}[/]" );
        }

        public ConsoleHelper( IConsole console )
        {
            var factory = new AnsiConsoleFactory();
            this.Out = factory.Create( new AnsiConsoleSettings()
            {
                Out = new AnsiConsoleOutputWrapper( console.Out )
            } );
            this.Error =
                factory.Create( new AnsiConsoleSettings() { Out = new AnsiConsoleOutputWrapper( console.Error ) } );
        }
    }
}