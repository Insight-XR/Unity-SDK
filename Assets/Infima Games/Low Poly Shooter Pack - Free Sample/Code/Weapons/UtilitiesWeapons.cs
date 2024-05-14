// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Weapon Static Utilities.
    /// </summary>
    public static class UtilitiesWeapons
    {
        /// <summary>
        /// Enables one object, disables all others.
        /// </summary>
        public static T SelectAndSetActive<T>(this T[] array, int index) where T : MonoBehaviour
        {
            //Make sure we have objects in the array! If we don't, we could get an error or a crash here.
            if (!array.IsValid()) 
                return null;
            
            //Deactivate All. This way we don't have to do it manually.
            array.ForEach(obj => obj.gameObject.SetActive(false));

            //Error Check.
            if (!array.IsValidIndex(index)) 
                return null;
                
            //Activate.
            T behaviour = array[index];
            if(behaviour != null)
                behaviour.gameObject.SetActive(true);

            //Return.
            return behaviour;
        }
    }
}