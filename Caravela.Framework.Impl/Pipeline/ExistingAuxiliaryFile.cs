// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;

namespace Caravela.Framework.Impl.Pipeline
{
    internal class ExistingAuxiliaryFile : AuxiliaryFile
    {
        private readonly string _auxiliaryFileDirectory;
        private readonly string _path;
        private readonly AuxiliaryFileKind _kind;
        private byte[]? _content;

        public override string Path => this._path;

        public override AuxiliaryFileKind Kind => this._kind;

        public override byte[] Content => this._content ?? this.LoadContent();

        public ExistingAuxiliaryFile( string auxiliaryFileDirectory, AuxiliaryFileKind kind, string path )
        {
            this._auxiliaryFileDirectory = auxiliaryFileDirectory;
            this._path = path;
            this._kind = kind;
        }

        private byte[] LoadContent()
        {
            var path = System.IO.Path.Combine( this._auxiliaryFileDirectory, this._kind.ToString(), this._path );

            this._content = File.ReadAllBytes( path );

            return this._content;
        }
    }
}