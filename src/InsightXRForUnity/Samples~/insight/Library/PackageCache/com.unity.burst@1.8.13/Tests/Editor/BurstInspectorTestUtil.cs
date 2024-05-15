using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Unity.Burst;
using Unity.Burst.Editor;
using UnityEngine;

internal static class BurstInspectorTestUtil
{
    internal static BurstDisassembler GetDisassemblerAndText(
        string compileTargetName,
        int debugLvl,
        BurstTargetCpu targetCpu,
        out string textToRender)
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

        options.AppendLine($"{BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionTarget, targetCpu)}");

        options.AppendLine($"{BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionDebug, $"{debugLvl}")}");

        var baseOptions = options.ToString();

        var append = BurstInspectorGUI.GetDisasmOptions()[(int)DisassemblyKind.Asm];

        // Setup disAssembler with the job:
        compileTarget.RawDisassembly = BurstInspectorGUI.GetDisassembly(compileTarget.Method, baseOptions + append);
        textToRender = compileTarget.RawDisassembly.TrimStart('\n');

        return disassembler;
    }
}
