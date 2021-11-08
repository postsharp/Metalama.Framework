using Spectre.Console;
using System.CommandLine.IO;
using System.IO;
using System.Text;

namespace PostSharp.Engineering.BuildTools.Console
{
    internal class AnsiConsoleOutputWrapper : TextWriter, IAnsiConsoleOutput
    {
        private readonly IStandardStreamWriter _underlying;

        public AnsiConsoleOutputWrapper( IStandardStreamWriter underlying )
        {
            this._underlying = underlying;
        }

        void IAnsiConsoleOutput.SetEncoding( Encoding encoding ) { }

        TextWriter IAnsiConsoleOutput.Writer => this;

        public bool IsTerminal => true;

        public int Width => System.Console.WindowWidth;

        public int Height => System.Console.WindowHeight;

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write( char value ) => this._underlying.Write( new string(  value, 1 ) );
        public override void Write( string? value )
        {
            if ( value != null )
            {
                this._underlying.Write( value );
            }
        }
    }
}