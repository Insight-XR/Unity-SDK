# SharedStatic struct

Burst has basic support for accessing static readonly data. However, if you want to share static mutable data between C# and HPC#, use the `SharedStatic<T>` struct.

The following example shows accessing an `int` static field that both C# and HPC# can change: 

```C#
    public abstract class MutableStaticTest
    {
        public static readonly SharedStatic<int> IntField = SharedStatic<int>.GetOrCreate<MutableStaticTest, IntFieldKey>();

        // Define a Key type to identify IntField
        private class IntFieldKey {}
    }
```     

C# and HPC# can then access this:

```C#
    // Write to a shared static 
    MutableStaticTest.IntField.Data = 5;
    // Read from a shared static
    var value = 1 + MutableStaticTest.IntField.Data;
``` 

When you use `SharedStatic<T>`, be aware of the following:

* The `T` in `SharedStatic<T>` defines the data type.
* To identify a static field, provide a context for it. To do this, create a key for both the containing type (for example, `MutableStaticTest` in the example above), identify the field (for example, `IntFieldKey` class in the example above) and pass these classes as generic arguments of `SharedStatic<int>.GetOrCreate<MutableStaticTest, IntFieldKey>()`.
* Always initialize the shared static field in C# from a static constructor before accessing it from HPC#. If you don't initialize the data before accessing it, it might lead to an undefined initialization state.