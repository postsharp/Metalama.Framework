// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Options;
using Caravela.Framework.Project;
using System;
using System.Collections.Immutable;
using System.IO;

namespace Caravela.Framework.Impl.Pipeline
{
    internal class AuxiliaryFileProvider : IAuxiliaryFileProvider
    {
        private readonly ServiceProvider _serviceProvider;

        public AuxiliaryFileProvider( ServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
        }

        public ImmutableArray<AuxiliaryFile> GetAuxiliaryFiles()
        {
            var projectOptions = this._serviceProvider.GetOptionalService<IProjectOptions>();

            if ( projectOptions == null || projectOptions.AuxiliaryFilePath == null )
            {
                return ImmutableArray<AuxiliaryFile>.Empty;
            }

            var builder = ImmutableArray<AuxiliaryFile>.Empty.ToBuilder();

            foreach ( var kindDirectory in Directory.GetDirectories( projectOptions.AuxiliaryFilePath ) )
            {
                if ( !Enum.TryParse<AuxiliaryFileKind>( Path.GetFileName( kindDirectory ), out var kind ) )
                {
                    continue;
                }

                var kindDirectoryNormalized = Path.GetFullPath( kindDirectory );

                foreach ( var file in Directory.GetFiles( kindDirectory, "*", SearchOption.AllDirectories ) )
                {
                    // TODO: This is probably not reliable.
                    var fileNormalized = Path.GetFullPath( file );
                    var relativePath = fileNormalized.Substring( kindDirectoryNormalized.Length + 1 );

                    builder.Add( new ExistingAuxiliaryFile( projectOptions.AuxiliaryFilePath, kind, relativePath ) );
                }
            }

            return builder.ToImmutable();
        }
    }
}