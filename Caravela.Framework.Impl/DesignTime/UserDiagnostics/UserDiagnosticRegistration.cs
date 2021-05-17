// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.DesignTime.UserDiagnostics
{
    /// <summary>
    /// Represents a JSON-serializable user diagnostic for <see cref="UserDiagnosticRegistrationFile"/>.
    /// </summary>
    internal class UserDiagnosticRegistration
    {
        // Deserialization constructor.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private UserDiagnosticRegistration() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public UserDiagnosticRegistration( DiagnosticDescriptor descriptor )
        {
            this.Id = descriptor.Id;
            this.Severity = descriptor.DefaultSeverity;
            this.Category = descriptor.Category;
            this.Title = "A Caravela user diagnostic.";
            this.MessageFormat = "";
        }

        public DiagnosticDescriptor DiagnosticDescriptor() => new( this.Id, this.Title, this.MessageFormat, this.Category, this.Severity, true );

        /// <summary>
        /// Gets the severity of the diagnostic.
        /// </summary>
        public DiagnosticSeverity Severity { get; init; }

        /// <summary>
        /// Gets an unique identifier for the diagnostic (e.g. <c>MY001</c>).
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the formatting string of the diagnostic message.
        /// </summary>
        public string MessageFormat { get; init; }

        /// <summary>
        /// Gets the category of the diagnostic (e.g. your product name).
        /// </summary>
        public string Category { get; init; }

        /// <summary>
        /// Gets a short title describing the diagnostic. This title is typically described in the solution explorer of the IDE
        /// and does not contain formatting string parameters.
        /// </summary>
        public string Title { get; init; }
    }
}