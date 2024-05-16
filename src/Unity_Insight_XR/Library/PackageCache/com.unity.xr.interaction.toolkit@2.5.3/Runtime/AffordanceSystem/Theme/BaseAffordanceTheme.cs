using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Datums;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme
{
    /// <summary>
    /// Affordance state theme data structure for blended affordances.
    /// Represents the initial and target value that will be animated towards for an affordance state
    /// during interactions.
    /// </summary>
    /// <typeparam name="T">Serialized type used in affordance blending.</typeparam>
    [Serializable]
    public sealed class AffordanceThemeData<T>
    {
        /// <summary>
        /// Name of the affordance state the theme data is for.
        /// This value is optional and does not serve a functional purpose.
        /// </summary>
        [Tooltip("Name of the affordance state the theme data is for." +
                 "\nThis value is optional and does not serve a functional purpose.")]
        public string stateName;

        /// <summary>
        /// Target value for the curve at 0.
        /// </summary>
        [Tooltip("Target value for the curve at 0")]
        public T animationStateStartValue;

        /// <summary>
        /// Target value for the curve at 1.
        /// </summary>
        [Tooltip("Target value for the curve at 1.")]
        public T animationStateEndValue;
    }

    /// <summary>
    /// Base abstract class that holds a list of type <see cref="AffordanceThemeData{T}"/> matching affordance state indices to state data.
    /// </summary>
    /// <typeparam name="T">Serialized type used in affordance blending.</typeparam>
    [Serializable]
    public abstract class BaseAffordanceTheme<T> : IEquatable<BaseAffordanceTheme<T>> where T : struct
    {
        [SerializeField]
        [Tooltip("Curve used to evaluate the target value of the animation state according to the affordance state's transition amount value.")]
        AnimationCurveDatumProperty m_StateAnimationCurve = new AnimationCurveDatumProperty(AnimationCurve.EaseInOut(0, 0f, 1f, 1f));

        [SerializeField]
        [Tooltip("List of affordance states supported by this theme. The entry index is how states are mapped to their theme data." +
                 "\nDo not re-order entries.")]
        List<AffordanceThemeData<T>> m_List;

        /// <summary>
        /// Animation curve used to evaluate the affordance state's transition amount value.
        /// </summary>
        public AnimationCurve animationCurve => m_StateAnimationCurve.Value;

        /// <summary>
        /// Initializes and returns an instance of <see cref="BaseAffordanceTheme{T}"/>.
        /// </summary>
        protected BaseAffordanceTheme()
        {
            // Create initial list with states and names as initial order
            m_List = new List<AffordanceThemeData<T>>
            {
                new AffordanceThemeData<T> { stateName = nameof(AffordanceStateShortcuts.disabled) },
                new AffordanceThemeData<T> { stateName = nameof(AffordanceStateShortcuts.idle) },
                new AffordanceThemeData<T> { stateName = nameof(AffordanceStateShortcuts.hovered) },
                new AffordanceThemeData<T> { stateName = nameof(AffordanceStateShortcuts.hoveredPriority) },
                new AffordanceThemeData<T> { stateName = nameof(AffordanceStateShortcuts.selected) },
                new AffordanceThemeData<T> { stateName = nameof(AffordanceStateShortcuts.activated) },
                new AffordanceThemeData<T> { stateName = nameof(AffordanceStateShortcuts.focused) },
            };
        }

        /// <summary>
        /// This method is used to validate the theme of the affordance state.
        /// It checks if the number of entries in the list is correct and adds missing states using the idle state data, if necessary.
        /// </summary>
        /// <remarks>
        /// The method first checks if the <c>m_List</c> object is <see langword="null"/>, and if so, it returns immediately. 
        /// If the <c>m_List</c> object is not <see langword="null"/>, it calculates the discrepancy between the built-in state count and the count of list items.
        /// If this discrepancy is greater than zero, meaning there are missing entries, it then creates a new state copy from the existing idle state data in the list. 
        /// It uses this idle state copy to fill the missing entries. 
        /// Lastly, it updates the state names for the built-in state elements of the list, in order, with the appropriate state names.
        /// </remarks>
        internal void ValidateTheme()
        {
            if (m_List == null)
                return;

            var listCount = m_List.Count;
            var listDiscrepancy =  AffordanceStateShortcuts.stateCount - listCount;
            
            // Validate that the list has the correct number of entries
            if (listDiscrepancy > 0)
            {
                AffordanceThemeData<T> idleState;
                if (listCount < AffordanceStateShortcuts.idle + 1)
                    idleState = new AffordanceThemeData<T> { stateName = nameof(AffordanceStateShortcuts.idle) };
                else
                    idleState = m_List[AffordanceStateShortcuts.idle];
                
                // Add missing states with the data found in the idle state
                while (listDiscrepancy-- > 0)
                {
                    // Add state
                    var idleStateCopy = new AffordanceThemeData<T>
                    {
                        stateName = idleState.stateName,
                        animationStateStartValue = idleState.animationStateStartValue,
                        animationStateEndValue = idleState.animationStateEndValue,
                    };
                    m_List.Add(idleStateCopy);
                    
                    // Update state name
                    var currentIndex = (byte)(m_List.Count - 1);
                    var stateName = AffordanceStateShortcuts.GetNameForIndex(currentIndex);
                    m_List[currentIndex].stateName = stateName;
                    
                    Debug.LogWarning($"Found missing state {currentIndex} \"{stateName}\" in your affordance theme. Adding missing state with idle state data.");
                }
            }
        }

        /// <summary>
        /// Gets the affordance theme data for the affordance state.
        /// </summary>
        /// <param name="stateIndex">The affordance state index to get the theme data for.</param>
        /// <returns>Returns the affordance theme data for the affordance state,
        /// or <see langword="null"/> if there is no theme data associated with the state.</returns>
        public AffordanceThemeData<T> GetAffordanceThemeDataForIndex(byte stateIndex)
        {
            return stateIndex < m_List.Count ? m_List[stateIndex] : null;
        }

        /// <summary>
        /// Update internal affordance theme data list by copying elements from the provided list.
        /// </summary>
        /// <param name="newList">List to replace old list with.</param>
        /// <remarks>
        /// This method populates the theme data list from the elements in the provided list at the time the
        /// method is called. It is not a live view, meaning elements added or removed afterward will not be
        /// reflected in the list managed by this class.
        /// </remarks>
        public void SetAffordanceThemeDataList(List<AffordanceThemeData<T>> newList)
        {
            m_List.Clear();
            m_List.AddRange(newList);
        }

        /// <summary>
        /// Makes this theme's settings match the settings of another theme.
        /// </summary>
        /// <param name="other">The theme to deep copy values from. It will not be modified.</param>
        public virtual void CopyFrom(BaseAffordanceTheme<T> other)
        {
            m_List = new List<AffordanceThemeData<T>>(other.m_List);
            m_StateAnimationCurve = other.m_StateAnimationCurve;
        }

        /// <summary>
        /// Update internal animation curve reference.
        /// </summary>
        /// <param name="newAnimationCurve">Animation curve to replace theme value with.</param>
        public void SetAnimationCurve(AnimationCurve newAnimationCurve)
        {
            m_StateAnimationCurve.Value = newAnimationCurve;
        }

        // IEquatable API
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>Returns <see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
        public bool Equals(BaseAffordanceTheme<T> other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(m_StateAnimationCurve, other.m_StateAnimationCurve) &&
                   Equals(m_List, other.m_List);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((BaseAffordanceTheme<T>)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>Returns a 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            // NOTE HashCode.Combine was used before, but it is not available in older versions of dotNet
            var hash = 17;
            hash = hash * 31 + m_StateAnimationCurve.GetHashCode();
            hash = hash * 31 + m_List.GetHashCode();
            return hash;
        }
        // End IEquatable API
    }
}