using System;
using NUnit.Framework;
using Unity.Burst.Editor;
using UnityEngine.TestTools;
using Unity.Burst;
using Unity.Jobs;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class BurstMethodTreeViewTests
{
    /*
      Consists of a tree looking like:
       Root
       │
       └►BurstMethodTreeViewTests
         │
         │►Job1
         │ │
         │ └►TestMethod1
         │
         │►Job2
         │ │
         │ └►TestMethod1
         │
         │►Job3
         │ │
         │ └►TestMethod1(System.IntPtr)
         │
         │►Job4
         │ │
         │ └►TestMethod1
         │
         │►Job5 - (IJob)
         │
         │►NameOverlapping
         │  │
         │  └►NameOverlap(int,int)
         │
         └►Job5 - (IJob)
    */
    private BurstMethodTreeView _treeView;

    private List<BurstCompileTarget> _targets;

    private string _filter;

    [SetUp]
    public void SetUp()
    {
        _filter = string.Empty;

        _treeView = new BurstMethodTreeView(
            new TreeViewState(),
            () => _filter,
            () => (true, true)
        );

        string name = "TestMethod1";
        _targets = new List<BurstCompileTarget>
        {
            new BurstCompileTarget(typeof(Job1).GetMethod(name), typeof(Job1), null, true),
            new BurstCompileTarget(typeof(Job2).GetMethod(name), typeof(Job2), null, true),
            new BurstCompileTarget(typeof(Job3).GetMethod(name), typeof(Job3), null, true),
            new BurstCompileTarget(typeof(Job4).GetMethod(name), typeof(Job4), null, true),
            new BurstCompileTarget(typeof(NameOverlapping).GetMethod(nameof(NameOverlapping.NameOverlap)), typeof(NameOverlapping), null, true),
            new BurstCompileTarget(typeof(Job5).GetMethod("Execute"), typeof(Job5), typeof(IJob), false),
        };
    }

    private void testEquality<T>(List<T> exp, List<T> act)
    {
        Assert.AreEqual(exp.Count, act.Count, "List length did not match.");

        if (exp is List<TreeViewItem> elist && act is List<TreeViewItem> alist)
        {
            for (int i = 0; i < act.Count; i++)
            {
                Assert.IsTrue(alist[i].CompareTo(elist[i]) == 0,
                    $"expected: {elist[i].displayName}\nactual: {alist[i].displayName}");

            }
        }
        else
        {
            for (int i = 0; i < act.Count; i++)
            {
                Assert.AreEqual(exp[i], act[i], $"list items did not match at index {i}");
            }
        }
    }

    [Test]
    public void ProcessNewItemTest()
    {
        // Test for method containing . in name:
        List<StringSlice> oldNameSpace = new List<StringSlice>();
        int idJ = 0;
        var (idn, newtarget, nameSpace) =
            BurstMethodTreeView.ProcessNewItem(0, ++idJ, _targets[2], oldNameSpace);

        Assert.AreEqual(-2, idn);
        var expTargets = new List<TreeViewItem>
        {
            new TreeViewItem(-1, 0, $"{nameof(BurstMethodTreeViewTests)}"),
            new TreeViewItem(-2,1,$"{nameof(Job3)}"),
            new TreeViewItem(1, 2, "TestMethod1(System.IntPtr)")
        };
        var expNS = new List<StringSlice>
        {
            new StringSlice($"{nameof(BurstMethodTreeViewTests)}"),
            new StringSlice($"{nameof(Job3)}")
        };
        testEquality(expTargets, newtarget);
        testEquality(expNS, nameSpace);

        // Test for method with . and with thing in namespace:
        (idn, newtarget, nameSpace) = BurstMethodTreeView.ProcessNewItem(idn, ++idJ, _targets[2], nameSpace);
        Assert.AreEqual(-2, idn); // no new non-leafs added.
        expTargets = new List<TreeViewItem>
        {
            new TreeViewItem(2, 2, "TestMethod1(System.IntPtr)")
        };
        testEquality(expTargets, newtarget);
        testEquality(expNS, nameSpace);

        // Test with IJob instead of static method:
        (idn, newtarget, nameSpace) = BurstMethodTreeView.ProcessNewItem(0, ++idJ, _targets[5], oldNameSpace);
        Assert.AreEqual(-1, idn); // no new non-leafs added.
        expTargets = new List<TreeViewItem>
        {
            new TreeViewItem(-1, 0, $"{nameof(BurstMethodTreeViewTests)}"),
            new TreeViewItem(2, 2, $"{nameof(Job5)} - ({nameof(IJob)})")
        };
        expNS = new List<StringSlice> { new StringSlice($"{nameof(BurstMethodTreeViewTests)}"), };
        testEquality(expTargets, newtarget);
        testEquality(expNS, nameSpace);
    }


    private readonly (string, string[])[] _findNameSpacesTestInput =
    {
        ("Burst.Compiler.IL.Tests.TestGenerics+GenericStructOuter2`2+GenericStructInner`1[[Burst.Compiler.IL.Tests.TestGenerics+MyValueData1, Unity.Burst.Tests.UnitTests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null],[Burst.Compiler.IL.Tests.TestGenerics+MyValueData2, Unity.Burst.Tests.UnitTests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null],[Burst.Compiler.IL.Tests.TestGenerics+MyValueData2, Unity.Burst.Tests.UnitTests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]",
            new []{ "Burst.Compiler.IL.Tests.TestGenerics", "GenericStructOuter2`2" }),
        ("NameOverlapping+NameOverlap(int a, int b)",
            new []{ "NameOverlapping" }),
    };

    [Test]
    public void ExtractNameSpacesTest()
    {
        foreach (var (displayName, expectedNameSpaces) in _findNameSpacesTestInput)
        {
            var (nameSpaces, methodNameIdx) = BurstMethodTreeView.ExtractNameSpaces(displayName);

            int len = expectedNameSpaces.Length;
            Assert.AreEqual(len, nameSpaces.Count, "Amount of namespaces found is wrong.");
            int expectedNSIdx = 0;
            for (int i = 0; i < len; i++)
            {
                expectedNSIdx += expectedNameSpaces[i].Length + 1;
                Assert.AreEqual(expectedNameSpaces[i], nameSpaces[i].ToString(), "Wrong namespace name retrieval.");
            }
            Assert.AreEqual(expectedNSIdx, methodNameIdx, "Wrong index of method name.");
        }
    }


    [Test]
    public void InitializeTest()
    {
        int numNodes = 1 + _targets.Count; // Root + internal nodes.
        _treeView.Initialize(_targets, false);
        LogAssert.NoUnexpectedReceived();
        Assert.AreEqual(numNodes, _treeView.GetExpanded().Count, "All menu items should be expanded");

        _treeView.SetExpanded(-2, false);
        Assert.AreEqual(numNodes-1, _treeView.GetExpanded().Count, "First Job should be folded.");

        _treeView.Initialize(_targets, true);
        LogAssert.NoUnexpectedReceived();
        Assert.AreEqual(numNodes-1, _treeView.GetExpanded().Count);
    }

    [Test]
    public void BuildRootTest()
    {
        _treeView.Initialize(_targets, false);
        LogAssert.NoUnexpectedReceived();

        int dexp = 0;
        int idNexp = -1;
        int idLexp = 1;
        foreach (TreeViewItem twi in _treeView.GetRows())
        {
            Assert.AreEqual(dexp++, twi.depth, $"Depth of item \"{twi}\" was wrong.");
            if (dexp > 2) { dexp = 1; }

            Assert.AreEqual(twi.hasChildren ? idNexp-- : idLexp++, twi.id, $"ID of item \"{twi}\" was wrong.");
        }
    }

    [Test]
    public void GetSelection()
    {
        _treeView.Initialize(_targets, false);
        LogAssert.NoUnexpectedReceived();

        IList<int> actual = _treeView.GetSelection();
        Assert.IsEmpty(actual, "Selection count wrong.");

        _treeView.SelectAllRows();
        actual = _treeView.GetSelection();
        Assert.IsEmpty(actual, "Selection count wrong. Multirow selection not allowed.");

        _treeView.SetSelection(new List<int> { -2 });
        actual = _treeView.GetSelection();
        Assert.AreEqual(0, actual.Count, "Selection count wrong. Label selection not allowed.");

        _treeView.SetSelection(new List<int> { 1 });
        actual = _treeView.GetSelection();
        Assert.AreEqual(1, actual.Count, "Selection count wrong.");
        Assert.AreEqual(1, actual[0], "Selection ID wrong.");
    }

    [Test]
    public void TrySelectByDisplayNameTest()
    {
        const string name = "BurstMethodTreeViewTests.Job1.TestMethod1()";
        _treeView.Initialize(_targets, false);
        LogAssert.NoUnexpectedReceived();

        Assert.IsFalse(_treeView.TrySelectByDisplayName("Not present"));

        Assert.IsTrue(_treeView.TrySelectByDisplayName(name), "TreeView Could not find method.");
    }

    [Test]
    public void ProcessEntireTestProject()
    {
        // Make it filter out some jobs:
        _filter = "Unity";

        // Get all target jobs!
        var assemblyList = BurstReflection.EditorAssembliesThatCanPossiblyContainJobs;
        var result = BurstReflection.FindExecuteMethods(assemblyList, BurstReflectionAssemblyOptions.None).CompileTargets;
        result.Sort((left, right) => string.Compare(left.GetDisplayName(), right.GetDisplayName(), StringComparison.Ordinal));

        // Initialize the tree view:
        _treeView.Initialize(result, false);
        LogAssert.NoUnexpectedReceived();

        // Check if everything is good and ready:
        var visibleJobs = _treeView.GetRows().Where(twi => !twi.hasChildren);
        foreach (TreeViewItem twi in visibleJobs)
        {
            var actual = result[twi.id - 1];
            var expected = twi.displayName;

            Assert.IsTrue(actual.GetDisplayName().Contains(expected), $"Retrieved the wrong target base on id.\nGot \"{actual.GetDisplayName()}\"\nExpected \"{expected}\"");
        }
    }


    [BurstCompile]
    private struct Job1
    {
        [BurstCompile]
        public static void TestMethod1() { }
    }
    [BurstCompile]
    private struct Job2
    {
        [BurstCompile]
        public static void TestMethod1() { }
    }
    [BurstCompile]
    private struct Job3
    {
        [BurstCompile]
        public static void TestMethod1(System.IntPtr ptr) { }
    }
    [BurstCompile]
    private struct Job4
    {
        [BurstCompile]
        public static void TestMethod1() { }
    }

    [BurstCompile]
    private struct Job5 : IJob
    {
        public void Execute() { }
    }

    [BurstCompile]
    private class NameOverlapping
    {
        [BurstCompile]
        public static int NameOverlap(int a, int b) => a + b;
    }
}