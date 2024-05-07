// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using System;

namespace Metalama.Framework.Code.DeclarationBuilders;

public interface IInterfaceImplementationBuilder
{
    void AddMethod( in MethodTemplateSelector template, Action<IMethodBuilder>? buildMethod = null, object? args = null, object? tags = null );

    // TODO: properties, events
}