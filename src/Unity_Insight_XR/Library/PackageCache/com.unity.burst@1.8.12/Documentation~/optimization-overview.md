# Optimization

Best practices around optimizing Burst-compiled code.

|**Topic**|**Description**|
|---|---|
|[Debugging and profiling tools](debugging-profiling-tools.md)|Debug and profile your Burst-compiled code in the Editor and in player builds.|
|[Loop vectorization optimization](optimization-loop-vectorization.md)| Understand how Burst uses loop vectorization to optimize your code.|
|[Memory aliasing](aliasing.md)| Use memory aliasing to tell Burst how your code uses data.|
|[AssumeRange attribute](optimization-assumerange.md)| Use AssumeRange to tell Burst a given scalar-integer lies within a certain constrained range.|
|[Hint intrinsic](optimization-hint.md)| Use the Hint intrinsic to give Burst more information about your data.|
|[Constant intrinsic](optimization-constant.md)| Use IsConstantExpression top check if an expression is constant at run time.|
|[SkipLocalsInit attribute](optimization-skiplocalsinit.md)|Use SkipLocalsInitAttribute to tell Burst that any stack allocations within a method don't have to be initialized to zero.|

## Additional resources

* [Burst intrinsics](csharp-burst-intrinsics.md)