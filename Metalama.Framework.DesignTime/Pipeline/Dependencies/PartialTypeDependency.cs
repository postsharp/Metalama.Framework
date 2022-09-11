// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

internal record struct PartialTypeDependency( TypeDependencyKey MasterType, string DependentFilePath );