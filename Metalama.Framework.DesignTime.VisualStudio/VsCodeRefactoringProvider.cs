// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.DesignTime.VisualStudio;

public class VsCodeRefactoringProvider : TheCodeRefactoringProvider
{
    public VsCodeRefactoringProvider( ServiceProvider serviceProvider ) : base( serviceProvider ) { }

    public VsCodeRefactoringProvider() : this( VsServiceProviderFactory.GetServiceProvider() ) { }
}