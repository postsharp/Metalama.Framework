// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.Advices
{
    internal sealed class Filter
    {
        public Ref<IDeclaration> TargetDeclaration { get; }

        public TemplateMember<IMethod> Template { get; }

        public FilterDirection Direction { get; }

        public IObjectReader Tags { get; }

        public IObjectReader TemplateArguments { get; }

        public Filter(
            IDeclaration targetDeclaration,
            TemplateMember<IMethod> template,
            FilterDirection direction,
            IObjectReader tags,
            IObjectReader templateArguments )
        {
            this.TargetDeclaration = targetDeclaration.ToTypedRef();
            this.Template = template;
            this.Tags = tags;
            this.TemplateArguments = templateArguments;

            // Resolve the default value before storing the direction.
            if ( direction == FilterDirection.Default )
            {
                this.Direction = targetDeclaration switch
                {
                    IParameter { IsReturnParameter: true } => FilterDirection.Output,
                    IParameter { RefKind: RefKind.Out } => FilterDirection.Output,
                    IParameter => FilterDirection.Input,
                    IFieldOrProperty { Writeability: Writeability.None } => FilterDirection.Output,
                    IFieldOrProperty => FilterDirection.Input,
                    _ => throw new AssertionFailedException()
                };
            }
            else
            {
                this.Direction = direction;
            }
        }

        public bool AppliesTo( FilterDirection direction )
        {
            return this.Direction == direction || this.Direction == FilterDirection.Both;
        }
    }
}