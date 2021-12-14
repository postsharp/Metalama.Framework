// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Aspects
{
    internal interface IAspectInstanceInternal : IAspectInstance, IAspectPredecessorImpl
    {
        new Ref<IDeclaration> TargetDeclaration { get; }

        void Skip();

        ImmutableDictionary<TemplateClass, TemplateClassInstance> TemplateInstances { get; }

        void SetState( IAspectState? value );
    }
}