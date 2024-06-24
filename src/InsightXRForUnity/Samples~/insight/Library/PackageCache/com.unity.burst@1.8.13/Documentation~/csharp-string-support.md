# String support

Burst supports string usage in the following scenarios:

* [`Debug.Log`](https://docs.unity3d.com/ScriptReference/Debug.Log.html)
* Assigning a string to the [`FixedString`](https://docs.unity3d.com/Packages/com.unity.collections@2.2/api/Unity.Collections.FixedString.html) structs that `Unity.Collections` provides, for example [`FixedString128Bytes`](https://docs.unity3d.com/Packages/com.unity.collections@2.2/api/Unity.Collections.FixedString128Bytes.html).
* The [`System.Runtime.CompilerServices`](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices?view=net-6.0) attributes `[CallerLineNumber]`, `[CallerMemberName]`, and `[CallerFilePath]` on arguments to Burst functions. However, you can only pass the strings directly to calls to `Debug.Log`.

A string can be either:
* A string literal. For example: `"This is a string literal"`.
* An interpolated string using `$"This is an integer {value}` or using `string.Format`, where the string to format is also a string literal.

For example, Burst supports the following constructions:

* Logging with a string literal:

    ```c#
    Debug.Log("This a string literal");
   ```

* Logging using string interpolation:

    ```c#
    int value = 256;
    Debug.Log($"This is an integer value {value}"); 
    ```

    This is the same as using `string.Format` directly:

    ```c#
    int value = 256;
    Debug.Log(string.Format("This is an integer value {0}", value));
    ```

## Supported `Debug.Log` methods

Burst supports the following [`Debug.Log`](https://docs.unity3d.com/ScriptReference/Debug.Log.html) methods:

* `Debug.Log(object)`
* `Debug.LogWarning(object)`
* `Debug.LogError(object)`

## String interpolation support

String interpolation has the following restrictions:

* The string must be a string literal
* Burst supports the following `string.Format` methods:
    * `string.Format(string, object)` 
    * `string.Format(string, object, object)` 
    * `string.Format(string, object, object, object)`
    * `string.Format(string, object[])`. Use this for a string interpolation that contains more than three arguments, for example `$"{arg1} {arg2} {arg3} {arg4} {arg5}"`. In this case, the `object[]` array needs to be a constant size and no arguments should involve control flows (for example, `$"This is a {(cond ? arg1 : arg2)}"`).
* The string must only use value types
* The string must take only built-in type arguments:
    * `char`
    * `boolean` 
    * `byte` / `sbyte`
    * `double`
    * `float`
    * `short` / `ushort`
    * `int` / `uint`
    * `long` / `ulong`
* Burst supports sll vector types (for example `int2`, `float3`), except `half` vector types. For example:

    ```c#
    var value = new float3(1.0f, 2.0f, 3.0f);
    // Logs "This value float3(1f, 2f, 3f)"
    Debug.Log($"This value `{value}`");
* Burst doesn't support `ToString()` of structs. It displays the full name of the struct instead.

For more information, see the .NET documentation on [String interpolation](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated) and  [Standard numeric format strings](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings).

## Managed strings

You can pass a managed `string` literal or an interpolated string directly to `Debug.Log`, but you can't pass a string to a user method or to use them as fields in a struct. To pass around or store strings, use one of the [`FixedString`](https://docs.unity3d.com/Packages/com.unity.collections@1.2/api/Unity.Collections.FixedString.html) structs in the `Unity.Collections` package:

```c#
int value = 256;
FixedString128 text = $"This is an integer value {value} used with FixedString128";
MyCustomLog(text);

// ...

// String can be passed as an argument to a method using a FixedString, 
// but not using directly a managed `string`:
public static void MyCustomLog(in FixedString128 log)
{
    Debug.Log(text);
}
```

## Arguments and specifiers

Burst has limited support for string format arguments and specifiers:

```c#
int value = 256;

// Padding left: "This value `  256`
Debug.Log($"This value `{value,5}`");

// Padding right: "This value `256  `
Debug.Log($"This value `{value,-5}`");

// Hexadecimal uppercase: "This value `00FF`
Debug.Log($"This value `{value:X4}`");

// Hexadecimal lowercase: "This value `00ff`
Debug.Log($"This value `{value:x4}`");

// Decimal with leading-zero: "This value `0256`
Debug.Log($"This value `{value:D4}`");
```
