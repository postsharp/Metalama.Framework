﻿using System;
using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Tests.AspectTests.Tests.Fabrics.SelectAttributes_Inheritable;

// <target>
public class DerivedClass : BaseClass { }