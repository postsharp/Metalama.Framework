// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advising
{
    internal class TemplateBasedInitializeAdvice : InitializeAdvice
    {
        public BoundTemplateMethod BoundTemplate { get; }

        public IObjectReader Tags { get; }

        public TemplateBasedInitializeAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IMemberOrNamedType targetDeclaration,
            ICompilation sourceCompilation,
            BoundTemplateMethod boundTemplate,
            InitializerKind kind,
            string? layerName,
            IObjectReader tags ) : base( aspect, templateInstance, targetDeclaration, sourceCompilation, kind, layerName )
        {
            this.BoundTemplate = boundTemplate;
            this.Tags = tags;
        }

        protected override void AddTransformation( IMemberOrNamedType targetDeclaration, IConstructor targetCtor, Action<ITransformation> addTransformation )
        {
            var initialization = new TemplateBasedInitializationTransformation(
                this,
                targetDeclaration,
                targetCtor,
                this.BoundTemplate,
                this.Tags );

            addTransformation( initialization );
        }
    }
}