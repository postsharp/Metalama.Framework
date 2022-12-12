// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable 8618 // Property Id not initialized.

namespace Metalama.Framework.Engine.Formatting
{
    public class DiagnosticAnnotation
    {
        [JsonConstructor]
        public DiagnosticAnnotation() { }

        public DiagnosticAnnotation( Diagnostic diagnostic )
        {
            this.Id = diagnostic.Id;
            this.Message = diagnostic.GetMessage( CultureInfo.CurrentCulture );
            this.Severity = diagnostic.Severity;
        }

        public string Id { get; init; }

        public DiagnosticSeverity Severity { get; init; }

        public string Message { get; init; }

        public string ToJson() => JsonConvert.SerializeObject( this );

        public static DiagnosticAnnotation FromJson( string json ) => JsonConvert.DeserializeObject<DiagnosticAnnotation>( json )!;

        public override string ToString() => $"{this.Severity} {this.Id}: {this.Message}";
    }
}