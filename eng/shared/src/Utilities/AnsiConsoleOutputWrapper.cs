using Spectre.Console;
using System;
using System.IO;
using System.Text;

namespace PostSharp.Engineering.BuildTools.Utilities
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

        public int Width => Console.WindowWidth;

        public int Height => Console.WindowHeight;
    }
}