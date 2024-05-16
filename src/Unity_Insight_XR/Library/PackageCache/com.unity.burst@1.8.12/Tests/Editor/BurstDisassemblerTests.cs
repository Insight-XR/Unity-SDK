using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Burst;
using Unity.Burst.Editor;
using UnityEditorInternal;
using System.Runtime.CompilerServices;

public class BurstDisassemblerTests
{
    private BurstDisassembler _disassembler;

    [OneTimeSetUp]
    public void SetUp()
    {
        _disassembler = new BurstDisassembler();
    }

    private static string GetThisFilePath([CallerFilePath] string path = null) => path;

    // A Test behaves as an ordinary method
    [Test]
    public void GetBlockIdxFromTextIdxTest()
    {
        var thisPath = Path.GetDirectoryName(GetThisFilePath());

        Assert.IsTrue(_disassembler.Initialize(
            File.ReadAllText(Path.Combine(thisPath, "burstTestTarget.txt")),
            BurstDisassembler.AsmKind.Intel,
            false,
            false));

        for (int blockIdx = 0; blockIdx < _disassembler.Blocks.Count; blockIdx++)
        {
            int blockStart = 0;
            for (int i = 0; i < blockIdx; i++)
            {
                blockStart += _disassembler.GetOrRenderBlockToText(i).Length;
            }

            var blockStr = _disassembler.GetOrRenderBlockToText(blockIdx);

            Assert.AreEqual((blockIdx, blockStart),
                _disassembler.GetBlockIdxFromTextIdx(blockStart + 1),
                $"Block index was wrong for block with label {blockStr.Substring(0, blockStr.IndexOf('\n'))}");
        }
    }

    [Test]
    public void InstantiateRegistersUsedTest()
    {
        Assert.IsTrue(_disassembler.Initialize(simpleAssembly, BurstDisassembler.AsmKind.Intel));

        var regsUsed = _disassembler._registersUsedAtLine;

        // Match against expected:
        var expectedLines = from l in expected select l.lineNr;

        var failed = expectedLines.Except(regsUsed._linesRegisters.Keys);
        failed = failed.Concat(regsUsed._linesRegisters.Keys.Except(expectedLines)).Distinct();
        if (failed.Any())
        {
            // Not exact match
            foreach (var f in failed)
            {
                Debug.Log($"lineNumber {f} failed");
            }
            Assert.Fail();
        }
    }

    [Test]
    public void CleanRegisterListTest()
    {
        Assert.IsTrue(_disassembler.Initialize(simpleAssembly, BurstDisassembler.AsmKind.Intel));

        var regs = new List<string> { "rcx", "ecx", "rax" };
        var output = _disassembler._registersUsedAtLine.CleanRegs(regs);

        var expected = new List<string> { "rcx", "rax" };
        Assert.AreEqual(output, expected);
    }

    [Test]
    public void IndexOfRegisterTest()
    {
        var assembly =
            "\n" +
            "        nop\n" +
            "        movsxd rcx, cx\n" +
            "        mov rax, qword ptr [rbp - 16]";
        Assert.IsTrue(_disassembler.Initialize(assembly, BurstDisassembler.AsmKind.Intel));

        string[,] regs =
        {
            { "rcx", "cx" },
            { "rax", "rbp" }
        };
        string[] lines =
        {
            "        movsxd    rcx, cx\n",
            "        mov       rax, qword ptr [rbp - 16]"
        };
        for (var i = 0; i < 2; i++)
        {
            var line = lines[i];

            var reg = regs[i, 0];
            var asmLine = _disassembler.Lines[i+1];
            var output = _disassembler.GetRegisterTokenIndex(asmLine, reg);
            var regIdx = _disassembler.Tokens[output].AlignedPosition - _disassembler.Tokens[asmLine.TokenIndex].AlignedPosition;

            var expected = line.IndexOf(reg) + 1;
            Assert.AreEqual(expected, regIdx, $"Failed for line \"{line}\"");

            reg = regs[i, 1];
            output = _disassembler.GetRegisterTokenIndex(asmLine, reg, output + 1);
            regIdx = _disassembler.Tokens[output].AlignedPosition - _disassembler.Tokens[asmLine.TokenIndex].AlignedPosition;

            expected = line.IndexOf(reg, expected + 1) + 1;
            Assert.AreEqual(expected, regIdx, $"Failed for line \"{line}\"");
        }
    }

    [Test]
    [TestCase("x86", new [] {"rdx","edx","dx","dl"}, "dl")]
    [TestCase("arm", new [] {"wsp", "sp"},"sp")]
    [TestCase("arm", new [] {"v0.2d", "s0", "q0", "h0", "d0", "b0"}, "b0")]
    [TestCase("arm", new [] {"w0","x0"}, "x0")]
    public void RegisterEqualityTest(string assemblyName, string[] assemblyLine, string register)
    {
        BurstDisassembler.AsmTokenKindProvider tokenProvider = BurstDisassembler.ARM64AsmTokenKindProvider.Instance;
        if (assemblyName == "x86")
        {
            tokenProvider = BurstDisassembler.X86AsmTokenKindProvider.Instance;
        }

        foreach (var reg in assemblyLine)
        {
            Assert.IsTrue(tokenProvider.RegisterEqual(reg, register), $"{reg} == {register}");
        }

        // Some special cases:
        tokenProvider = BurstDisassembler.ARM64AsmTokenKindProvider.Instance;

        Assert.IsFalse(tokenProvider.RegisterEqual("w8", "x0"), $"w8 != x0");
        Assert.IsFalse(tokenProvider.RegisterEqual("w0", "q0"), "w0 != q0");
        Assert.IsFalse(tokenProvider.RegisterEqual("x0", "q0"), "x0 != q0");
    }

    [Test]
    public void RegisterEqualTest()
    {
        // Only tests for x86, as the others are trivial.
        Assert.IsTrue(_disassembler.Initialize(simpleAssembly, BurstDisassembler.AsmKind.Intel));

        // Get all register strings:
        var tokenProvider = BurstDisassembler.X86AsmTokenKindProvider.Instance;
        var tokenProviderT = typeof(BurstDisassembler.AsmTokenKindProvider);

        var field = tokenProviderT.GetField("_tokenKinds",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(field, "Could not find _tokenKinds field in AsmTokenKindProvider");

        var allTokens = (Dictionary<StringSlice, BurstDisassembler.AsmTokenKind>)field.GetValue(tokenProvider);

        var tokensToTest =
            from tok in allTokens.Keys
            where allTokens.TryGetValue(tok, out var kind)
                  && kind == BurstDisassembler.AsmTokenKind.Register
            select tok.ToString();

        // Test that equality works for all registers:
        try
        {
            foreach (var reg in tokensToTest)
            {
                // Simply check whether all registers are processable:
                tokenProvider.RegisterEqual(reg, "rax");
            }
        }
        catch (Exception e)
        {
            Assert.Fail($"Not all registers works for register equality (x86). {e}");
        }
    }

    [Test]
    public void InstructionAlignmentTest()
    {
        var assembly =
            "\n" + // newline as BurstDisassembler ignores first line
            "         push    rbp\n" +
            "         .seh_pushreg rbp\n" +
            "         sub    rsp, 32\n";
        (int, char)[] expectedPositions =
        {
            (1,' '), (10, 'p'), (14, ' '), (24, 'r'), (27, '\n'),
            (28, ' '), (37, '.'), (49, ' '), (50, 'r'), (53, '\n'),
            (54, ' '), (63, 's'),  (66, ' '), (77, 'r'), (80, ','), (82, '3'), (84, '\n')
        };

        Assert.IsTrue(_disassembler.Initialize(assembly, BurstDisassembler.AsmKind.Intel));

        var builder = new StringBuilder();
        for (int i = 0; i < _disassembler.Blocks.Count; i++)
        {
            var text = _disassembler.GetOrRenderBlockToTextUncached(i, false);
            builder.Append(text);
        }

        var output = builder.ToString();

        for (var i = 0; i < expectedPositions.Length; i++)
        {
            Assert.AreEqual(expectedPositions[i].Item1, _disassembler.Tokens[i].AlignedPosition);
        }

        foreach (var (idx, c) in expectedPositions)
        {
            // -1 as token index for some reason aren't zero indexed.
            Assert.AreEqual(c, output[idx-1], $"Token position for index {idx} was wrong.");
        }
    }

    [Test]
    public void X86AsmTokenProviderSimdKindTest()
    {
        var tp = BurstDisassembler.X86AsmTokenKindProvider.Instance;
        BurstDisassembler.SIMDkind actual = tp.SimdKind(new StringSlice("vsqrtsd"));
        var expected = BurstDisassembler.SIMDkind.Scalar;

        Assert.AreEqual(expected, actual);

        actual = tp.SimdKind(new StringSlice("vroundpd"));
        expected = BurstDisassembler.SIMDkind.Packed;
        Assert.AreEqual(expected, actual);

        actual = tp.SimdKind(new StringSlice("xsaves"));
        expected = BurstDisassembler.SIMDkind.Infrastructure;
        Assert.AreEqual(expected,actual);
    }

    [Test]
    public void ARMAsmTokenProviderSimdKindTest()
    {
        var tp = BurstDisassembler.ARM64AsmTokenKindProvider.Instance;

        BurstDisassembler.SIMDkind actual = tp.SimdKind(new StringSlice("vaddw"));
        var expected = BurstDisassembler.SIMDkind.Scalar;
        Assert.AreEqual(expected, actual);

        actual = tp.SimdKind(new StringSlice("vadd.i8"));
        expected = BurstDisassembler.SIMDkind.Packed;
        Assert.AreEqual(expected, actual);
    }

    private string GetFirstColorTag(string line)
    {
        const string colorTag = "#XXXXXX";
        const string tag = "<color=";
        int idx = line.IndexOf('<');
        return line.Substring(idx + tag.Length, colorTag.Length);
    }

    private const string ARMsimdAssembly =
        "\n" +
        "        ldr        r0, [sp, #12]\n" +
        "        vldr        s0, [sp, #20]\n" +
        "        vstr        s0, [sp, #4]\n" +
        "        ldr        r1, [sp, #24]\n" +
        "        vldr        s0, [sp, #4]\n" +
        "        vmov        s2, r0\n" +
        "        vadd.f32        s0, s0, s2\n" +
        "        vstr        s0, [sp, #20]";

    private const string X86SimdAssembly =
        "\n" +
        "        mov               rcx, qword ptr [rbp - 32]\n" +
        "        vmovss            xmm0, dword ptr [rbp - 12]\n" +
        "        vmovss            dword ptr [rbp - 40], xmm0\n" +
        "        mov               edx, dword ptr [rbp - 8]\n" +
        "        call              \"Unity.Collections.NativeArray`1<float>.get_Item(Unity.Collections.NativeArray`1<float>* this, int index) -> float_c303f72c9cc472e2ef84a442ead69ef2 from Unity.Burst.Editor.Tests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\"\n" +
        "        vmovaps           xmm1, xmm0\n" +
        "        vmovss            xmm0, dword ptr [rbp - 40]\n" +
        "        vaddss            xmm0, xmm0, xmm1\n" +
        "        vmovss            dword ptr [rbp - 12], xmm0\n" +
        "        vzeroall";
    [Test]
    [TestCase(X86SimdAssembly, 0, BurstDisassembler.DarkColorInstructionSIMDScalar, 1)]
    [TestCase(X86SimdAssembly, 0, BurstDisassembler.DarkColorInstructionSIMDPacked, 5)]
    [TestCase(X86SimdAssembly, 0, BurstDisassembler.DarkColorInstructionSIMD, 9)]
    [TestCase(ARMsimdAssembly, 1, BurstDisassembler.DarkColorInstructionSIMDScalar, 1)]
    [TestCase(ARMsimdAssembly, 1, BurstDisassembler.DarkColorInstructionSIMDPacked, 6)]
    public void AssemblyColouringSmellTest(string asm, int asmkind, string colorTag, int lineIdx)
    {
        _disassembler.Initialize(asm, (BurstDisassembler.AsmKind)asmkind, true, true, true);
        var line = _disassembler.Lines[lineIdx];
        _disassembler._output.Clear();
        _disassembler.RenderLine(ref line, true);
        var lineString = _disassembler._output.ToString();

        _disassembler._output.Clear();
        Assert.AreEqual(colorTag, GetFirstColorTag(lineString));
    }

    private List<(int lineNr, List<string>)> expected = new List<(int lineNr, List<string>)>
    {
        (2, new List<string> { "rbp" }),
        (3, new List<string> { "rbp" }),
        (4, new List<string> { "rsp" }),
        (6, new List<string> { "rbp", "rsp" }),
        (7, new List<string> { "rbp" }),
        (11, new List<string> { "rsp" }),
        (12, new List<string> { "rbp" }),
        (26, new List<string> { "rbp" }),
        (27, new List<string> { "rbp" }),
        (28, new List<string> { "rsp" }),
        (30, new List<string> { "rbp", "rsp" }),
        (31, new List<string> { "rbp" }),
        (36, new List<string> { "rsp" }),
        (37, new List<string> { "rbp" }),
    };
    private string simpleAssembly =
        "\n" + // newline as BurstDisassembler ignores first line
        ".Lfunc_begin0:\n" +
        ".seh_proc 589a9d678dbb1201e550a054238fad11\n" +
        "         push              rbp\n" +
        "         .seh_pushreg rbp\n" +
        "         sub               rsp, 32\n" +
        "         .seh_stackalloc 32\n" +
        "         lea               rbp, [rsp + 32]\n" +
        "         .seh_setframe rbp, 32\n" +
        "         .seh_endprologue\n" +
        "         call              A.B.DoIt\n" +
        "         nop\n" +
        "         add               rsp, 32\n" +
        "         pop               rbp\n" +
        "         ret\n" +
        " .Lfunc_end0:\n" +
        "         .seh_endproc\n" +
        " \n" +
        "         .def        burst.initialize;\n" +
        "         .scl        2;\n" +
        "         .type        32;\n" +
        "         .endef\n" +
        "         .globl        burst.initialize\n" +
        "         .p2align        4, 0x90\n" +
        " burst.initialize:\n" +
        " .Lfunc_begin1:\n" +
        " .seh_proc burst.initialize\n" +
        "         push              rbp\n" +
        "         .seh_pushreg rbp\n" +
        "         sub               rsp, 32\n" +
        "         .seh_stackalloc 32\n" +
        "         lea               rbp, [rsp + 32]\n" +
        "         .seh_setframe rbp, 32\n" +
        "         .seh_endprologue\n" +
        "         call              burst.initialize.externals\n" +
        "         call              burst.initialize.statics\n" +
        "         nop\n" +
        "         add               rsp, 32\n" +
        "         pop               rbp\n" +
        "         ret\n" +
        " .Lfunc_end1:\n" +
        "         .seh_endproc\n" +
        " \n" +
        "         .def        burst.initialize.externals;\n" +
        "         .scl        2;\n" +
        "         .type        32;\n" +
        "         .endef\n" +
        "         .globl        burst.initialize.externals\n" +
        "         .p2align        4, 0x90";
}