using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using Unity.Burst;
using UnityEngine;

namespace Burst.Compiler.IL.Tests
{

    internal class TestCompilerAttribute : TestCompilerAttributeBase
    {
        public TestCompilerAttribute(params object[] arguments) : base(arguments)
        {
        }

        protected override bool IsCommandLine()
        {
            return false;
        }

        protected override bool IsMono()
        {
            return true;
        }

        protected override bool SupportException {
            get {
                // We don't support exception in Unity editor
                return false;
            }
        }

        protected override TestCompilerCommandBase GetTestCommand(TestCompilerAttributeBase attribute, Test test, TestMethod originalMethod)
        {
            return new TestCompilerCommand(attribute, test, originalMethod);
        }

        public class TestCompilerCommand : TestCompilerCommandBase
        {
            public TestCompilerCommand(TestCompilerAttributeBase attribute, Test test, TestMethod originalMethod) : base(attribute, test, originalMethod)
            {
            }

            protected override int GetRunCount()
            {
                return 1;
            }

            protected override void CompleteTest(ITestExecutionContext context)
            {
                context.CurrentResult.SetResult(ResultState.Success);
            }

            protected override int GetULP()
            {
                return 512;
            }

            protected override object[] GetArgumentsArray(TestMethod method)
            {
                return method.parms.Arguments.ToArray();
            }

            protected unsafe Delegate CompileDelegate(ITestExecutionContext context, MethodInfo methodInfo,
                                                               Type delegateType, byte* returnBox, out Type returnBoxType) {
                return CompileDelegate(context, methodInfo, delegateType, returnBox, out returnBoxType, out _);
            }

            protected unsafe override Delegate CompileDelegate(ITestExecutionContext context, MethodInfo methodInfo,
                                                               Type delegateType, byte* returnBox, out Type returnBoxType,
                                                               out Delegate interpretDelegate)
            {
                interpretDelegate = null;
                returnBoxType = null;

                var functionDelegate = Delegate.CreateDelegate(delegateType, methodInfo);
                var compiledFunction = BurstCompiler.CompileDelegate(functionDelegate);

                return compiledFunction;
            }

            protected override void GoldFileTestForOtherPlatforms(MethodInfo methodInfo, bool testWasRun)
            {
                // This is a no-op here.
            }
            
            protected override bool TestOnCurrentHostEnvironment(MethodInfo methodInfo)
            {
                // Query architecture via burst to avoid mono bug in detecting processor architecture
                if (BurstCompiler.IsHostEditorArm())
                    return !methodInfo.CustomAttributes.Any((e) => e.AttributeType.Name == "TestCpuAttribute");
                return true;
            }

            protected override object CompileFunctionPointer(MethodInfo methodInfo, Type functionType)
            {
                throw new NotImplementedException();
            }

            protected override bool InterpretMethod(Delegate interpretDelegate, MethodInfo methodInfo, object[] args, Type returnType, out string reason, out object result) {
                reason = null;
                result = null;
                return false;
            }

            protected override void Setup()
            {
            }

            protected override TestResult HandleCompilerException(ITestExecutionContext context, MethodInfo methodInfo)
            {
                var arguments = GetArgumentsArray(_originalMethod);
                Type[] nativeArgTypes = new Type[arguments.Length];

                for (var i = 0; i < arguments.Length; ++i)
                {
                    nativeArgTypes[i] = arguments[i].GetType();
                }

                bool isInRegistry;
                Func<object, object[], object> caller;
                var delegateType = CreateNativeDelegateType(methodInfo.ReturnType, nativeArgTypes, out isInRegistry, out caller);

                var functionDelegate = Delegate.CreateDelegate(delegateType, methodInfo);
                Delegate compiledFunction = BurstCompiler.CompileDelegate(functionDelegate);

                if (functionDelegate == compiledFunction)
                    context.CurrentResult.SetResult(ResultState.Success);
                else
                    context.CurrentResult.SetResult(ResultState.Failure, $"The function have been compiled successfully, but an error was expected.");

                return context.CurrentResult;
            }

            string cachedLog = null;
            bool logPresent = false;

            protected override string GetLinesFromDeterminismLog()
            {
                if (cachedLog==null)
                {
                    try
                    {
                        TextAsset txtData = (TextAsset)Resources.Load("btests_deterministic");
                        cachedLog = txtData.text;
                        logPresent = true;
                    }
                    catch
                    {
                        logPresent = false;
                    }
                }

                return cachedLog;
            }

            protected override bool IsDeterministicTest(TestMethod method)
            {
                return logPresent && (method.Method.ReturnType.IsType(typeof(double)) ||
                        method.Method.ReturnType.IsType(typeof(float)));
            }

            protected override TestCompilerBaseExtensions GetExtension()
            {
                return null;
            }

            protected override bool TargetIs32Bit()
            {
                return false;
            }
        }
    }
}
