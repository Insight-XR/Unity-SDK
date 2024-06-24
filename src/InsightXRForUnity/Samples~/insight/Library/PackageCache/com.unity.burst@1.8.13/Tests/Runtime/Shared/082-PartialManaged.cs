using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Burst.CompilerServices;

#if BURST_TESTS_ONLY
namespace Unity.Collections.LowLevel.Unsafe
{
    internal class DisposeSentinel
    {
    }
}
#endif

namespace Burst.Compiler.IL.Tests
{
    /// <summary>
    /// Tests related to usage of partial managed objects (e.g loading null or storing null
    /// reference to a struct, typically used by NativeArray DisposeSentinel)
    /// </summary>
    internal class PartialManaged
    {
#if BURST_TESTS_ONLY || ENABLE_UNITY_COLLECTIONS_CHECKS
        [TestCompiler]
        public static int TestWriteNullReference()
        {
            var element = new Element();
            WriteNullReference(out element.Reference);
            return element.Value;
        }

        [BurstDiscard]
        private static void WriteNullReference(out DisposeSentinel reference)
        {
            reference = null;
        }

        private struct Element
        {
#pragma warning disable 0649
            public int Value;
            public DisposeSentinel Reference;
#pragma warning restore 0649

        }
#endif

        [TestCompiler]
        public static void AssignNullToLocalVariableClass()
        {
            MyClass x = null;
#pragma warning disable 0219
            MyClass value = x;
#pragma warning restore 0219
        }

        [TestCompiler(ExpectCompilerException = true, ExpectedDiagnosticId = DiagnosticId.ERR_CallingManagedMethodNotSupported)]
        public static int GetIndexOfCharFomString()
        {
            return "abc".IndexOf('b');
        }

        struct StructWithManaged
        {
#pragma warning disable 0649
            public MyClass myClassValue;
            public string stringValue;
            public object objectValue;
            public float[] arrayValue;

            public int value;
#pragma warning restore 0649
        }

        [TestCompiler(ExpectCompilerException = true, ExpectedDiagnosticId = DiagnosticId.ERR_TypeNotSupported)]
        public static int AccessClassFromStruct()
        {
            var val = new StructWithManaged();
            val.myClassValue.value = val.value;
            return val.myClassValue.value;
        }

        [TestCompiler(ExpectCompilerException = true, ExpectedDiagnosticId = DiagnosticId.ERR_TypeNotSupported)]
        public static void AccessStringFromStruct()
        {
            var val = new StructWithManaged();
#pragma warning disable 0219
            var p = val.stringValue = "abc";
#pragma warning restore 0219
        }

        [TestCompiler(ExpectCompilerException = true, ExpectedDiagnosticId = DiagnosticId.ERR_TypeNotSupported)]
        public static void AccessObjectFromStruct()
        {
            var val = new StructWithManaged();
#pragma warning disable 0219
            var p = val.objectValue;
            p = new object();
#pragma warning restore 0219
        }

        [TestCompiler(ExpectCompilerException = true, ExpectedDiagnosticId = DiagnosticId.ERR_TypeNotSupported)]
        public static void AccessArrayFromStruct()
        {
            var val = new StructWithManaged();
            var p = val.arrayValue;
            p[0] = val.value;
        }

        [TestCompiler(ExpectCompilerException = true, ExpectedDiagnosticId = DiagnosticId.ERR_TypeNotSupported)]
        public static int GetValueFromStructWithClassField()
        {
            var val = new StructWithManaged();
            val.value = 5;

            return val.value;
        }

        [TestCompiler(ExpectCompilerException = true, ExpectedDiagnosticId = DiagnosticId.ERR_InstructionNewobjWithManagedTypeNotSupported)]
        public static void NewMyClass()
        {
#pragma warning disable 0219
            var value = new MyClass();
#pragma warning restore 0219
        }

        private class MyClass
        {
            public int value;
        }

        private class SomeClassWithMixedStatics
        {
            public static int SomeInt = 42;

            public static readonly SharedStatic<int> SomeSharedStatic = SharedStatic<int>.GetOrCreate<int>();

            [BurstDiscard]
            private static void DoSomethingWithStaticInt(ref int x) => x = SomeInt;

            [IgnoreWarning(1371)]
            public static int DoSomething()
            {
                ref var data = ref SomeSharedStatic.Data;
                DoSomethingWithStaticInt(ref data);
                return SomeSharedStatic.Data;
            }
        }

        [TestCompiler(OverrideManagedResult = 0)]
        public static int DoSomethingThatUsesMixedStatics()
        {
            return SomeClassWithMixedStatics.DoSomething();
        }

        private class SomeClassWithMixedStaticsWithExplicitStaticConstructor
        {
            public static int SomeInt = 42;

            public static readonly SharedStatic<int> SomeSharedStatic = SharedStatic<int>.GetOrCreate<int>();

            static SomeClassWithMixedStaticsWithExplicitStaticConstructor()
            {
                SomeInt = 1;
            }

            [BurstDiscard]
            private static void DoSomethingWithStaticInt(ref int x) => x = SomeInt;

            [IgnoreWarning(1371)]
            public static int DoSomething()
            {
                ref var data = ref SomeSharedStatic.Data;
                DoSomethingWithStaticInt(ref data);
                return SomeSharedStatic.Data;
            }
        }

        [TestCompiler(OverrideManagedResult = 0)]
        public static int DoSomethingThatUsesMixedStaticsWithExplicitStaticConstructor()
        {
            return SomeClassWithMixedStaticsWithExplicitStaticConstructor.DoSomething();
        }
    }
}
