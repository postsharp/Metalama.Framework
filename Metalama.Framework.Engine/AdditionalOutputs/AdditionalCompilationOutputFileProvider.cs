// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Project;
using System;
using System.Collections.Immutable;
using System.IO;

namespace Metalama.Framework.Engine.AdditionalOutputs
{
    internal class AdditionalCompilationOutputFileProvider : IAdditionalOutputFileProvider
    {
        private readonly ServiceProvider _serviceProvider;

        public AdditionalCompilationOutputFileProvider( ServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
        }

        public ImmutableArray<AdditionalCompilationOutputFile> GetAdditionalCompilationOutputFiles()
        {
            var projectOptions = this._serviceProvider.GetOptionalService<IProjectOptions>();

            if ( projectOptions == null || projectOptions.AdditionalCompilationOutputDirectory == null )
            {
                return ImmutableArray<AdditionalCompilationOutputFile>.Empty;
            }

            var builder = ImmutableArray.CreateBuilder<AdditionalCompilationOutputFile>();

            foreach ( var kindDirectory in Directory.GetDirectories( projectOptions.AdditionalCompilationOutputDirectory ) )
            {
                if ( !Enum.TryParse<AdditionalCompilationOutputFileKind>( Path.GetFileName( kindDirectory ), out var kind ) )
                {
                    continue;
                }

                var kindDirectoryNormalized = Path.GetFullPath( kindDirectory );

                foreach ( var file in Directory.GetFiles( kindDirectory, "*", SearchOption.AllDirectories ) )
                {
                    // TODO: This is probably not reliable.
                    var fileNormalized = Path.GetFullPath( file );
                    var relativePath = fileNormalized.Substring( kindDirectoryNormalized.Length + 1 );

                    builder.Add( new ExistingAdditionalCompilationOutputFile( projectOptions.AdditionalCompilationOutputDirectory, kind, relativePath ) );
                }
            }

            return builder.ToImmutable();
        }
    }
}