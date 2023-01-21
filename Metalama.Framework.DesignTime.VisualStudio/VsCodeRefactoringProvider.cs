// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.DesignTime.VisualStudio.Services;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;

namespace Metalama.Framework.DesignTime.VisualStudio;

[UsedImplicitly]
public class VsCodeRefactoringProvider : TheCodeRefactoringProvider
{
    public VsCodeRefactoringProvider( ServiceProvider<IGlobalService> serviceProvider ) : base( serviceProvider ) { }

    public VsCodeRefactoringProvider() : this( VsServiceProviderFactory.GetServiceProvider() ) { }
}