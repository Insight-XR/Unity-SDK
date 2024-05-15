using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Unity.Burst.Editor;
using System.Text;
using Unity.Burst;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.TestTools.Utils;
using System.Runtime.CompilerServices;


[TestFixture]
public class LongTextAreaTests
{
    private LongTextArea _textArea;

    [OneTimeSetUp]
    public void SetUp()
    {
        _textArea = new LongTextArea();
    }

    [Test]
    [TestCase("", "        push        rbp\n        .seh_pushreg rbp\n", 7, true)]
    [TestCase("<color=#CCCCCC>", "        push        rbp\n        .seh_pushreg rbp\n", 25, true)]
    [TestCase("<color=#d7ba7d>", "        push        rbp\n        .seh_pushreg rbp\n", 21 + 15 + 8 + 15, true)]
    [TestCase("", "\n# hulahop    hejsa\n", 5, false)]
    public void GetStartingColorTagTest(string tag, string text, int textIdx, bool syntaxHighlight)
    {
        var disAssembler = new BurstDisassembler();
        _textArea.SetText("", text, true, disAssembler, disAssembler.Initialize(text, BurstDisassembler.AsmKind.Intel, true, syntaxHighlight));
        if (!_textArea.CopyColorTags) _textArea.ChangeCopyMode();

        Assert.That(_textArea.GetStartingColorTag(0, textIdx), Is.EqualTo(tag));
    }

    [Test]
    [TestCase("", "        push        rbp\n        .seh_pushreg rbp\n", 7, true)]
    [TestCase("</color>", "        push        rbp\n        .seh_pushreg rbp\n", 25, true)]
    [TestCase("</color>", "        push        rbp\n        .seh_pushreg rbp\n", 21 + 15 + 8 + 15, true)]
    [TestCase("", "        push        rbp\n        .seh_pushreg rbp\n", 14 + 15 + 8, true)]
    [TestCase("", "\n# hulahop    hejsa\n", 5, false)]
    public void GetEndingColorTagTest(string tag, string text, int textIdx, bool syntaxHighlight)
    {
        var disAssembler = new BurstDisassembler();
        _textArea.SetText("", text, true, disAssembler, disAssembler.Initialize(text, BurstDisassembler.AsmKind.Intel, true, syntaxHighlight));
        if (!_textArea.CopyColorTags) _textArea.ChangeCopyMode();

        Assert.That(_textArea.GetEndingColorTag(0, textIdx), Is.EqualTo(tag));
    }

    [Test]
    [TestCase("<color=#FFFF00>hulahop</color>    <color=#DCDCAA>hejsa</color>\n", 0, 16, 16)]
    [TestCase("<color=#FFFF00>hulahop</color>\n    <color=#DCDCAA>hejsa</color>\n", 1, 40, 9)]
    [TestCase("<color=#FFFF00>hulahop</color>\n    <color=#DCDCAA>hejsa</color>\n hej", 2, 67, 3)]
    [TestCase("<color=#FFFF00>hulahop</color>    <color=#DCDCAA>hejsa</color>", 0, 15, 15)]
    [TestCase("\n        <color=#4EC9B0>je</color>                <color=#d4d4d4>.LBB11_4</color>", 1, 34, 33)]
    // Test cases for when on enhanced text and not coloured.
    [TestCase("hulahop    hejsa\n", 0, 16, 16)]
    [TestCase("hulahop\n    hejsa\n", 1, 17, 9)]
    [TestCase("hulahop\n    hejsa\n hej", 2, 21, 3)]
    [TestCase("hulahop    hejsa", 0, 15, 15)]
    public void GetEndIndexOfColoredLineTest(string text, int line, int resTotal, int resRel)
    {
        Assert.That(_textArea.GetEndIndexOfColoredLine(text, line), Is.EqualTo((resTotal, resRel)));
    }

    [Test]
    [TestCase("hulahop    hejsa\n", 0, 16, 16)]
    [TestCase("hulahop\n    hejsa\n", 1, 17, 9)]
    [TestCase("hulahop\n    hejsa\n hej", 2, 21, 3)]
    [TestCase("hulahop    hejsa", 0, 15, 15)]
    [TestCase("\nhulahop    hejsa", 1, 16, 15)]
    public void GetEndIndexOfPlainLineTest(string text, int line, int resTotal, int resRel)
    {
        Assert.That(_textArea.GetEndIndexOfPlainLine(text, line), Is.EqualTo((resTotal, resRel)));
    }

    [Test]
    [TestCase("<color=#FFFF00>hulahop</color>\n    <color=#DCDCAA>hejsa</color>\n hej", 2, 2, 0)]
    [TestCase("<color=#FFFF00>hulahop</color>\n    <color=#DCDCAA>hejsa</color>\n hej", 1, 5, 15)]
    [TestCase("<color=#FFFF00>hulahop</color>    <color=#DCDCAA>hejsa</color>:", 0, 17, 46)]
    public void BumpSelectionXByColortagTest(string text, int lineNum, int charsIn, int colourTagFiller)
    {
        var (idxTotal, idxRel) = _textArea.GetEndIndexOfColoredLine(text, lineNum);
        Assert.That(_textArea.BumpSelectionXByColorTag(text, idxTotal - idxRel, charsIn), Is.EqualTo(charsIn + colourTagFiller));
    }

    [Test]
    [TestCase("        push        rbp\n        .seh_pushreg rbp\n", false)]
    [TestCase("        push        rbp\n        .seh_pushreg rbp\n", true)]
    public void SelectAllTest(string text, bool useDisassembler)
    {
        if (useDisassembler)
        {
            var disAssembler = new BurstDisassembler();
            _textArea.SetText("", text, true, disAssembler, disAssembler.Initialize(text, BurstDisassembler.AsmKind.Intel));
            _textArea.LayoutEnhanced(GUIStyle.none, Rect.zero, true);
        }
        else
        {
            _textArea.SetText("", text, true, null, false);
        }


        _textArea.selectPos = new Vector2(2, 2);
        // There is no inserted comments or similar in my test example, so finalAreaSize, should be equivalent for the two.
        _textArea.finalAreaSize = new Vector2(7.5f * text.Length, 15.2f);

        _textArea.SelectAll();
        Assert.That(_textArea.selectPos, Is.EqualTo(Vector2.zero));
        Assert.That(_textArea.selectDragPos, Is.EqualTo(new Vector2(7.5f * text.Length, 15.2f)));

        if (!useDisassembler)
        {
            Assert.That(_textArea.textSelectionIdx, Is.EqualTo((0, text.Length)));
        }
    }

    private BurstDisassembler GetDisassemblerandText(string compileTargetName, int debugLvl, out string textToRender)
    {
        // Get target job assembly:
        var assemblies = BurstReflection.EditorAssembliesThatCanPossiblyContainJobs;
        var result = BurstReflection.FindExecuteMethods(assemblies, BurstReflectionAssemblyOptions.None);
        var compileTarget =  result.CompileTargets.Find(x => x.GetDisplayName() == compileTargetName);

        Assert.IsTrue(compileTarget != default, $"Could not find compile target: {compileTarget}");

        BurstDisassembler disassembler = new BurstDisassembler();

        var options = new StringBuilder();

        compileTarget.Options.TryGetOptions(compileTarget.JobType, out string defaultOptions);
        options.AppendLine(defaultOptions);
        // Disables the 2 current warnings generated from code (since they clutter up the inspector display)
        // BC1370 - throw inside code not guarded with ConditionalSafetyCheck attribute
        // BC1322 - loop intrinsic on loop that has been optimised away
        options.AppendLine($"{BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionDisableWarnings, "BC1370;BC1322")}");

        options.AppendLine($"{BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionTarget, BurstTargetCpu.X64_SSE4)}");

        options.AppendLine($"{BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionDebug, $"{debugLvl}")}");

        var baseOptions = options.ToString();

        var append = BurstInspectorGUI.GetDisasmOptions()[(int)DisassemblyKind.Asm];

        // Setup disAssembler with the job:
        compileTarget.RawDisassembly = BurstInspectorGUI.GetDisassembly(compileTarget.Method, baseOptions + append);
        textToRender = compileTarget.RawDisassembly.TrimStart('\n');

        return disassembler;
    }

    [Test]
    [TestCase(true, true, 2)]
    [TestCase(true, true, 1)]
    [TestCase(true, false, 2)]
    [TestCase(true, false, 1)]
    [TestCase(false, true, 2)]
    [TestCase(false, true, 1)]
    [TestCase(false, false, 2)]
    [TestCase(false, false, 1)]
    public void CopyAllTest(bool useDisassembler, bool coloured, int debugLvl)
    {
        // Get target job assembly:
        var disassembler = new BurstDisassembler();
        var thisPath = Path.GetDirectoryName(GetThisFilePath());
        var textToRender = File.ReadAllText(Path.Combine(thisPath, _burstJobPath));


        if (useDisassembler)
        {
            _textArea.SetText("", textToRender, true, disassembler, disassembler.Initialize(textToRender, BurstDisassembler.AsmKind.Intel, true, coloured));
            _textArea.ExpandAllBlocks();

            var builder = new StringBuilder();

            for (int i = 0; i < disassembler.Blocks.Count; i++)
            {
                builder.Append(disassembler.GetOrRenderBlockToText(i));
            }

            textToRender = builder.ToString();
        }
        else
        {
            _textArea.SetText("", textToRender, true, null, false);
        }

        _textArea.Layout(GUIStyle.none, _textArea.horizontalPad);

        _textArea.SelectAll();
        _textArea.DoSelectionCopy();

        Assert.AreEqual(textToRender, EditorGUIUtility.systemCopyBuffer);
    }

    [Test]
    public void CopyAllTextWithoutColorTagsTest()
    {
        // Setup:
        var disassembler = new BurstDisassembler();
        var thisPath = Path.GetDirectoryName(GetThisFilePath());
        var textToRender = File.ReadAllText(Path.Combine(thisPath, _burstJobPath));

        _textArea.SetText("", textToRender, true, disassembler,
            disassembler.Initialize(
                textToRender,
                BurstDisassembler.AsmKind.Intel));

        _textArea.Layout(GUIStyle.none, _textArea.horizontalPad);
        _textArea.LayoutEnhanced(GUIStyle.none, Rect.zero, true);

        // Actual test to reproduce error:
        _textArea.ChangeCopyMode();
        _textArea.SelectAll();
        Assert.DoesNotThrow(_textArea.DoSelectionCopy);
    }

    [Test]
    public void CopyTextAfterSelectionMovedTest()
    {
        // Setup:
        const bool sbm = true;
        var wa = Rect.zero;

        var disassembler = new BurstDisassembler();
        var thisPath = Path.GetDirectoryName(GetThisFilePath());
        var textToRender = File.ReadAllText(Path.Combine(thisPath, _burstJobPath));

        _textArea.SetText("", textToRender, true, disassembler,
            disassembler.Initialize(
                textToRender,
                BurstDisassembler.AsmKind.Intel));

        _textArea.Layout(GUIStyle.none, _textArea.horizontalPad);
        _textArea.LayoutEnhanced(GUIStyle.none, Rect.zero, sbm);

        // Actual test to reproduce error:
        _textArea.ChangeCopyMode();

        _textArea.MoveSelectionDown(wa, sbm);
        _textArea.MoveSelectionDown(wa, sbm);
        _textArea.MoveSelectionLeft(wa, sbm);

        Assert.DoesNotThrow(_textArea.DoSelectionCopy);

        _textArea.MoveSelectionRight(wa, sbm);
        Assert.DoesNotThrow(_textArea.DoSelectionCopy);
    }

    [Test]
    public void CopyTextIdenticalWithAndWithoutColorTags()
    {
        // We don't wanna go messing with the users system buffer. At least if user didn't break anything.
        var savedSystemBuffer = EditorGUIUtility.systemCopyBuffer;

        // Get target job assembly:
        var disassembler = new BurstDisassembler();
        var thisPath = Path.GetDirectoryName(GetThisFilePath());
        var textToRender = File.ReadAllText(Path.Combine(thisPath, _burstJobPath));

        _textArea.SetText("", textToRender, true, disassembler,
            disassembler.Initialize(
                textToRender,
                BurstDisassembler.AsmKind.Intel));

        _textArea.Layout(GUIStyle.none, _textArea.horizontalPad);
        _textArea.LayoutEnhanced(GUIStyle.none, Rect.zero, true);
        for (var i=0; i<disassembler.Blocks[0].Length+50; i++) _textArea.MoveSelectionDown(Rect.zero, true);

        _textArea.LayoutEnhanced(GUIStyle.none, Rect.zero, true);
        _textArea.UpdateEnhancedSelectTextIdx(_textArea.horizontalPad);

        _textArea.DoSelectionCopy();
        var copiedText1 = EditorGUIUtility.systemCopyBuffer;

        _textArea.ChangeCopyMode();
        _textArea.DoSelectionCopy();
        var copiedText2 = EditorGUIUtility.systemCopyBuffer;

        var regx = new Regex(@"(<color=#[0-9A-Za-z]*>)|(</color>)");

        if (!_textArea.CopyColorTags)
        {
            (copiedText1, copiedText2) = (copiedText2, copiedText1);
        }
        copiedText2 = regx.Replace(copiedText2, "");

        EditorGUIUtility.systemCopyBuffer = savedSystemBuffer;
        Assert.AreEqual(copiedText1, copiedText2,
            "Copy with color tags did not match copy without " +
            "(Color tags is removed from the copy to make it comparable with the color-tag-less copy).");
    }

    // Disabled due to https://jira.unity3d.com/browse/BUR-2207
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void KeepingSelectionWhenMovingTest(bool useDisassembler)
    {
        const string jobName = "BurstInspectorGUITests.MyJob - (IJob)";
        BurstDisassembler disassembler = new BurstDisassembler();
        var thisPath = Path.GetDirectoryName(GetThisFilePath());
        var textToRender = File.ReadAllText(Path.Combine(thisPath, _burstJobPath));
        Rect workingArea = new Rect();

        if (useDisassembler)
        {
            _textArea.SetText(jobName, textToRender, true, disassembler, disassembler.Initialize(textToRender, BurstDisassembler.AsmKind.Intel));
            _textArea.LayoutEnhanced(GUIStyle.none, workingArea, true);
        }
        else
        {
            _textArea.SetText(jobName, textToRender, false, null, false);
        }
        _textArea.Layout(GUIStyle.none, _textArea.horizontalPad);

        Assert.IsFalse(_textArea.HasSelection);

        Vector2 start = _textArea.selectDragPos;
        if (useDisassembler) start.x = _textArea.horizontalPad + _textArea.fontWidth / 2;

        // Horizontal movement:
        _textArea.MoveSelectionRight(workingArea, true);
        Assert.IsTrue(_textArea.HasSelection);
        Assert.AreEqual(start + new Vector2(_textArea.fontWidth, 0), _textArea.selectDragPos);

        _textArea.MoveSelectionLeft(workingArea, true);
        Assert.IsTrue(_textArea.HasSelection);
        Assert.AreEqual(start, _textArea.selectDragPos);

        // Vertical movement:
        _textArea.MoveSelectionDown(workingArea, true);
        Assert.IsTrue(_textArea.HasSelection);
        Assert.AreEqual(start + new Vector2(0, _textArea.fontHeight), _textArea.selectDragPos);

        _textArea.MoveSelectionUp(workingArea, true);
        Assert.IsTrue(_textArea.HasSelection);
        Assert.AreEqual(start, _textArea.selectDragPos);
    }

    [Test]
    public void GetFragNrFromBlockIdxTest()
    {
        var disassembler = new BurstDisassembler();
        var thisPath = Path.GetDirectoryName(GetThisFilePath());
        var textToRender = File.ReadAllText(Path.Combine(thisPath, _burstJobPath));

        _textArea.SetText("", textToRender, true, disassembler,
            disassembler.Initialize(textToRender, BurstDisassembler.AsmKind.Intel, false, false));


        var garbageVariable = 0f;
        var numBlocks = disassembler.Blocks.Count;

        // Want to get the last fragment possible
        var expectedFragNr = 0;
        for (var i = 0; i < _textArea.blocksFragmentsPlain.Length-1; i++)
        {
            expectedFragNr += _textArea.GetPlainFragments(i).Count;
        }

        Assert.AreEqual(expectedFragNr, _textArea.GetFragNrFromBlockIdx(numBlocks-1, 0, 0, ref garbageVariable));

        Assert.AreEqual(3, _textArea.GetFragNrFromBlockIdx(3, 1, 1, ref garbageVariable));
    }

    [Test]
    public void GetFragNrFromEnhancedTextIdxTest()
    {
        const string jobName = "BurstJobTester2.MyJob - (IJob)";

        var disassembler = new BurstDisassembler();
        var thisPath = Path.GetDirectoryName(GetThisFilePath());
        var textToRender = File.ReadAllText(Path.Combine(thisPath, _burstJobPath));
        _textArea.SetText(jobName, textToRender, true, disassembler,
            disassembler.Initialize(textToRender, BurstDisassembler.AsmKind.Intel, false, false));
        _textArea.Layout(GUIStyle.none, 0);

        var garbageVariable = 0f;
        const int blockIdx = 2;

        var fragments = _textArea.RecomputeFragmentsFromBlock(blockIdx);
        var text = _textArea.GetText;
        var expectedFrag = blockIdx + fragments.Count - 1;

        var info = disassembler.BlockIdxs[blockIdx];

        var extraFragLen = fragments.Count > 1
            ? fragments[0].text.Length + 1 // job only contains 2 fragments at max
            : 0;

        var idx = info.startIdx + extraFragLen + 1;

        var expectedPosY = garbageVariable;
        for (int i = 0; i < blockIdx; i++)
        {
            foreach (var frag in _textArea.RecomputeFragmentsFromBlock(i))
            {
                expectedPosY += frag.lineCount * _textArea.fontHeight;
            }
        }

        var expected =
            ( expectedFrag
                , info.startIdx + extraFragLen
                , expectedPosY);
        var actual = _textArea.GetFragNrFromEnhancedTextIdx(idx, 0, 0, 0, garbageVariable);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SearchTextEnhancedTest(bool colored)
    {
        var disassembler = new BurstDisassembler();
        var thisPath = Path.GetDirectoryName(GetThisFilePath());
        var textToRender = File.ReadAllText(Path.Combine(thisPath, _burstJobPath));
        _textArea.SetText("", textToRender, true, disassembler, disassembler.Initialize(textToRender, BurstDisassembler.AsmKind.Intel, true, colored));

        var workingArea = new Rect(0, 0, 10, 10);
        _textArea.SearchText(new SearchCriteria(".Ltmp.:", true, false, true), new Regex(@"\.Ltmp.:"), ref workingArea);

        Assert.AreEqual(10, _textArea.NrSearchHits);

        // Check that they are filled out probably
        int nr = 0;
        foreach (var fragHits in _textArea.searchHits.Values)
        {
            foreach (var hit in fragHits)
            {
                Assert.AreEqual((0, 7, nr++), hit);
            }
        }
    }

    [Test]
    public void SelectOnOneLineTest()
    {
        const string testCase = "\n<color=#d4d4d4>.Ltmp12</color>: ...";

        var disassembler = new BurstDisassembler();
        _textArea.SetText("", testCase, false, disassembler, disassembler.Initialize(testCase, BurstDisassembler.AsmKind.Intel));

        // Set fontWidth and fontHeight
        _textArea.Layout(GUIStyle.none, 20f);

        // Set selection markers.
        // Error happened when selection started at the lowest point of a line.
        _textArea.selectPos = new Vector2(0, _textArea.fontHeight);
        // Select further down to make sure it wont be switched with selectPos.
        _textArea.selectDragPos = new Vector2(10 * _textArea.fontWidth, _textArea.fontHeight*2);

        // Hopefully it wont throw anything
        Assert.DoesNotThrow(() =>
            _textArea.PrepareInfoForSelection(0, 0, _textArea.fontHeight,
                new LongTextArea.Fragment() { text = testCase.TrimStart('\n'), lineCount = 1 },
                _textArea.GetEndIndexOfColoredLine));
    }

    [Test]
    public void GetLineHighlightTest()
    {
        const float hPad = 20f;
        const int linePressed = 4 + 13;
        // Get target job assembly:
        var disassembler = new BurstDisassembler();
        var thisPath = Path.GetDirectoryName(GetThisFilePath());
        var textToRender = File.ReadAllText(Path.Combine(thisPath, _burstJobPath));

        // Set up dependencies for GetLineHighlight(.)
        _textArea.SetText("", textToRender, true, disassembler,
            disassembler.Initialize(
                textToRender,
                BurstDisassembler.AsmKind.Intel)
        );

        _textArea.Layout(GUIStyle.none, hPad);
        _textArea.LayoutEnhanced(GUIStyle.none,
            new Rect(0,0, _textArea.fontWidth*50,_textArea.fontHeight*50),
            false
        );

        // Setup simple cache.
        var cache = new LongTextArea.LineRegRectsCache();
        var rect = _textArea.GetLineHighlight(ref cache, hPad, linePressed);
        Assert.IsFalse(cache.IsRegistersCached(linePressed));
        Assert.IsTrue(cache.IsLineHighlightCached(linePressed, false));

        var expectedX = hPad;
        var b = 0;
        for (; b < disassembler.Blocks.Count; b++)
        {
            if (disassembler.Blocks[b].LineIndex > linePressed)
            {
                b--;
                break;
            }
        }

        var expectedY = (_textArea.blockLine[b] + (linePressed - disassembler.Blocks[b].LineIndex)) * _textArea.fontHeight + _textArea.fontHeight;
        var lineStr = _textArea.GetLineString(disassembler.Lines[linePressed]);
        var lineLen = lineStr.Length * _textArea.fontWidth;

        var expected = new Rect(expectedX,
            expectedY,
            lineLen,
            2f
        );

        var result = Mathf.Approximately(expectedX, rect.x)
                     && Mathf.Approximately(expectedY, rect.y)
                     && Mathf.Approximately(lineLen, rect.width)
                     && Mathf.Approximately(2f, rect.height);

        Assert.IsTrue(result, $"line highlight for \"{lineStr}\" was wrong.\nExpected: {expected}\nBut was: {rect}");
    }


    [Test]
    public void GetRegRectsTest()
    {
        #region Initialize-test-states
        const float hPad = 20f;
        const int linePressed = 8 + 13;
        // Get target job assembly:
        var disassembler = new BurstDisassembler();
        var thisPath = Path.GetDirectoryName(GetThisFilePath());
        var textToRender = File.ReadAllText(Path.Combine(thisPath, _burstJobPath));

        // Set up dependencies for GetLineHighlight(.)
        _textArea.SetText("", textToRender, true, disassembler,
            disassembler.Initialize(
                textToRender,
                BurstDisassembler.AsmKind.Intel)
        );
        // Setting up variables to determine view size:
        _textArea.Layout(GUIStyle.none, hPad);
        _textArea.LayoutEnhanced(GUIStyle.none, Rect.zero, false);
        #endregion

        // Find the block index to put within view:
        var blockIdx = disassembler.Blocks.Count/2;
        for (; blockIdx > 0; blockIdx--)
        {
            // Take the first block where we know the lastLine will be in the next block.
            if (!_textArea._folded[blockIdx + 1] && disassembler.Blocks[blockIdx].Length >= 5) break;
        }
        // Initialize states with regards to view:
        _textArea.Layout(GUIStyle.none, hPad);
        _textArea.LayoutEnhanced(GUIStyle.none,
            new Rect(0,0, _textArea.fontWidth*100,_textArea.fontHeight*(_textArea.blockLine[blockIdx]+1)),
            false
        );

        #region Function-to-test-call
        var cache = new LongTextArea.LineRegRectsCache();
        var registersUsed = new List<string> { "rbp", "rsp" };
        var rects = _textArea.GetRegisterRects(hPad, ref cache, linePressed, registersUsed);
        #endregion
        #region Expected-variables
        var lastLine = disassembler.Blocks[_textArea._renderBlockEnd+1].LineIndex + 4;

        var expectedRbp =
            (from pair in disassembler._registersUsedAtLine._linesRegisters.TakeWhile(x => x.Key < lastLine)
                where pair.Value.Contains("rbp") && disassembler.Lines[pair.Key].Kind != BurstDisassembler.AsmLineKind.Directive
                select pair);
        var expectedRsp =
            (from pair in disassembler._registersUsedAtLine._linesRegisters.TakeWhile(x => x.Key < lastLine)
                where pair.Value.Contains("rsp") && disassembler.Lines[pair.Key].Kind != BurstDisassembler.AsmLineKind.Directive
                select pair);

        // Check that they are correctly placed!
        // Only check the last here, as under development the "hardest" behaviour was from within the lowest blocks.
        var lastRectLineIdx = expectedRbp.Last().Key;
        var lastRectLine = disassembler.Lines[lastRectLineIdx];
        var lastRectLineStr = _textArea.GetLineString(lastRectLine);

        var expectedX = lastRectLineStr.Substring(0, lastRectLineStr.IndexOf("rbp")).Length * _textArea.fontWidth + hPad + 2f;
        #endregion

        Assert.IsTrue(cache.IsRegistersCached(linePressed), "Register Rect cache not probarly setup.");
        Assert.IsFalse(cache.IsLineHighlightCached(linePressed, false), "Line highlight cache faultily set to cached.");

        Assert.AreEqual(2, rects.Length, "Register Rect cache does not have correct number of registered registers.");
        Assert.AreEqual(expectedRbp.Count(), rects[0].Count, "Did not find all \"rbp\" registers.");
        Assert.AreEqual(expectedRsp.Count(), rects[1].Count, "Did not find all \"rsp\" registers.");
        Assert.That(rects[0][rects[0].Count - 1].x, Is.EqualTo(expectedX).Using(FloatEqualityComparer.Instance),
            "Wrong x position for last found \"rbp\" rect.");
        // Note: Does not check Y position, as this is highly dependent on architecture, making it annoyingly hard
        // to reason about.
    }


    [Test]
    public void RegsRectCacheTest()
    {
        const float hPad = 20f;
        const int linePressed = 8 + 13;
        // Get target job assembly:
        var disassembler = new BurstDisassembler();
        var thisPath = Path.GetDirectoryName(GetThisFilePath());
        var textToRender = File.ReadAllText(Path.Combine(thisPath, _burstJobPath));

        // Set up dependencies for GetLineHighlight(.)
        _textArea.SetText("", textToRender, true, disassembler,
            disassembler.Initialize(
                textToRender,
                BurstDisassembler.AsmKind.Intel)
        );

        _textArea.Layout(GUIStyle.none, hPad);
        var yStart = 0f;
        var yHeight = _textArea.fontHeight*44;
        _textArea.LayoutEnhanced(GUIStyle.none,
            new Rect(0,yStart, _textArea.fontWidth*100,yHeight),
            false
        );

        var cache = new LongTextArea.LineRegRectsCache();
        var registersUsed = new List<string> { "rbp", "rsp" };
        var rects = _textArea.GetRegisterRects(hPad, ref cache, linePressed, registersUsed);
        Assert.IsTrue(cache.IsRegistersCached(linePressed));
        var cachedItems =
            (from elm in rects
                select elm.Count).Sum();

        yStart = yHeight;
        _textArea.Layout(GUIStyle.none, hPad);
        _textArea.LayoutEnhanced(GUIStyle.none,
            new Rect(0,yStart, _textArea.fontWidth*100,yHeight),
            false
        );

        rects = _textArea.GetRegisterRects(hPad, ref cache, linePressed, registersUsed);
        Assert.IsTrue(cache.IsRegistersCached(linePressed));
        var cachedItems2 =
            (from elm in rects
                select elm.Count).Sum();
        Assert.IsTrue(cachedItems2 >= cachedItems);
    }

    [Test]
    [TestCase("\n        xor               r9d, r9d\n", "r9d")]
    [TestCase("\n        push              edx  rdx\n", "rdx")]
    public void SameRegisterUsedTwiceTest(string line, string reg)
    {
        const float hPad = 20f;
        const int linePressed = 0;

        // Get target job assembly:
        var disassembler = new BurstDisassembler();

        // Set up dependencies for GetLineHighlight(.)
        _textArea.SetText("", line, true, disassembler,
            disassembler.Initialize(
                line,
                BurstDisassembler.AsmKind.Intel)
        );

        _textArea.Layout(GUIStyle.none, hPad);
        var yStart = 0f;
        var yHeight = _textArea.fontHeight;
        _textArea.LayoutEnhanced(GUIStyle.none,
            new Rect(0,yStart, _textArea.fontWidth*100,yHeight),
            false
        );

        var cache = new LongTextArea.LineRegRectsCache();
        var registersUsed = new List<string> { reg };
        var rects = _textArea.GetRegisterRects(hPad, ref cache, linePressed, registersUsed);
        Assert.IsTrue(cache.IsRegistersCached(linePressed));
        Assert.IsTrue(rects.Length == 1);
        Assert.IsTrue(rects[0].Count == 2, "Did not find exactly both registers.");
    }

    /// <summary>
    /// This test should check whether line press information is cleared when it is necessary.
    /// It does not check whether it is unnecessarily cleared.
    /// </summary>
    [Test]
    public void ClearLinePressTest()
    {
        void SetupCache(float pad, int lineNr, ref LongTextArea.LineRegRectsCache cache, List<string> regsUsed)
        {
            _textArea._pressedLine = lineNr;
            _ = _textArea.GetRegisterRects(pad, ref cache, lineNr, regsUsed);
            _ = _textArea.GetLineHighlight(ref cache, pad, lineNr);
        }

        // Test setup:
        var registersUsed = new List<string> { "rbp", "rsp" };
        const float hPad = 20f;
        const int linePressed = 4 + 13;

        var disassembler = new BurstDisassembler();
        var thisPath = Path.GetDirectoryName(GetThisFilePath());
        Assert.NotNull(thisPath, "Could not retrieve path for current directory.");
        var textToRender = File.ReadAllText(Path.Combine(thisPath, _burstJobPath));

        // Set up dependencies for GetLineHighlight(.)
        _textArea.SetText("", textToRender, true, disassembler,
            disassembler.Initialize(
                textToRender,
                BurstDisassembler.AsmKind.Intel)
        );

        // Setting up variables to determine view size:
        _textArea.Layout(GUIStyle.none, hPad);
        _textArea.LayoutEnhanced(GUIStyle.none, Rect.zero, false);

        var blockIdx = _textArea.GetLinesBlockIdx(linePressed);

        _textArea.Layout(GUIStyle.none, hPad);
        _textArea.LayoutEnhanced(GUIStyle.none,
            new Rect(0,0, _textArea.fontWidth*100,_textArea.fontHeight*(_textArea.blockLine[blockIdx]+1)),
            false);


        void TestCache(bool isLineRect, bool isRect, bool isLine, string msg)
        {
            Assert.AreEqual(isLineRect,
                _textArea._lineRegCache.IsLineHighlightCached(linePressed, _textArea._folded[blockIdx]),
                msg + " Line highlight failed.");
            Assert.AreEqual(isRect,
                _textArea._lineRegCache.IsRegistersCached(linePressed),
                msg + " Register cache failed.");

            msg += " Line press failed.";
            if (!isLine)
            {
                Assert.AreEqual(-1, _textArea._pressedLine, msg);
            }
            else
            {
                Assert.AreNotEqual(-1, _textArea._pressedLine, msg);
            }

            SetupCache(hPad, linePressed, ref _textArea._lineRegCache, registersUsed);
        }


        SetupCache(hPad, linePressed, ref _textArea._lineRegCache, registersUsed);
        TestCache(true, true, true, "Initial setup failed.");

        // Following changes should result in clearing everything, as assembly text might have changed:
        //  * Expand all.
        _textArea.ExpandAllBlocks();
        TestCache(false, false, false, "Expanding blocks failed.");

        //  * Focus code.
        _textArea.FocusCodeBlocks();
        TestCache(false, false, false, "Focusing code blocks failed.");

        //  * disassembly kind, Target change, Safety check changes, Assembly kind changes e.g. by amount of debug info.
        _textArea.SetText("", textToRender, true, disassembler,
            disassembler.Initialize(
                textToRender,
                BurstDisassembler.AsmKind.Intel)
        );
        TestCache(false, false, false, "Setting up new text failed.");

        // Following changes should only result in Rec change clear, as line number still resembles same line:
        //  * Font size.
        _textArea.Invalidate();
        TestCache(false, false, true, "Changing font size failed.");

        //  * Show branch flow.
        _textArea.LayoutEnhanced(GUIStyle.none,
            new Rect(0,0, _textArea.fontWidth*100,_textArea.fontHeight*(_textArea.blockLine[blockIdx]+1)),
            true);
        TestCache(false, false, true, "Changing font size failed.");

        //  * Smell test (This will however clear everything as ´SetText()´ required).
        //    Hence tested in the cases for fill clear.
    }


    private static string GetThisFilePath([CallerFilePath] string path = null) => path;
    private readonly string _burstJobPath = "burstTestTarget.txt";
}