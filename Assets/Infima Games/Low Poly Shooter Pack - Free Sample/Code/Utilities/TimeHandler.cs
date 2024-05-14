// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;
using UnityEngine.InputSystem;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Time Manager.
    /// </summary>
    public class TimeHandler : MonoBehaviour
    {
        [Header("Settings")]
        
        [Tooltip("Value the time scale gets updated by every time.")]
        [SerializeField]
        private float increment = 0.1f;
        
        /// <summary>
        /// Determines if the time is stopped.
        /// </summary>
        private bool paused;
        
        /// <summary>
        /// Current Time Scale.
        /// </summary>
        private float current = 1.0f;

        /// <summary>
        /// Updates The Time Scale.
        /// </summary>
        private void Scale()
        {
            //Update Time Scale.
            Time.timeScale = current;
        }
        
        /// <summary>
        /// Change Time Scale.
        /// </summary>
        private void Change(float value = 1.0f)
        {
            //Save Value.
            current = value;
            
            //Update.
            Scale();
        }

        /// <summary>
        /// Increase Time Scale Value.
        /// </summary>
        private void Increase(float value = 1.0f)
        {
            //Change.
            Change(Mathf.Clamp01(current + value));
        }

        /// <summary>
        /// Pause.
        /// </summary>
        private void Pause()
        {
            //Pause.
            paused = true;
            
            //Pause.
            Time.timeScale = 0.0f;
        }
        
        /// <summary>
        /// Toggle Pause.
        /// </summary>
        private void Toggle()
        {
            //Toggle Pause.
            if (paused)
                Unpause();
            else
                Pause();
        }

        /// <summary>
        /// Unpause.
        /// </summary>
        private void Unpause()
        {
            //Unpause.
            paused = false;
            
            //Unpause.
            Change(current);
        }

        /// <summary>
        /// Increase Time Scale Event.
        /// </summary>
        public virtual void OnIncrease(InputAction.CallbackContext context)
        {
            //Switch.
            switch (context.phase)
            {
                //Performed.
                case InputActionPhase.Performed:
                    //Increase.
                    Increase(increment);
                    break;
            }
        }
        
        /// <summary>
        /// Increase Time Scale Event.
        /// </summary>
        public virtual void OnDecrease(InputAction.CallbackContext context)
        {
            //Switch.
            switch (context.phase)
            {
                //Performed.
                case InputActionPhase.Performed:
                    //Increase.
                    Increase(-increment);
                    break;
            }
        }

        /// <summary>
        /// Toggle Time Scale Stop.
        /// </summary>
        public virtual void OnToggle(InputAction.CallbackContext context)
        {
            //Switch.
            switch (context.phase)
            {
                //Performed.
                case InputActionPhase.Performed:
                    //Toggle.
                    Toggle();
                    break;
            }      
        }
    }
}