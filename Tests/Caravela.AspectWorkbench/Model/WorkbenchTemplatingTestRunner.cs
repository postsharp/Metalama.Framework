// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Tests.Integration.Templating;
using Microsoft.CodeAnalysis;

namespace Caravela.AspectWorkbench.Model
{
    internal class WorkbenchTemplatingTestRunner : TemplatingTestRunner
    {
        public new Project CreateProject()
        {
            return base.CreateProject();
        }
    }
}
