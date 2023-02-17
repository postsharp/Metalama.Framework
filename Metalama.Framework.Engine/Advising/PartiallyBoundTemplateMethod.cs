// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed class PartiallyBoundTemplateMethod
    {
        public TemplateMember<IMethod> TemplateMember { get; }

        public IMethod Declaration => this.TemplateMember.Declaration;

        public IObjectReader? Arguments { get; }

        /// <summary>
        /// Gets the bound template type arguments.
        /// </summary>
        public object?[] TypeArguments { get; }

        public PartiallyBoundTemplateMethod( TemplateMember<IMethod> template, object?[] typeArguments, IObjectReader? argumentReader )
        {
            this.TemplateMember = template;
            this.Arguments = argumentReader;
            this.TypeArguments = typeArguments;
        }
    }
}