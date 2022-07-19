// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Engine.Transformations;

/// <summary>
/// Represents a transformation of a constructor so that it calls <c>: this()</c>.
/// </summary>
internal class CallDefaultConstructorTransformation : BaseTransformation, IMemberLevelTransformation
{
    public CallDefaultConstructorTransformation( Advice advice, IConstructor constructor ) : base( advice )
    {
        this.TargetMember = constructor;
    }

    public IMember TargetMember { get; }
}