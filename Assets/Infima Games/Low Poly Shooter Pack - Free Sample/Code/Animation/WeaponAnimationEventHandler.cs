// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Handles all the animation events that come from the weapon in the asset.
    /// </summary>
    public class WeaponAnimationEventHandler : MonoBehaviour
    {
        #region FIELDS

        /// <summary>
        /// Equipped Weapon.
        /// </summary>
        private WeaponBehaviour weapon;

        #endregion

        #region UNITY

        private void Awake()
        {
            //Cache. We use this one to call things on the weapon later.
            weapon = GetComponent<WeaponBehaviour>();
        }

        #endregion

        #region ANIMATION

        /// <summary>
        /// Ejects a casing from this weapon. This function is called from an Animation Event.
        /// </summary>
        private void OnEjectCasing()
        {
            //Notify.
            if(weapon != null)
                weapon.EjectCasing();
        }

        #endregion
    }
}