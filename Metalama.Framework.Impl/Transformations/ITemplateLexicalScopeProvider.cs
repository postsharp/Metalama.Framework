﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.Templating;

namespace Metalama.Framework.Impl.Transformations
{
    internal interface ITemplateLexicalScopeProvider
    {
        TemplateLexicalScope GetLexicalScope( IDeclaration declaration );
    }
}