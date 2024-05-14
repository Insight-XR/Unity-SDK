// Copyright 2021, Infima Games. All Rights Reserved.

using TMPro;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Displays a material's name in the world.
    /// </summary>
    public class DisplayMaterialName : MonoBehaviour
    {
        #region FIELDS SERIALIZED

        [Header("Settings")]
        
        [Tooltip("Mesh.")]
        [SerializeField]
        private Renderer mesh;

        [Tooltip("Text.")]
        [SerializeField]
        private TextMeshProUGUI materialText;

        #endregion

        #region FIELDS

        /// <summary>
        /// Material.
        /// </summary>
        private Material meshMaterial;

        #endregion

        #region UNITY

        private void Start()
        {
            //Get current material name from the mesh.
            string sharedMaterialName = mesh.sharedMaterial.name;
            //Output current material name to the UI text.
            materialText.text = sharedMaterialName;
        }

        #endregion
    }
}