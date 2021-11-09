// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Eligibility;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// An <see cref="AspectInstance"/> with resolved <see cref="TargetDeclaration"/> and <see cref="Eligibility"/>.
    /// </summary>
    internal readonly struct ResolvedAspectInstance
    {
        public AspectInstance AspectInstance { get; }

        public IDeclarationImpl TargetDeclaration { get; }

        public EligibleScenarios Eligibility { get; }

        public ResolvedAspectInstance( AspectInstance aspectInstance, IDeclarationImpl targetDeclaration, EligibleScenarios eligibility )
        {
            this.AspectInstance = aspectInstance;
            this.TargetDeclaration = targetDeclaration;
            this.Eligibility = eligibility;
        }
    }
}