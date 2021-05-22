// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.ArchitectureValidation;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;

namespace Caravela.Framework.Aspects
{
    [InternalImplement]
    public interface IAspectClassBuilder
    {
        string DisplayName { get; set; }

        string? Description { get; set; }

        void DefinesLayers( string firstLayer, params string[] otherLayers );

        IAspectDependencyBuilder Dependencies { get; }
    }

   
}