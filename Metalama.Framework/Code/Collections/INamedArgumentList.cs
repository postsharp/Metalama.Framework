// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Code;

public interface INamedArgumentList : IReadOnlyList<KeyValuePair<string, TypedConstant>>, IReadOnlyDictionary<string, TypedConstant> { }