namespace OverloadedFunctionPointers
{
#if UNITY_2021_2_OR_NEWER && UNITY_EDITOR
    public unsafe struct Callable
    {
        public int Value;

        private Callable(int x)
        {
            Value = x;
        }

        public static Callable Create<T1, T2>(delegate* unmanaged[Cdecl] < T1, T2, void > function) => new Callable(2);
        public static Callable Create<T1, TRet>(delegate* unmanaged[Cdecl] < T1, TRet > function) => new Callable(3);
    }
#endif
}
