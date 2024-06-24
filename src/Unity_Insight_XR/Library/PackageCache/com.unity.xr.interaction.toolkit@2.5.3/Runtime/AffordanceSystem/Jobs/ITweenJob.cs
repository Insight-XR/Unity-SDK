using Unity.Jobs;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs
{
    /// <summary>
    /// Interface representing a tween job's basic functions.
    /// </summary>
    /// <typeparam name="T">Struct type of tween output.</typeparam>
    public interface ITweenJob<T> : IJob where T : struct
    {
        /// <summary>
        /// Typed job data used in tween job.
        /// </summary>
        TweenJobData<T> jobData { get; set; }

        /// <summary>
        /// Function used to interpolate between a tween's start value and target value.
        /// </summary>
        /// <param name="from">Tween start value.</param>
        /// <param name="to">Tween target value.</param>
        /// <param name="t">Value between 0-1 used to evaluate the output between the from and to values.</param>
        /// <returns>Returns the interpolation from <paramref name="from"/> to <paramref name="to"/>.</returns>
        T Lerp(T from, T to, float t);

        /// <summary>
        /// Function used to compare two values when evaluating a tween to determine if they're nearly equal in order
        /// to short-circuit the tween.
        /// </summary>
        /// <param name="from">First value in equality comparison.</param>
        /// <param name="to">Second value in equality comparison.</param>
        /// <returns>Returns <see langword="true"/> if values are nearly equal.</returns>
        bool IsNearlyEqual(T from, T to);
    }
}