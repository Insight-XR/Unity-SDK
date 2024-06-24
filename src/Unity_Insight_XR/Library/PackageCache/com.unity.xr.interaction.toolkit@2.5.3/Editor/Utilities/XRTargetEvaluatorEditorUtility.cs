using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

namespace UnityEditor.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Utility class to edit the evaluator list of a Target Filter.
    /// </summary>
    /// <remarks>
    /// This class will directly call the methods <see cref="XRTargetFilter.AddEvaluator"/> and <see cref="XRTargetFilter.RemoveEvaluatorAt"/>
    /// in the filter to properly trigger the evaluator callbacks when needed.
    /// </remarks>
    /// <seealso cref="XRTargetEvaluator"/>
    static class XRTargetEvaluatorEditorUtility
    {
        /// <summary>
        /// List of valid evaluator instance types.
        /// </summary>
        static List<Type> s_EvaluatorInstancesTypes;

        static bool IsPublicEvaluatorInstanceType(Type type)
        {
            return XRTargetEvaluator.IsInstanceType(type) && (type.IsPublic || type.IsNestedPublic);
        }

        /// <summary>
        /// (Read Only) Gets a list populated with valid evaluator instance types.
        /// Use these types as parameter for <see cref="AddEvaluator"/>.
        /// </summary>
        /// <returns>Returns a list with valid evaluator instance types.</returns>
        public static List<Type> GetEvaluatorInstanceTypes()
        {
            if (s_EvaluatorInstancesTypes == null)
            {
                s_EvaluatorInstancesTypes = new List<Type>();
                typeof(XRTargetEvaluator).GetAssignableTypes(s_EvaluatorInstancesTypes, IsPublicEvaluatorInstanceType);
                s_EvaluatorInstancesTypes.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            }

            return s_EvaluatorInstancesTypes;
        }

        /// <summary>
        /// Map of full type names to their type name (used to cache evaluator type names).
        /// </summary>
        static readonly Dictionary<string, string> s_TypeNameMap = new Dictionary<string, string>();

        /// <summary>
        /// Gets the type name of the given full type name.
        /// Useful to get an evaluator type name from its serialized property.
        /// </summary>
        /// <param name="fullTypeName">The full type name of the class.</param>
        /// <returns>Returns the type name. The returned value is cached for reuse.</returns>
        public static string GetTypeName(string fullTypeName)
        {
            if (string.IsNullOrEmpty(fullTypeName))
                return string.Empty;

            if (s_TypeNameMap.TryGetValue(fullTypeName, out var typeName))
                return typeName;

            var namespaceName = fullTypeName.Substring(fullTypeName.LastIndexOf(' ') + 1);
            typeName = namespaceName.Substring(namespaceName.LastIndexOf('.') + 1);
            s_TypeNameMap.Add(fullTypeName, typeName);

            return typeName;
        }

        /// <summary>
        /// Checks if the given behaviour is a valid instance at runtime.
        /// This is achieved by checking if the given behaviour is not in any prefab nor is it an object in Prefab Mode.
        /// Useful to check if a Target Filter is a valid runtime instance and then directly call methods on it to invoke evaluator callbacks.
        /// </summary>
        /// <param name="behaviour">The behaviour to check.</param>
        /// <returns>
        /// Returns <see langword="true"/> when the Unity editor is in play mode and the given behaviour is a valid instance.
        /// Otherwise, returns <see langword="false"/>.
        /// </returns>
        public static bool IsRuntimeInstance(Behaviour behaviour)
        {
            if (!Application.isPlaying || PrefabUtility.IsPartOfAnyPrefab(behaviour))
                return false;

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            return prefabStage == null || !prefabStage.IsPartOfPrefabContents(behaviour.gameObject);
        }

        /// <summary>
        /// Adds an instance of the given evaluator type to the the given evaluator list property.
        /// </summary>
        /// <param name="evaluatorListProperty">The evaluator list property of the Target Filter.</param>
        /// <param name="evaluatorType">Type of the evaluator to be added.</param>
        /// <returns>Returns <see langword="true"/> when the evaluator is successfully added. Otherwise, returns <see langword="false"/>.</returns>
        /// <remarks>
        /// During play mode if the Target Filter is a valid runtime instance this method will not register Undo or Redo
        /// and directly call <see cref="XRTargetFilter.AddEvaluator"/>.
        /// </remarks>
        public static bool AddEvaluator(SerializedProperty evaluatorListProperty, Type evaluatorType)
        {
            var serializedObject = evaluatorListProperty.serializedObject;
            var filter = serializedObject.targetObject as XRTargetFilter;
            if (filter == null)
            {
                Debug.LogError($"Couldn't retrieve a valid evaluator for the list {evaluatorListProperty.propertyPath}.");
                return false;
            }

            // If the filter is a runtime instance we call AddEvaluator on it
            if (IsRuntimeInstance(filter))
                return filter.AddEvaluator(evaluatorType) != null;

            var newEvaluator = XRTargetEvaluator.CreateInstance(evaluatorType, filter);
            if (newEvaluator == null)
            {
                Debug.LogError($"Couldn't create a valid evaluator instance of type {evaluatorType} in the filter {filter}.", filter);
                return false;
            }

            evaluatorListProperty.arraySize++;
            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();
            var newElement = evaluatorListProperty.GetArrayElementAtIndex(evaluatorListProperty.arraySize - 1);
            newElement.managedReferenceValue = newEvaluator;
            serializedObject.ApplyModifiedProperties();

            // Here is where we call Reset (in the editor) on the new added evaluator
            serializedObject.Update();
            newEvaluator.Reset();
            serializedObject.ApplyModifiedProperties();

            return true;
        }

        /// <summary>
        /// Removes the evaluator in the given index from the evaluator list in the given serialized property.
        /// </summary>
        /// <param name="evaluatorListProperty">The evaluator list property of the Target Filter.</param>
        /// <param name="evaluatorIndex">Index of the evaluator to be removed.</param>
        /// <returns>Returns <see langword="true"/> when the evaluator is successfully removed. Otherwise, returns <see langword="false"/>.</returns>
        /// <remarks>
        /// During play mode if the Target Filter is a valid runtime instance this method will not register Undo or Redo
        /// and directly call <see cref="XRTargetFilter.RemoveEvaluatorAt"/>.
        /// </remarks>
        public static bool RemoveEvaluatorAt(SerializedProperty evaluatorListProperty, int evaluatorIndex)
        {
            var element = evaluatorListProperty.GetArrayElementAtIndex(evaluatorIndex);
            if (element == null || element.propertyType != SerializedPropertyType.ManagedReference)
            {
                Debug.LogError($"Couldn't retrieve a valid element serialized property at index {evaluatorIndex.ToString()} in the evaluator list {evaluatorListProperty.propertyPath}.");
                return false;
            }

            var serializedObject = evaluatorListProperty.serializedObject;
            var filter = serializedObject.targetObject as XRTargetFilter;
            if (filter == null)
            {
                Debug.LogError($"Couldn't retrieve a valid evaluator for the list {evaluatorListProperty.propertyPath}.");
                return false;
            }

            if (IsRuntimeInstance(filter))
            {
                // If the filter is a runtime instance we call RemoveEvaluatorAt on it
                filter.RemoveEvaluatorAt(evaluatorIndex);
            }
            else
            {
                evaluatorListProperty.DeleteArrayElementAtIndex(evaluatorIndex);
                serializedObject.ApplyModifiedProperties();
            }

            return true;
        }
    }
}
