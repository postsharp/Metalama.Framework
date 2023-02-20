// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CompileTime;

internal interface IAttributeDeserializer
{
    bool TryCreateAttribute<T>( AttributeData attribute, IDiagnosticAdder diagnosticAdder, [NotNullWhen( true )] out T? attributeInstance )
        where T : Attribute;

    bool TryCreateAttribute( AttributeData attribute, IDiagnosticAdder diagnosticAdder, [NotNullWhen( true )] out Attribute? attributeInstance );
}