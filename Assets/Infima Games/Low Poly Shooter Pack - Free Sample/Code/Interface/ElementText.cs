// Copyright 2021, Infima Games. All Rights Reserved.

using TMPro;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack.Interface
{
    /// <summary>
    /// Text Interface Element.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public abstract class ElementText : Element
    {
        #region FIELDS

        /// <summary>
        /// Text Mesh.
        /// </summary>
        protected TextMeshProUGUI textMesh;

        #endregion

        #region UNITY

        protected override void Awake()
        {
            //Base.
            base.Awake();

            //Get Text Mesh.
            textMesh = GetComponent<TextMeshProUGUI>();
        }

        #endregion
    }
}