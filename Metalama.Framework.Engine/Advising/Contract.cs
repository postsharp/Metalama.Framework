// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed class Contract
    {
        public Ref<IDeclaration> TargetDeclaration { get; }

        public TemplateMember<IMethod> Template { get; }

        public ContractDirection Direction { get; }

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

            // Resolve the default value before storing the direction.
            if ( direction == ContractDirection.Default )
            {
                this.Direction = targetDeclaration switch
                {
                    IParameter { IsReturnParameter: true } => ContractDirection.Output,
                    IParameter { RefKind: RefKind.Out } => ContractDirection.Output,
                    IParameter => ContractDirection.Input,
                    IFieldOrProperty { Writeability: Writeability.None } => ContractDirection.Output,
                    IFieldOrProperty => ContractDirection.Input,
                    _ => throw new AssertionFailedException()
                };
            }
            else
            {
#if DEBUG
                if ( direction == ContractDirection.Input && targetDeclaration is IParameter { IsReturnParameter: true } )
                {
                    throw new AssertionFailedException();
                }

#endif
                this.Direction = direction;
            }
        }

        public bool AppliesTo( ContractDirection direction )
        {
            return this.Direction == direction || this.Direction == ContractDirection.Both;
        }
    }
}