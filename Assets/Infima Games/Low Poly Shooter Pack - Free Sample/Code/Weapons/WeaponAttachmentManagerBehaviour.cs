// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Weapon Attachment Manager Behaviour.
    /// </summary>
    public abstract class WeaponAttachmentManagerBehaviour : MonoBehaviour
    {
        #region UNITY FUNCTIONS

        /// <summary>
        /// Awake.
        /// </summary>
        protected virtual void Awake(){}

        /// <summary>
        /// Start.
        /// </summary>
        protected virtual void Start(){}

        /// <summary>
        /// Update.
        /// </summary>
        protected virtual void Update(){}

        /// <summary>
        /// Late Update.
        /// </summary>
        protected virtual void LateUpdate(){}

        #endregion
        
        #region GETTERS

        /// <summary>
        /// Returns the equipped scope.
        /// </summary>
        public abstract ScopeBehaviour GetEquippedScope();
        /// <summary>
        /// Returns the equipped scope default.
        /// </summary>
        public abstract ScopeBehaviour GetEquippedScopeDefault();
        
        /// <summary>
        /// Returns the equipped magazine.
        /// </summary>
        public abstract MagazineBehaviour GetEquippedMagazine();
        /// <summary>
        /// Returns the equipped muzzle.
        /// </summary>
        public abstract MuzzleBehaviour GetEquippedMuzzle();
        
        #endregion
    }
}