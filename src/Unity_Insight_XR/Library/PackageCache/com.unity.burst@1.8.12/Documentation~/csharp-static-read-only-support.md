# Static read-only fields and static constructor support

Burst evaluates all static fields and all static constructors at compile time. It evaluates all the static fields and the static constructors for a given struct together. 

When there is a static field that isn't read-only in a Burst-compiled struct, a compilation error happens. This is because Burst only supports read-only static fields.

When Burst fails to evaluate any static field or static constructor, all fields and constructors fail for that struct. 

When compile-time evaluation fails, Burst falls back to compiling all static initialization code into an initialization function that it calls once at runtime. This means that your code needs to be Burst compatible, or it will fail compilation if it fails compile-time evaluation.

An exception to this is that there's limited support for initializing static read-only array fields as long as they're initialized from either an array constructor or from static data:
- `static readonly int[] MyArray0 = { 1, 2, 3, .. };`
- `static readonly int[] MyArray1 = new int[10];`

## Language support

Burst doesn't support calling external functions and function pointers. 

It supports using the following base language with static read-only fields and constructors:

* Managed arrays
* Strings
* Limited intrinsic support:
    * `Unity.Burst.BurstCompiler.IsEnabled`
    * `Unity.Burst.BurstRuntime.GetHashCode32`
    * `Unity.Burst.BurstRuntime.GetHashCode64`
    * Vector type construction
* Limited intrinsic assertion support:
    * `UnityEngine.Debug.Assert`
    * `NUnit.Framework.Assert.AreEqual`
    * `NUnit.Framework.Assert.AreNotEqual`
* Simple throw patterns. Any exceptions thrown during evaluation become compiler errors.