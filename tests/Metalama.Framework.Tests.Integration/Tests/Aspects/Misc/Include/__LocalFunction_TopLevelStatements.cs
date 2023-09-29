using Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.LocalFunction_TopLevelStatements;

[MethodAspect]
[MethodBaseAspect]
[return: Contract]
int LocalFunction([Contract] int a) => a;

LocalFunction(42);