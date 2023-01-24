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

            // Resolve the default value before storing the direction.
            if ( direction == ContractDirection.Default )
            {
                this._direction = targetDeclaration switch
                {
                    IParameter { IsReturnParameter: true } => ContractDirection.Output,
                    IParameter { RefKind: RefKind.Out } => ContractDirection.Output,
                    IParameter => ContractDirection.Input,
                    IFieldOrPropertyOrIndexer { Writeability: Writeability.None } => ContractDirection.Output,
                    IFieldOrPropertyOrIndexer => ContractDirection.Input,
                    _ => throw new AssertionFailedException( $"Unexpected kind of declaration: '{targetDeclaration}'." )
                };
            }
            else
            {
                if ( direction == ContractDirection.Input && targetDeclaration is IParameter { IsReturnParameter: true } )
                {
                    throw new AssertionFailedException( $"Unexpected declaration for input contract: '{targetDeclaration}'." );
                }

                this._direction = direction;
            }
        }

        public bool AppliesTo( ContractDirection direction )
        {
            return this._direction == direction || this._direction == ContractDirection.Both;
        }
    }
}