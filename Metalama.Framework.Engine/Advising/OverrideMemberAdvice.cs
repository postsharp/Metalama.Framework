// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.Advising
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
            ICompilation sourceCompilation,
            string? layerName,
            IObjectReader tags ) : base( aspect, templateInstance, targetDeclaration, sourceCompilation, layerName )
        {
            this.Tags = tags;
            
        }

        public override string ToString() => $"Override {this.TargetDeclaration}";
    }
}