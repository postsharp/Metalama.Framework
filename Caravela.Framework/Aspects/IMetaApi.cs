// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Syntax;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Validation;
using System.Collections.Generic;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Exposes information about the declaration to which a template was applied.
    /// This interface is exposed by the <see cref="meta"/> static type.
    /// </summary>
    [CompileTimeOnly]
    [InternalImplement]
    internal interface IMetaApi
    {
        /// <summary>
        /// Gets access to the declaration being overridden or introduced.
        /// </summary>
        IMetaTarget Target { get; }

        IMetaCodeBuilder CodeBuilder { get; }

        /// <summary>
        /// Gets an object that gives <c>dynamic</c> access to the instance members of the type. Equivalent to the <c>this</c> C# keyword.
        /// </summary>
        /// <seealso cref="Base"/>
        object This { get; }

        /// <summary>
        /// Gets an object that gives <c>dynamic</c> access to the instance members of the type in the state they were before the application
        /// of the current advice. Equivalent to the <c>base</c> C# keyword.
        /// </summary>
        /// <seealso cref="This"/>
        object Base { get; }

        object ThisStatic { get; }

        object BaseStatic { get; }

        IReadOnlyDictionary<string, object?> Tags { get; }

        IDiagnosticSink Diagnostics { get; }

        void DebugBreak();
    }

    [CompileTimeOnly]
    internal interface IMetaCodeBuilder
    {
        IExpression Expression( object? expression );

        object BuildArray( ArrayBuilder arrayBuilder );

        object BuildInterpolatedString( InterpolatedStringBuilder interpolatedStringBuilder );
    }
}