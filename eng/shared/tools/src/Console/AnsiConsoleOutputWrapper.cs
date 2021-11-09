using Spectre.Console;
using System.IO;
using System.Text;

namespace PostSharp.Engineering.BuildTools.Console
{
    internal class AnsiConsoleOutputWrapper : IAnsiConsoleOutput
    {
        private readonly TextWriter _underlying;

        public AnsiConsoleOutputWrapper( TextWriter underlying )
        {
            this._underlying = underlying;
        }

        void IAnsiConsoleOutput.SetEncoding( Encoding encoding ) { }

        TextWriter IAnsiConsoleOutput.Writer => this._underlying;

        public bool IsTerminal => true;

        public int Width => System.Console.WindowWidth;

        public int Height => System.Console.WindowHeight;
    }
}