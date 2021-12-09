// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Metalama.Framework.Engine.DesignTime.Diagnostics
{
    /// <summary>
    /// A JSON-serializable file that contains user-defined diagnostic and suppression descriptors.
    /// </summary>
    [Obfuscation( Exclude = true /* JSON */ )]
    internal class UserDiagnosticRegistrationFile
    {
        public Dictionary<string, UserDiagnosticRegistration> Diagnostics { get; } = new( StringComparer.OrdinalIgnoreCase );

        public HashSet<string> Suppressions { get; } = new( StringComparer.OrdinalIgnoreCase );

        [JsonIgnore]
        public DateTime Timestamp { get; private set; }

        public void Write( string file )
        {
            var textWriter = new StringWriter();
            this.Write( textWriter );
            RetryHelper.Retry( () => File.WriteAllText( file, textWriter.ToString() ) );
        }

        public void Write( TextWriter textWriter )
        {
            var serializer = JsonSerializer.Create();
            serializer.Formatting = Newtonsoft.Json.Formatting.Indented;

            serializer.Serialize( textWriter, this );

            this.Timestamp = DateTime.UtcNow;
        }

        public static UserDiagnosticRegistrationFile ReadFile( string file )
        {
            if ( !File.Exists( file ) )
            {
                // Return an empty file.
                return new UserDiagnosticRegistrationFile();
            }

            try
            {
                var json = File.ReadAllText( file );

                var timestamp = File.GetLastWriteTimeUtc( file );

                var obj = ReadContent( json );

                obj.Timestamp = timestamp;

                return obj;
            }
            catch
            {
                // Return an empty file.
                return new UserDiagnosticRegistrationFile();
            }
        }

        public static UserDiagnosticRegistrationFile ReadContent( string json )
        {
            var serializer = JsonSerializer.Create();

            return serializer.Deserialize<UserDiagnosticRegistrationFile>( new JsonTextReader( new StringReader( json ) ) )
                   ?? new UserDiagnosticRegistrationFile();
        }
    }
}