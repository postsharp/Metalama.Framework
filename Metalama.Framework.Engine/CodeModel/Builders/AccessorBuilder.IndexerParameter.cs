// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System;
using RefKind = Metalama.Framework.Code.RefKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        // ReSharper disable once UnusedType.Local
        // TODO: Use this type and remove the warning waiver.

        private sealed class IndexerParameter : ParameterBase
        {
            public IndexerParameter( AccessorBuilder accessor, int index, string name, IType parameterType, RefKind refKind ) : base( accessor, index )
            {
                this.Name = name;
                this.Type = parameterType;
                this.RefKind = refKind;
            }

            public override TypedConstant? DefaultValue { get; set; }

            public override IType Type { get; set; }

            public override RefKind RefKind { get; set; }

            public override bool IsParams => false;

            public override string Name { get; set; }

            public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
                => throw new NotImplementedException();
        }
    }
}