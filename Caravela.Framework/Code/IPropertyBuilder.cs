// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Code
{
    public interface IPropertyBuilder : IFieldOrPropertyBuilder, IProperty
    {
        new RefKind RefKind { get; set; }

        IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant defaultValue = default );

        IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, object? defaultValue = null );

        new IMethodBuilder? Getter { get; }

        new IMethodBuilder? Setter { get; }
    }
}