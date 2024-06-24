using System;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Burst.Editor;
using UnityEngine;
using Unity.Burst;
using Random = System.Random;

[TestFixture]
public class BurstDisassemblerCoreInstructionTests
{
    // Use chooser enum instead of BurstDisassembler.AsmKind because of accessibility level.
    public enum Chooser
    {
        ARM,
        INTEL,
        LLVMIR,
        Wasm
    }

    [Test]
    [TestCase(Chooser.ARM)]
    [TestCase(Chooser.INTEL)]
    // [TestCase(Chooser.LLVMIR)]
    [TestCase(Chooser.Wasm)]
    public void TestInfo(Chooser provider)
    {
        BurstDisassembler.AsmTokenKindProvider tokenProvider;
        switch (provider)
        {
            case Chooser.ARM:
                tokenProvider = BurstDisassembler.ARM64AsmTokenKindProvider.Instance;
                break;
            case Chooser.INTEL:
                tokenProvider = BurstDisassembler.X86AsmTokenKindProvider.Instance;
                break;
            case Chooser.Wasm:
                tokenProvider = BurstDisassembler.WasmAsmTokenKindProvider.Instance;
                break;
            default:
                throw new Exception("Oops you forgot to add a switch case in the test *quirky smiley*.");
        }


        var tokenProviderT = typeof(BurstDisassembler.AsmTokenKindProvider);

        var field = tokenProviderT.GetField("_tokenKinds",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(field, "Could not find _tokenKinds field in AsmTokenKindProvider");

        var allTokens = (Dictionary<StringSlice, BurstDisassembler.AsmTokenKind>)field.GetValue(tokenProvider);

        var tokensToTest =
            from tok in allTokens.Keys
            where allTokens.TryGetValue(tok, out var kind)
                  && kind != BurstDisassembler.AsmTokenKind.Qualifier
                  && kind != BurstDisassembler.AsmTokenKind.Register
            select tok.ToString();

        var count = 0;
        foreach (var token in tokensToTest)
        {
            var res = false;
            switch (provider)
            {
                case Chooser.ARM:
                    res = BurstDisassembler.ARM64InstructionInfo.GetARM64Info(token, out var _);
                    break;
                case Chooser.INTEL:
                    res = BurstDisassembler.X86AsmInstructionInfo.GetX86InstructionInfo(token, out var _);
                    break;
                case Chooser.LLVMIR:
                    res = BurstDisassembler.LLVMIRInstructionInfo.GetLLVMIRInfo(token, out var _);
                    break;
                case Chooser.Wasm:
                    res = BurstDisassembler.WasmInstructionInfo.GetWasmInfo(token, out var _);
                    break;
            }

            if (!res)
            {
                Debug.Log($"Token \"{token}\" from {provider} does not have information associated.");
                count++;
            }
        }
        Assert.Zero(count, $"{provider.ToString()} is missing information for {count} token(s).");
    }


    /// <summary>
    /// Tests whether all instructions in available burst jobs are displayed correctly.
    /// </summary>
    [Test]
    [TestCase(Chooser.ARM)]
    [TestCase(Chooser.INTEL)]
    [TestCase(Chooser.Wasm)]
    public void TestInstructionsPresent(Chooser asmKind)
    {
        BurstTargetCpu targetCpu;
        BurstDisassembler.AsmKind targetKind;
        switch (asmKind)
        {
            case Chooser.INTEL:
                targetCpu = BurstTargetCpu.X64_SSE4;
                targetKind = BurstDisassembler.AsmKind.Intel;
                break;
            case Chooser.ARM:
                targetCpu = BurstTargetCpu.ARMV7A_NEON32;
                targetKind = BurstDisassembler.AsmKind.ARM;
                break;
            default: // WASM as LLVMIR is not tested.
                targetCpu = BurstTargetCpu.WASM32;
                targetKind = BurstDisassembler.AsmKind.Wasm;
                break;
        }

        // Find all possible burst compile targets.
        var jobList = BurstReflection.FindExecuteMethods(
            BurstReflection.EditorAssembliesThatCanPossiblyContainJobs,
            BurstReflectionAssemblyOptions.None).CompileTargets;

        var missingInstructions = new Dictionary<string, string>();
        var disassembler = new BurstDisassembler();
        foreach (var target in jobList)
        {
            // Get disassembly of target.
            var options = new StringBuilder();

            target.Options.TryGetOptions(target.JobType, out var defaultOptions);
            options.AppendLine(defaultOptions);
            // Disables the 2 current warnings generated from code (since they clutter up the inspector display)
            // BC1370 - throw inside code not guarded with ConditionalSafetyCheck attribute
            // BC1322 - loop intrinsic on loop that has been optimised away
            options.AppendLine($"{BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionDisableWarnings, "BC1370;BC1322")}");
            options.AppendLine($"{BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionTarget, Enum.GetNames(typeof(BurstTargetCpu))[(int)targetCpu])}");
            options.AppendLine($"{BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionDebug, "0")}");

            var baseOptions = options.ToString();
            var append = BurstInspectorGUI.GetDisasmOptions()[(int)DisassemblyKind.Asm];

            // Setup disAssembler with the job:
            var text = BurstInspectorGUI.GetDisassembly(target.Method, baseOptions + append);

            // Bail out if there was a Burst compiler error, because we'll have all sorts of unexpected tokens.
            if (BurstInspectorGUI.IsBurstError(text))
            {
                continue;
            }

            text = text.TrimStart('\n');
            Assert.IsTrue(disassembler.Initialize(text, targetKind, true, false), "Could not initialize disassembler.");

            // Get all tokens labeled as AsmTokenKind.Identifier that do not start with '.' nor ends on ':'.
            // If this number exceeds 0 we are missing instructions (I believe).
            const int INSTRUCTION_PRE_PADDING = 8;
            var tokens =
                (from tok in disassembler.Tokens
                where tok.Kind == BurstDisassembler.AsmTokenKind.Identifier
                      && !disassembler.GetTokenAsText(tok).StartsWith(".")
                      && !disassembler.GetTokenAsText(tok).EndsWith(":")
                      && text[tok.Position - INSTRUCTION_PRE_PADDING - 1] == '\n'
                select tok).ToList();

            foreach (var token in tokens)
            {
                if (missingInstructions.ContainsKey(token.ToString(text)))
                {
                    continue;
                }
                missingInstructions.Add(token.ToString(text), target.GetDisplayName());
            }
        }
        // Convey result.
        if (missingInstructions.Count > 0)
        {
            foreach (var itm in missingInstructions)
            {
                var token = itm.Key;
                var name = itm.Value;
                Debug.Log($"Token \"{token}\" was not recognised as instruction for {targetKind} (Found in job {name}).");
            }
            Assert.Fail($"{missingInstructions.Count} missing instructions, see log. Add missing instructions and call both this test and {nameof(TestInfo)}.");
        }
    }
}
