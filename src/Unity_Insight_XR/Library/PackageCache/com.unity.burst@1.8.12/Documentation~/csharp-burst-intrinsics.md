# Burst intrinsics overview

Burst provides low level intrinsics in the [`Unity.Burst.Intrinsics`](xref:Unity.Burst.Intrinsics) namespace. This is useful if you know how to write single instruction, multiple data (SIMD) assembly code, and you want to get extra performance from Burst code. For most use cases, you won't need to use these.

This section contains the following information

|**Page**|**Description**|
|---|---|
|[Burst intrinsics Common class](csharp-burst-intrinsics-common.md)|Overview of the `Burst.Intrinsics.Common` class, which provides functionality shared across the hardware targets that Burst supports. |
|[DllImport and internal calls](csharp-burst-intrinsics-dllimport.md)|Overview of `[DllImport]`, which is for calling native functions.|
|[Processor specific SIMD extensions](csharp-burst-intrinsics-processors.md)|Overview of the Intel and Arm Neon intrinsics.|
|[Arm Neon intrinsics reference](csharp-burst-intrinsics-neon.md)|Reference of the methods in the `Burst.Intrinsics.Arm.Neon` class.|
