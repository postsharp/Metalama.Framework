// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Caravela.Framework.Impl.DesignTime.UserDiagnostics
{
    /// <summary>
    /// A JSON-serializable file that contains user-defined diagnostic and suppression descriptors.
    /// </summary>
    internal class UserDiagnosticRegistrationFile
    {
        public Dictionary<string, UserDiagnosticRegistration> Diagnostics { get; } = new( StringComparer.OrdinalIgnoreCase );

        public HashSet<string> Suppressions { get; } = new( StringComparer.OrdinalIgnoreCase );

        [JsonIgnore]
        public DateTime Timestamp { get; private set; }

        public void Write( string file )
        {
            var serializer = JsonSerializer.Create();
            serializer.Formatting = Formatting.Indented;
            var textWriter = new StringWriter();
            serializer.Serialize( textWriter, this );

            RetryHelper.Retry( () => File.WriteAllText( file, textWriter.ToString() ) );
            this.Timestamp = DateTime.UtcNow;
        }

        public static UserDiagnosticRegistrationFile Read( string file )
        {
            if ( !File.Exists( file ) )
            {
                // Return an empty file.
                return new UserDiagnosticRegistrationFile();
            }

            try
            {
                var json = File.ReadAllText( file );
                JsonSerializer serializer = JsonSerializer.Create();

                var timestamp = File.GetLastWriteTimeUtc( file );

                var obj = serializer.Deserialize<UserDiagnosticRegistrationFile>( new JsonTextReader( new StringReader( json ) ) )
                          ?? new UserDiagnosticRegistrationFile();

                obj.Timestamp = timestamp;

                return obj;
            }
            catch
            {
                // Return an empty file.
                return new UserDiagnosticRegistrationFile();
            }
        }
    }
}