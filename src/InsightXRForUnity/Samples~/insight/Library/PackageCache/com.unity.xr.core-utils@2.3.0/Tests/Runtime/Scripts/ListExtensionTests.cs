using System;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.XR.CoreUtils;

namespace Unity.XR.CoreUtils.Tests
{
    class ListExtensionTests
    {
        const int k_DefaultTestCapacity = 35;

        class TestElement
        {
            public int Value;
        }

        void EnsureCapacityHelper<T>(List<T> list, int desiredCapacity = k_DefaultTestCapacity)
        {
            Assert.AreNotEqual(desiredCapacity, list.Capacity);

            list.EnsureCapacity(desiredCapacity);

            Assert.AreEqual(desiredCapacity, list.Capacity);
        }

        void SwapAtIndicesHelper<T>(List<T> list, Func<int, T> create, Action<T,T> assertEqual, int addCount = k_DefaultTestCapacity)
        {
            Assert.NotNull(create);
            for (var i = 0; i < addCount; i++)
            {
                list.Add(create(i));
            }

            Assert.AreEqual(list.Count, addCount);

            var lastIndex = addCount - 1;
            var firstValue = create(0);
            var lastValue = create(lastIndex);

            assertEqual(list[0], firstValue);
            assertEqual(list[lastIndex], lastValue);

            list.SwapAtIndices(0, lastIndex);

            assertEqual(list[0], lastValue);
            assertEqual(list[lastIndex], firstValue);
        }

        void FillHelper<T>(List<T> list, Action<T,T> assertEqual, T defaultNewValue, int fillNum = 5)
            where T : new()
        {
            Assert.NotNull(assertEqual);
            Assert.AreNotEqual(fillNum, 0);

            var beforeCount = list.Count;

            list.Fill(fillNum);

            var fillCount = beforeCount + fillNum;

            Assert.AreEqual(beforeCount + fillNum, list.Count);

            for (var i = beforeCount; i < fillCount; i++)
            {
                assertEqual(list[i], defaultNewValue);
            }
        }

        static void AssertEqualInt(int x, int y)
        {
            Assert.AreEqual(x, y);
        }

        static void AssertEqualRef(TestElement x, TestElement y)
        {
            Assert.NotNull(x);
            Assert.NotNull(y);
            Assert.AreNotEqual(x, y);
            Assert.AreEqual(x.Value, y.Value);
        }

        static int CreateInt(int i) => i;

        static TestElement CreateRef(int i) => new TestElement() { Value = i };

        [Test]
        public void Test_EnsureCapacity()
        {
            var intList = new List<int>(4);
            var refList = new List<TestElement>(2);

            EnsureCapacityHelper(intList);
            EnsureCapacityHelper(refList);
        }

        [Test]
        public void Test_SwapAtIndices()
        {
            var intList = new List<int>();
            var refList = new List<TestElement>();

            SwapAtIndicesHelper(intList, CreateInt, AssertEqualInt);
            SwapAtIndicesHelper(refList, CreateRef, AssertEqualRef);
        }

        [Test]
        public void Test_Fill()
        {
            var intList = new List<int>();
            var refList = new List<TestElement>();

            FillHelper(intList, AssertEqualInt, new int());
            FillHelper(refList, AssertEqualRef, new TestElement());
        }
    }
}
