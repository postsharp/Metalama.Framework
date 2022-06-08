// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;

namespace Metalama.Framework.Engine.Advices
{
    internal abstract class OverrideMemberAdvice<TMember> : Advice
        where TMember : class, IMember
    {
        public new Ref<TMember> TargetDeclaration => base.TargetDeclaration.As<TMember>();

        public IObjectReader Tags { get; }

        public OverrideMemberAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            TMember targetDeclaration,
            string? layerName,
            IObjectReader tags ) : base( aspect, templateInstance, targetDeclaration, layerName )
        {
            this.Tags = tags;
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {
            // TODO: Test that the advice is not applied to declaration in a base class.
        }

        public override string ToString() => $"Override {this.TargetDeclaration}";
    }
}