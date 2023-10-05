// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed class Contract
    {
        private readonly ContractDirection _direction;

        public Ref<IDeclaration> TargetDeclaration { get; }

        public TemplateMember<IMethod> Template { get; }

        public IObjectReader Tags { get; }

        public IObjectReader TemplateArguments { get; }

        public Contract(
            IDeclaration targetDeclaration,
            TemplateMember<IMethod> template,
            ContractDirection direction,
            IObjectReader tags,
            IObjectReader templateArguments )
        {
            this.TargetDeclaration = targetDeclaration.ToTypedRef();
            this.Template = template;
            this.Tags = tags;
            this.TemplateArguments = templateArguments;
            this._direction = ContractAspectHelper.GetEffectiveDirection( direction, targetDeclaration );
        }

        public bool AppliesTo( ContractDirection direction )
        {
            return this._direction == direction || this._direction == ContractDirection.Both;
        }
    }
}