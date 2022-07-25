// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Engine.Utilities;

public delegate TResult UserCodeFunc<out TResult, TPayload>( ref TPayload payload );