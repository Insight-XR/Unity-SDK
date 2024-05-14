using TMPro;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack.Interface
{
    /// <summary>
    /// Interface component that hides or shows the tutorial text based on input.
    /// </summary>
    public class TextTutorial : ElementText
    {
        #region FIELDS SERIALIZED
        
        [Header("References")]
        
        [Tooltip("Tutorial prompt text.")]
        [SerializeField]
        private TextMeshProUGUI prompt;

        [Tooltip("Tutorial text.")]
        [SerializeField]
        private TextMeshProUGUI tutorial;

        #endregion

        #region UNITY

        protected override void Awake()
        {
            //Base.
            base.Awake();

            //Enable the prompt by default.
            prompt.enabled = true;
            //Disable the tutorial by default.
            tutorial.enabled = false;
        }

        #endregion

        #region METHODS

        protected override void Tick()
        {
            //Get whether we should be showing the tutorial text, or not.
            bool isVisible = playerCharacter.IsTutorialTextVisible();
            //Hide the prompt if the tutorial is visible.
            prompt.enabled = !isVisible;
            //Show the tutorial if needed.
            tutorial.enabled = isVisible;
        }

        #endregion
    }
}