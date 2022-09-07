// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.AdditionalOutputs;

namespace Metalama.Framework.DesignTime.Offline
{
    internal class ExistingAdditionalCompilationOutputFile : AdditionalCompilationOutputFile
    {
        private readonly string _additionalCompilationOutputDirectory;
        private readonly string _path;
        private readonly AdditionalCompilationOutputFileKind _kind;

        public override string Path => this._path;

        public override AdditionalCompilationOutputFileKind Kind => this._kind;

        public ExistingAdditionalCompilationOutputFile( string additionalCompilationOutputDirectory, AdditionalCompilationOutputFileKind kind, string path )
        {
            this._additionalCompilationOutputDirectory = additionalCompilationOutputDirectory;
            this._path = path;
            this._kind = kind;
        }

        public override Stream GetStream()
        {
            var path = System.IO.Path.Combine( this._additionalCompilationOutputDirectory, this._kind.ToString(), this._path );

            return File.OpenRead( path );
        }

        public override void WriteToStream( Stream stream )
        {
            throw new NotSupportedException();
        }
    }
}