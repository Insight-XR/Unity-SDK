// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Abstract Inventory Class. Helpful so you can implement your own inventory system!
    /// </summary>
    public abstract class InventoryBehaviour : MonoBehaviour
    {
        #region GETTERS

        /// <summary>
        /// Returns the index that is before the current index. Very helpful in order to figure out
        /// what the next weapon to equip is.
        /// </summary>
        /// <returns></returns>
        public abstract int GetLastIndex();
        /// <summary>
        /// Returns the next index after the currently equipped one. Very helpful in order to figure out
        /// what the next weapon to equip is.
        /// </summary>
        public abstract int GetNextIndex();
        /// <summary>
        /// Returns the currently equipped WeaponBehaviour.
        /// </summary>
        public abstract WeaponBehaviour GetEquipped();

        /// <summary>
        /// Returns the currently equipped index. Meaning the index in the weapon array of the equipped weapon.
        /// </summary>
        public abstract int GetEquippedIndex();
        
        #endregion
        
        #region METHODS

        /// <summary>
        /// Init. This function is called when the game starts. We don't use Awake or Start because we need the
        /// PlayerCharacter component to run this with the index it wants to equip!
        /// </summary>
        /// <param name="equippedAtStart">Inventory index of the weapon we want to equip when the game starts.</param>
        public abstract void Init(int equippedAtStart = 0);
        
        /// <summary>
        /// Equips a Weapon.
        /// </summary>
        /// <param name="index">Index of the weapon to equip.</param>
        /// <returns>Weapon that was just equipped.</returns>
        public abstract WeaponBehaviour Equip(int index);

        #endregion
    }
}