// CompileTimeAspectPipeline.ExecuteAsync failed.
// Error LAMA0037 on `this`: `The aspect 'NotZero' cannot be applied to the indexer 'Target.this[int]' because 'Target.this[int]' must be writable.`
// Error LAMA0037 on `this`: `The aspect 'NotZero' cannot be applied to the indexer 'Target.this[int, int]' because 'Target.this[int, int]' must be writable.`
// Error LAMA0037 on `this`: `The aspect 'NotZero' cannot be applied to the indexer 'Target.this[int, int, int]' because none of these conditions was fulfilled: { (a) 'Target.this[int, int, int]' must be a field, or (b) 'Target.this[int, int, int]' must have a getter }.`