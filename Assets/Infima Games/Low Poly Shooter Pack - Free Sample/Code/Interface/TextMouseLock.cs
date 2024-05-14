// Copyright 2021, Infima Games. All Rights Reserved.

namespace InfimaGames.LowPolyShooterPack.Interface
{
    /// <summary>
    /// This component handles warning developers whether their mouse is locked or not by
    /// updating a text in the interface.
    /// </summary>
    public class TextMouseLock : ElementText
    {
        #region METHODS

        protected override void Tick()
        {
            //Update the text based on whether the cursor is locked or not.
            textMesh.text = "Cursor " + (playerCharacter.IsCursorLocked() ? "Locked" : "Unlocked");
        }

        #endregion
    }
}