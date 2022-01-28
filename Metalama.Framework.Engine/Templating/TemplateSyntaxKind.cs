// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace Metalama.Framework.Engine.Templating
{
    internal enum TemplateSyntaxKind
    {
        Self,
        FieldInitializer,
        PropertyInitializer,
        EventFieldInitializer,
    }
}
