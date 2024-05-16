using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Burst.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

[TestFixture]
[UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor)]
public class BurstInspectorGUITests
{
    private readonly WaitUntil _waitForInitialized =
        new WaitUntil(() => EditorWindow.GetWindow<BurstInspectorGUI>()._initialized);

    private IEnumerator SelectJobAwaitLoad(string assemblyName)
    {
        EditorWindow.GetWindow<BurstInspectorGUI>()._treeView.TrySelectByDisplayName(assemblyName);
        return new WaitUntil(() =>
            EditorWindow.GetWindow<BurstInspectorGUI>()._textArea.IsTextSet(assemblyName)
        );
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Close down window if it's open, to start with a fresh inspector.
        EditorWindow.GetWindow<BurstInspectorGUI>().Close();
        EditorWindow.GetWindow<BurstInspectorGUI>().Show();

        // Make sure window is actually initialized before continuing.
        yield return _waitForInitialized;
    }

    [UnityTest]
    public IEnumerator TestInspectorOpenDuringDomainReloadDoesNotLogErrors()
    {
        // Show Inspector window
        EditorWindow.GetWindow<BurstInspectorGUI>().Show();

        Assert.IsTrue(EditorWindow.HasOpenInstances<BurstInspectorGUI>());

        // Ask for domain reload
        EditorUtility.RequestScriptReload();

        // Wait for the domain reload to be completed
        yield return new WaitForDomainReload();

        Assert.IsTrue(EditorWindow.HasOpenInstances<BurstInspectorGUI>());

        // Hide Inspector window
        EditorWindow.GetWindow<BurstInspectorGUI>().Close();

        Assert.IsFalse(EditorWindow.HasOpenInstances<BurstInspectorGUI>());
    }

    [UnityTest]
    public IEnumerator DisassemblerNotChangingUnexpectedlyTest()
    {
        const string jobName2 = "BurstReflectionTests.MyJob - (IJob)";
        const string jobName = "BurstInspectorGUITests.MyJob - (IJob)";

        // Selecting a specific assembly.
        yield return SelectJobAwaitLoad(jobName);
        var window = EditorWindow.GetWindow<BurstInspectorGUI>();

        try
        {
            // Sending event to set the displayname, to avoid it resetting _scrollPos because of target change.
            window.SendEvent(new Event()
            {
                type = EventType.Repaint,
                mousePosition = new Vector2(window.position.width / 2f, window.position.height / 2f)
            });
            yield return null;

            // Doing actual test work:
            var prev = new BurstDisassemblerWithCopy(window._burstDisassembler);
            window.SendEvent(new Event()
            {
                type = EventType.Repaint,
                mousePosition = new Vector2(window.position.width / 2f, window.position.height / 2f)
            });
            yield return null;
            Assert.IsTrue(prev.Equals(window._burstDisassembler),
                "Public fields changed in burstDisassembler even though they shouldn't");

            prev = new BurstDisassemblerWithCopy(window._burstDisassembler);
            window.SendEvent(new Event() { type = EventType.MouseUp, mousePosition = Vector2.zero });
            yield return null;
            Assert.IsTrue(prev.Equals(window._burstDisassembler),
                "Public fields changed in burstDisassembler even though they shouldn't");

            prev = new BurstDisassemblerWithCopy(window._burstDisassembler);
            yield return SelectJobAwaitLoad(jobName2);
            window = EditorWindow.GetWindow<BurstInspectorGUI>();


            window.SendEvent(new Event()
            {
                type = EventType.Repaint,
                mousePosition = new Vector2(window.position.width / 2f, window.position.height / 2f)
            });
            yield return null;
            Assert.IsFalse(prev.Equals(window._burstDisassembler), "Public fields of burstDisassembler did not change");
        }
        finally
        {
            window.Close();
        }
    }

    [UnityTest]
    public IEnumerator InspectorStallingLoadTest()
    {
        // Error was triggered by selecting a display name, filtering it out, and then doing a script recompilation.
        yield return SelectJobAwaitLoad("BurstInspectorGUITests.MyJob - (IJob)");

        var win = EditorWindow.GetWindow<BurstInspectorGUI>();
        win._searchFieldJobs.SetFocus();
        yield return null;

        // Simulate event for sending "a" as it will filter out the chosen job.
        win.SendEvent(Event.KeyboardEvent("a"));
        yield return null;

        // Send RequestScriptReload to try and trigger the bug
        // and wait for it to return
        EditorUtility.RequestScriptReload();
        yield return new WaitForDomainReload();

        win = EditorWindow.GetWindow<BurstInspectorGUI>();
        // Wait for it to actually initialize.
        yield return _waitForInitialized;

        Assert.IsTrue(win._initialized, "BurstInspector did not initialize properly after script reload");

        win.Close();
    }

    [UnityTest]
    public IEnumerator FontStyleDuringDomainReloadTest()
    {
        // Enter play mod
        yield return new EnterPlayMode();

        // Exit play mode
        yield return new ExitPlayMode();

        // Wait for the inspector to actually reload
        yield return _waitForInitialized;

        var inspectorWindow = EditorWindow.GetWindow<BurstInspectorGUI>();

#if UNITY_2023_1_OR_NEWER
        Assert.AreEqual("RobotoMono-Regular", inspectorWindow._font.name);
#else
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            Assert.AreEqual("Consolas", inspectorWindow._font.name);
        }
        else
        {
            Assert.AreEqual("Courier", inspectorWindow._font.name);
        }
#endif

        inspectorWindow.Close();
    }

    [UnityTest]
    public IEnumerator BranchHoverTest()
    {
        const string jobName = "BurstInspectorGUITests.MyJob - (IJob)";

        yield return SelectJobAwaitLoad(jobName);
        var info = SetupBranchTest();
        var window = EditorWindow.GetWindow<BurstInspectorGUI>();

        window.SendEvent(new Event() { type = EventType.MouseUp, mousePosition = info.mousePos });
        var branch = window._textArea.hoveredBranch;
        yield return null;

        // Close window to avoid it sending more events
        window.Close();

        Assert.AreNotEqual(branch, default(LongTextArea.Branch), "Mouse is not hovering any branch.");
        Assert.AreEqual(info.blockIdx.src, branch.Edge.OriginRef.BlockIndex);
        Assert.AreEqual(info.blockIdx.dst, branch.Edge.LineRef.BlockIndex);
    }

    [UnityTest]
    public IEnumerator ClickBranchTest()
    {
        const string jobName = "BurstInspectorGUITests.MyJob - (IJob)";

        yield return SelectJobAwaitLoad(jobName);
        var info = SetupBranchTest();

        var window = EditorWindow.GetWindow<BurstInspectorGUI>();


        // Seeing if clicking the branch takes us to a spot where branch is still hovered.
        window.SendEvent(new Event() { type = EventType.MouseDown, mousePosition = info.mousePos });
        var branch = window._textArea.hoveredBranch;
        yield return null;

        Assert.AreNotEqual(branch, default(LongTextArea.Branch), "Mouse is not hovering any branch.");
        Assert.AreEqual(info.blockIdx.src, branch.Edge.OriginRef.BlockIndex);
        Assert.AreEqual(info.blockIdx.dst, branch.Edge.LineRef.BlockIndex);

        // Going back again.
        window.SendEvent(new Event() { type = EventType.MouseDown, mousePosition = info.mousePos });
        var branch2 = window._textArea.hoveredBranch;
        yield return null;

        Assert.AreNotEqual(branch2, default(LongTextArea.Branch), "Mouse is not hovering any branch.");
        Assert.AreEqual(info.blockIdx.src, branch2.Edge.OriginRef.BlockIndex);
        Assert.AreEqual(info.blockIdx.dst, branch2.Edge.LineRef.BlockIndex);

        // Close window to avoid it sending more events.
        window.Close();
    }

    private struct InfoThingy
    {
        public (int src, int dst) blockIdx;
        public Vector2 mousePos;
    }

    private InfoThingy SetupBranchTest()
    {
        var window = EditorWindow.GetWindow<BurstInspectorGUI>();

        // Make sure we use fontSize 12:
        window.fontSizeIndex = 4;
        window._textArea.Invalidate();
        window.fixedFontStyle = null;
        // Force window size to actually show branch arrows.
        window.position = new Rect(window.position.x, window.position.y, 390, 405);

        // Sending event to set the displayname, to avoid it resetting _scrollPos because of target change.
        // Sending two events as initial guess for buttonbar width might be off, and it will be a precise calculation after second event.
        window.SendEvent(new Event() { type = EventType.Repaint, mousePosition = new Vector2(window.position.width / 2f, window.position.height / 2f) });
        window.SendEvent(new Event() { type = EventType.Repaint, mousePosition = new Vector2(window.position.width / 2f, window.position.height / 2f) });

        // Setting up for the test.
        // Finding an edge:
        int dstBlockIdx = -1;
        int srcBlockIdx = -1;
        int line = -1;
        for (int idx = 0; idx < window._burstDisassembler.Blocks.Count; idx++)
        {
            var block = window._burstDisassembler.Blocks[idx];
            if (block.Edges != null)
            {
                foreach (var edge in block.Edges)
                {
                    if (edge.Kind == BurstDisassembler.AsmEdgeKind.OutBound)
                    {
                        dstBlockIdx = edge.LineRef.BlockIndex;
                        line = window._textArea.blockLine[dstBlockIdx];
                        if ((dstBlockIdx == idx + 1 && edge.LineRef.LineIndex == 0)) // pointing to next line
                        {
                            continue;
                        }
                        srcBlockIdx = idx;
                        break;
                    }
                }
                if (srcBlockIdx != -1)
                {
                    break;
                }
            }
        }
        if (srcBlockIdx == -1)
        {
            window.Close();
            throw new System.Exception("No edges present in assembly for \"BurstInspectorGUITests.MyJob - (IJob)\"");
        }

        float dist = window._textArea.fontHeight * line;

        float x = (window.position.width - (window._inspectorView.width + BurstInspectorGUI._scrollbarThickness)) + window._textArea.horizontalPad - (2*window._textArea.fontWidth);

        // setting _ScrollPos so end of arrow is at bottom of screen, to make sure there is actually room for the scrolling.
        window._scrollPos = new Vector2(0, dist - window._inspectorView.height * 0.93f);

        // Setting mousePos to bottom of inspector view.
        float topOfInspectorToBranchArrow = window._buttonOverlapInspectorView + 66.5f;//66.5f is the size of space over the treeview of different jobs.

        var mousePos = new Vector2(x, topOfInspectorToBranchArrow + window._inspectorView.height - 0.5f*window._textArea.fontHeight);

        return new InfoThingy() { blockIdx = (srcBlockIdx, dstBlockIdx), mousePos = mousePos};
    }

    public static IEnumerable ValueSource
    {
        get
        {
            yield return "BurstInspectorGUITests.MyJob - (IJob)";
            yield return "BurstReflectionTests.GenericType`1.NestedGeneric`1[System.Int32,System.Single].TestMethod3()";
            yield return "BurstReflectionTests.GenericType`1.NestedNonGeneric[System.Int32].TestMethod2()";
            yield return "BurstReflectionTests.GenericParallelForJob`1[System.Int32] - (IJobParallelFor)";
        }
    }

    [UnityTest]
    public IEnumerator FocusCodeTest([ValueSource(nameof(ValueSource))] string job)
    {
        var win = EditorWindow.GetWindow<BurstInspectorGUI>();

        yield return SelectJobAwaitLoad(job);

        // Doesn't check that it's at the right spot, simply that it actually moves
        Assert.IsFalse(Mathf.Approximately(win._inspectorView.y, 0f), "Inspector view did not change");
        win.Close();
    }

    public static IEnumerable FocusCodeNotBranchesSource
    {
        get
        {
            yield return (1000, false);
            yield return (563, true);
        }
    }
    [UnityTest]
    public IEnumerator FocusCodeNotBranchesTest([ValueSource(nameof(FocusCodeNotBranchesSource))] (int, bool) input)
    {
        var (width, doFocus) = input;
        const string case1 = "BurstInspectorGUITests.BranchArrows - (IJob)";
        const string case2 = "BurstInspectorGUITests.BranchArrows2 - (IJob)";

        var win = EditorWindow.GetWindow<BurstInspectorGUI>();

        // Force window size to be small enough for it to position it more to the right.
        win.position = new Rect(win.position.x, win.position.y, width, 405);

        // Test one where it should focus.
        yield return SelectJobAwaitLoad(case1);

        var val1 = win._inspectorView.x;
        var result1 = Mathf.Approximately(val1, 0f);


        // Test two with no focus.
        win._assemblyKind = BurstInspectorGUI.AssemblyOptions.PlainWithDebugInformation;
        yield return SelectJobAwaitLoad(case2);

        var val2 = win._inspectorView.x;
        var result2 = Mathf.Approximately(val2, 0f);

        // Cleanup and test assertions.
        win.Close();
        Assert.AreEqual(doFocus, result1 == doFocus, $"Inspector view wrong.");
        //Assert.IsFalse(result1, $"Inspector view did not change (Is {val1}).");
        Assert.IsTrue(result2, $"Inspector view changed unexpectedly (Is {val2}).");
    }

    [UnityTest]
    public IEnumerator SelectionNotOutsideBoundsTest()
    {
        void MoveSelection(BurstInspectorGUI gui, LongTextArea.Direction dir)
        {
            switch (dir)
            {
                case LongTextArea.Direction.Down:
                    gui._textArea.SelectAll();
                    gui._textArea.MoveSelectionDown(gui._inspectorView, true);
                    break;
                case LongTextArea.Direction.Right:
                    gui._textArea.SelectAll();
                    gui._textArea.MoveSelectionRight(gui._inspectorView, true);
                    break;
                case LongTextArea.Direction.Left:
                    gui._textArea.selectDragPos = Vector2.zero;
                    gui._textArea.MoveSelectionLeft(gui._inspectorView, true);
                    break;
                case LongTextArea.Direction.Up:
                    gui._textArea.selectDragPos = Vector2.zero;
                    gui._textArea.MoveSelectionUp(gui._inspectorView, true);
                    break;
            }
        }

        var win = EditorWindow.GetWindow<BurstInspectorGUI>();

        yield return SelectJobAwaitLoad("BurstInspectorGUITests.MyJob - (IJob)");

        try
        {
            foreach (var dir in Enum.GetValues(typeof(LongTextArea.Direction)))
            {
                MoveSelection(win, (LongTextArea.Direction)dir);
                yield return null;

                // Check that no errors have happened.
                LogAssert.NoUnexpectedReceived();
            }
        }
        finally
        {
            win.Close();
        }
    }

    [UnityTest]
    public IEnumerator SelectionInAssemblySearchBarTest()
    {
        yield return SelectJobAwaitLoad("BurstInspectorGUITests.MyJob - (IJob)");
        var win = EditorWindow.GetWindow<BurstInspectorGUI>();

        win._searchFieldAssembly.SetFocus();
        yield return null;

        // Send events to input some text.
        win.SendEvent(Event.KeyboardEvent("a"));
        win.SendEvent(Event.KeyboardEvent("b"));

        yield return null;

        // Move select some using keyboard input
        win.SendEvent(Event.KeyboardEvent("left"));
        win.SendEvent(Event.KeyboardEvent("#right"));

        yield return null;

        // Do a copy
        var savedClipBoard = EditorGUIUtility.systemCopyBuffer;
        win.SendEvent(SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX
            ? Event.KeyboardEvent("%c")
            : Event.KeyboardEvent("^c"));
        yield return null;

        var copiedText = EditorGUIUtility.systemCopyBuffer;
        EditorGUIUtility.systemCopyBuffer = savedClipBoard;

        // Check that all is good
        win.Close();

        Assert.AreEqual("b", copiedText, "Copied text did not match expectation.");
    }

    [UnityTest]
    public IEnumerator GoToNextSearchTargetTest()
    {
        var active = -1;
        var nextActive = -1;

        yield return SelectJobAwaitLoad("BurstInspectorGUITests.MyJob - (IJob)");
        var win = EditorWindow.GetWindow<BurstInspectorGUI>();

        try
        {
            win._searchFieldAssembly.SetFocus();
            yield return null;

            // Do a search in the text.
            win.SendEvent(Event.KeyboardEvent("p"));
            win.SendEvent(Event.KeyboardEvent("u"));
            win.SendEvent(Event.KeyboardEvent("return"));
            yield return null;

            active = win._textArea._activeSearchHitIdx;

            // Select next search target.
            win.SendEvent(Event.KeyboardEvent("return"));
            yield return null;

            nextActive = win._textArea._activeSearchHitIdx;
        }
        finally
        {
            win.Close();
        }

        Assert.AreNotEqual(active, nextActive, "Active search target was not changed.");
    }

    [Test]
    public void CorrectFormattingOfAssembly()
    {
        EditorWindow.GetWindow<BurstInspectorGUI>().Close();

        const string assemblyName = "BurstInspectorGUITests.MyJob - (IJob)";
        var textArea = new LongTextArea();

        foreach (BurstTargetCpu target in Enum.GetValues(typeof(BurstTargetCpu)))
        {
            if (target == BurstTargetCpu.WASM32)
            {
                // We do not draw flow-lines for wasm yet, so skip this one.
                continue;
            }

            var disassembler = BurstInspectorTestUtil.GetDisassemblerAndText(assemblyName, 1, target, out var assembly);

            var asmKind = BurstInspectorGUI.FetchAsmKind(target, DisassemblyKind.Asm);
            textArea.SetText(
                "random",
                assembly,
                true,
                disassembler,
                disassembler.Initialize(
                    assembly,
                    asmKind
                )
            );
            // Check that it found branches. This indicates whether we formatted as the correct assembly kind.
            Assert.Greater(textArea.MaxLineDepth, 0, $"Failed with {target}");
        }
    }

    [UnityTest]
    public IEnumerator FocusSpecialCharacterCode()
    {
        var win = EditorWindow.GetWindow<BurstInspectorGUI>();

        yield return SelectJobAwaitLoad("BurstInspectorGUITests.CustomJobWithSpecialCharacter - (BurstInspectorGUITests.ICustomJob`1[System.Single])");

        // Check that there are no errors.
        LogAssert.NoUnexpectedReceived();

        win.Close();
    }

    [BurstCompile]
    private struct MyJob : IJob
    {
        [ReadOnly]
        public NativeArray<float> Inpút;

        [WriteOnly]
        public NativeArray<float> Output;

        public void Execute()
        {
            float result = 0.0f;
            for (int i = 0; i < Inpút.Length; i++)
            {
                result += Inpút[i];
            }
            Output[0] = result;
        }
    }

    [BurstCompile]
    private struct BranchArrows : IJob
    {
        [ReadOnly]
        public NativeArray<float> Inpút;

        [WriteOnly]
        public NativeArray<float> Output;

        public void Execute()
        {
            float result = 0.0f;
            for (int i = 0; i < Inpút.Length; i++)
            {
                if (Inpút[i] < 10) { result += 1; }
                else if (Inpút[i] < 20) { result += 2; }
                else if (Inpút[i] < 30) { result += 3; }
                else if (Inpút[i] < 40) { result += 4; }
                else if (Inpút[i] < 50) { result += 5; }
                else if (Inpút[i] < 60) { result += 6; }
                else if (Inpút[i] < 70) { result += 7; }
                else if (Inpút[i] < 80) { result += 8; }
                else if (Inpút[i] < 90) { result += 9; }
                result += Inpút[i];
            }
            Output[0] = result;
        }
    }

    [BurstCompile]
    private struct BranchArrows2 : IJob
    {
        [ReadOnly]
        public NativeArray<float> Inpút;

        [WriteOnly]
        public NativeArray<float> Output;

        public void Execute()
        {
            float result = 0.0f;
            for (int i = 0; i < Inpút.Length; i++)
            {
                if (Inpút[i] < 10) { result += 1; }
                else if (Inpút[i] < 20) { result += 2; }
                else if (Inpút[i] < 30) { result += 3; }
                else if (Inpút[i] < 40) { result += 4; }
                else if (Inpút[i] < 50) { result += 5; }
                else if (Inpút[i] < 60) { result += 6; }
                else if (Inpút[i] < 70) { result += 7; }
                else if (Inpút[i] < 80) { result += 8; }
                else if (Inpút[i] < 90) { result += 9; }
                result += Inpút[i];
            }
            Output[0] = result;
        }
    }

    private class BurstDisassemblerWithCopy : BurstDisassembler
    {
        public List<AsmBlock> BlocksCopy;
        public bool IsColoredCopy;
        public List<AsmLine> LinesCopy;
        public List<AsmToken> TokensCopy;
        public BurstDisassemblerWithCopy(BurstDisassembler disassembler) : base()
        {
            IsColoredCopy = disassembler.IsColored;

            BlocksCopy = new List<AsmBlock>(disassembler.Blocks);
            LinesCopy = new List<AsmLine>(disassembler.Lines);
            TokensCopy = new List<AsmToken>(disassembler.Tokens);
        }

        public bool Equals(BurstDisassembler other)
        {
            return IsColoredCopy == other.IsColored
                && BlocksCopy.SequenceEqual(other.Blocks)
                && LinesCopy.SequenceEqual(other.Lines)
                && TokensCopy.SequenceEqual(other.Tokens);
        }
    }

    [BurstCompile]
    public struct CustomJobWithSpecialCharacter : ICustomJob<float>
    {
        public void Execute(float test)
        {
            test = 5;
        }
    }

    [JobProducerType(typeof(CustomJobExtensions.JobStruct<,>))]
    private interface ICustomJob<T> where T : struct
    {
        void Execute(T item);
    }

    private static class CustomJobExtensions
    {
        public struct JobStruct<TJ, T>
            where TJ : struct, ICustomJob<T>
            where T : struct
        {

            [BurstCompile]
            public struct Data
            {
                public T additionalData;
                public TJ job;
            }

            private static void Execute(ref Data data)
            {
                data.job.Execute(data.additionalData);
            }
        }
    }
    private class CustomJobScheduler
    {
        private void ScheduleJob()
        {
            var job = new CustomJobWithSpecialCharacter();
            job.Execute(10);
        }
    }
}
