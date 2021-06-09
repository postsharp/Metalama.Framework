// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Newtonsoft.Json;
using System.IO;

namespace Caravela.TestFramework
{
    public class TestDirectoryOptions : TestOptions
    {
        public bool? Include { get; set; }
        
        public bool? Exclude { get; set; }
        
        public static TestDirectoryOptions ReadFile( string path )
        {
            var json = File.ReadAllText( path );
            
            return JsonConvert.DeserializeObject<TestDirectoryOptions>( json )!;
        }

        public override void ApplyDirectoryOptions( TestDirectoryOptions directoryOptions )
        {
            base.ApplyDirectoryOptions( directoryOptions );

            if ( this.Include == null )
            {
                this.Include = directoryOptions.Include;
            }
        }
    }
}