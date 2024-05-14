// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Array Utilities.
    /// </summary>
    public static class UtilitiesArrays
    {
        /// <summary>
        /// Returns true if the array contains this index.
        /// </summary>
        public static bool IsValidIndex<T>(this T[] array, int index) => array.Length > index && index >= 0;
        /// <summary>
        /// Returns true if the array is valid.
        /// </summary>
        public static bool IsValid<T>(this T[] array) => !array.Equals(null) && array.Length > 0;
        /// <summary>
        /// Returns a random audio clip from an array of clips.
        /// </summary>
        public static T GetRandom<T>(this T[] array) => array[Random.Range(0, array.Length)];
    }
}