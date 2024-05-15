using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Custom editor for an <see cref="XRInteractionGroup"/>.
    /// </summary>
    [CustomEditor(typeof(XRInteractionGroup), true)]
    public class XRInteractionGroupEditor : BaseInteractionEditor
    {
        const string k_InteractionOverrideConfigExpandedKey = "XRI." + nameof(XRInteractionGroupEditor) + ".InteractionOverrideConfigurationExpanded";
        const string k_MemberAndOverridesPairGroupMemberPropertyName = "groupMember";
        const string k_MemberAndOverridesPairOverrideGroupMembersPropertyName = "overrideGroupMembers";
        static readonly GUIContent k_CanOverrideLabelContent = new GUIContent("Can Override");

        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractionGroup.interactionManager"/>.</summary>
        protected SerializedProperty m_InteractionManager;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractionGroup.startingGroupMembers"/>.</summary>
        protected SerializedProperty m_StartingGroupMembers;
        /// <summary>
        /// <see cref="SerializedProperty"/> of the <see cref="SerializeField"/> updated by
        /// <see cref="XRInteractionGroup.AddStartingInteractionOverride"/> and <see cref="XRInteractionGroup.RemoveStartingInteractionOverride"/>.
        /// </summary>
        protected SerializedProperty m_StartingInteractionOverridesMap;

        XRInteractionGroup m_TargetInteractionGroup;

        GUIContent m_InteractionOverrideConfigContent;
        bool m_InteractionOverrideConfigExpanded;
        Vector2 m_InteractionOverrideScrollPosition;

        readonly Dictionary<Object, HashSet<Object>> m_InteractionOverridesLookup = new Dictionary<Object, HashSet<Object>>();

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        /// <seealso cref="MonoBehaviour"/>
        protected virtual void OnEnable()
        {
            m_TargetInteractionGroup = target as XRInteractionGroup;

            m_InteractionManager = serializedObject.FindProperty("m_InteractionManager");
            m_StartingGroupMembers = serializedObject.FindProperty("m_StartingGroupMembers");
            m_StartingInteractionOverridesMap = serializedObject.FindProperty("m_StartingInteractionOverridesMap");

            m_InteractionOverrideConfigContent = EditorGUIUtility.TrTextContent("Interaction Override Configuration",
                m_StartingInteractionOverridesMap.tooltip);

            m_InteractionOverrideConfigExpanded = SessionState.GetBool(k_InteractionOverrideConfigExpandedKey, true);
            m_InteractionOverridesLookup.Clear();
        }

        /// <inheritdoc />
        protected override void DrawInspector()
        {
            DrawBeforeProperties();
            DrawProperties();
            DrawDerivedProperties();
        }

        /// <summary>
        /// This method is automatically called by <see cref="DrawInspector"/> to
        /// draw the section of the custom inspector before <see cref="DrawProperties"/>.
        /// By default, this draws the read-only Script property.
        /// </summary>
        protected virtual void DrawBeforeProperties()
        {
            DrawScript();
        }

        /// <summary>
        /// This method is automatically called by <see cref="DrawInspector"/> to
        /// draw the property fields. Override this method to customize the
        /// properties shown in the Inspector. This is typically the method overridden
        /// when a derived behavior adds additional serialized properties that should
        /// be displayed in the Inspector.
        /// </summary>
        protected virtual void DrawProperties()
        {
            using (new EditorGUI.DisabledScope(m_TargetInteractionGroup.isRegisteredWithInteractionManager))
                EditorGUILayout.PropertyField(m_InteractionManager);

            using (new EditorGUI.DisabledScope(m_TargetInteractionGroup.hasRegisteredStartingMembers))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUILayout.PropertyField(m_StartingGroupMembers);
                    if (check.changed)
                    {
                        serializedObject.ApplyModifiedProperties();
                        m_TargetInteractionGroup.RemoveMissingMembersFromStartingOverridesMap();
                        serializedObject.Update();
                    }
                    else
                    {
                        RemoveNullMembersFromStartingOverridesMap();
                    }
                }

                DrawInteractionOverrideConfiguration();
            }
        }

        void RemoveNullMembersFromStartingOverridesMap()
        {
            for (var i = m_StartingInteractionOverridesMap.arraySize - 1; i >= 0; i--)
            {
                var memberAndOverridesPairProperty = m_StartingInteractionOverridesMap.GetArrayElementAtIndex(i);
                var groupMemberProperty = memberAndOverridesPairProperty.FindPropertyRelative(k_MemberAndOverridesPairGroupMemberPropertyName);
                var overrideGroupMembersProperty = memberAndOverridesPairProperty.FindPropertyRelative(k_MemberAndOverridesPairOverrideGroupMembersPropertyName);
                var groupMember = groupMemberProperty.objectReferenceValue;
                if (groupMember == null)
                {
                    m_StartingInteractionOverridesMap.DeleteArrayElementAtIndex(i);
                }
                else
                {
                    for (var j = overrideGroupMembersProperty.arraySize - 1; j >= 0; j--)
                    {
                        if (overrideGroupMembersProperty.GetArrayElementAtIndex(j) == null)
                            overrideGroupMembersProperty.DeleteArrayElementAtIndex(j);
                    }
                }
            }
        }

        /// <summary>
        /// Draw the Interaction Override Configuration foldout.
        /// </summary>
        /// <seealso cref="DrawInteractionOverrideConfigurationNested"/>
        protected virtual void DrawInteractionOverrideConfiguration()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_InteractionOverrideConfigExpanded = EditorGUILayout.Foldout(m_InteractionOverrideConfigExpanded,
                    m_InteractionOverrideConfigContent, true);

                if (check.changed)
                    SessionState.SetBool(k_InteractionOverrideConfigExpandedKey, m_InteractionOverrideConfigExpanded);
            }

            if (!m_InteractionOverrideConfigExpanded)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                DrawInteractionOverrideConfigurationNested();
            }
        }

        /// <summary>
        /// Draw the nested contents of the Interaction Override Configuration foldout.
        /// </summary>
        /// <seealso cref="DrawInteractionOverrideConfiguration"/>
        protected virtual void DrawInteractionOverrideConfigurationNested()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var membersCount = m_StartingGroupMembers.arraySize;
                var memberNameLabelWidth = 0f;
                for (var i = 0; i < membersCount; i++)
                {
                    var memberObject = m_StartingGroupMembers.GetArrayElementAtIndex(i).objectReferenceValue;
                    var memberName = memberObject != null ? memberObject.name : "";
                    var labelSize = GUI.skin.label.CalcSize(new GUIContent(memberName));
                    if (labelSize.x > memberNameLabelWidth)
                        memberNameLabelWidth = labelSize.x;
                }

                // GUILayout.Width does not seem to respect indent level
                var indent = EditorGUI.indentLevel * 15f;
                var leftColumnWidth = memberNameLabelWidth + indent;
                var canOverrideLabelWidth = GUI.skin.label.CalcSize(k_CanOverrideLabelContent).x;
                var tableColumnWidth = Mathf.Max(memberNameLabelWidth, canOverrideLabelWidth) + indent;

                // Draw left side of override config table (labels for source group members)
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField("", GUILayout.Width(leftColumnWidth)); // [Override Name]
                    EditorGUILayout.LabelField("", GUILayout.Width(leftColumnWidth)); // "Can Override"
                    for (var i = 0; i < membersCount; i++)
                    {
                        var sourceMemberObject = m_StartingGroupMembers.GetArrayElementAtIndex(i).objectReferenceValue;
                        if (sourceMemberObject != null)
                            EditorGUILayout.LabelField(sourceMemberObject.name, GUILayout.Width(leftColumnWidth));
                    }
                }

                // Draw scrollable part of override config table (labels and checkboxes for each override)
                using (var scrollView = new EditorGUILayout.ScrollViewScope(m_InteractionOverrideScrollPosition))
                {
                    m_InteractionOverrideScrollPosition = scrollView.scrollPosition;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        for (var i = 0; i < membersCount; i++)
                        {
                            var overrideMemberObject = m_StartingGroupMembers.GetArrayElementAtIndex(i).objectReferenceValue;
                            if (overrideMemberObject != null)
                                EditorGUILayout.LabelField(overrideMemberObject.name, GUILayout.Width(tableColumnWidth));
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        for (var i = 0; i < membersCount; i++)
                        {
                            var overrideMemberObject = m_StartingGroupMembers.GetArrayElementAtIndex(i).objectReferenceValue;
                            if (overrideMemberObject != null)
                                EditorGUILayout.LabelField(k_CanOverrideLabelContent, GUILayout.Width(tableColumnWidth));
                        }
                    }

                    UpdateInteractionOverridesLookup();
                    for (var i = 0; i < membersCount; i++)
                    {
                        var sourceMemberObject = m_StartingGroupMembers.GetArrayElementAtIndex(i).objectReferenceValue;
                        if (sourceMemberObject == null)
                            continue;

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            var hasOverrides = m_InteractionOverridesLookup.TryGetValue(sourceMemberObject, out var overrideMembers);
                            for (var j = 0; j < membersCount; j++)
                            {
                                var overrideMemberObject = m_StartingGroupMembers.GetArrayElementAtIndex(j).objectReferenceValue;
                                if (overrideMemberObject == null)
                                    continue;

                                if (ReferenceEquals(overrideMemberObject, sourceMemberObject))
                                {
                                    using (new EditorGUI.DisabledScope(true))
                                        EditorGUILayout.Toggle(false, GUILayout.Width(tableColumnWidth));

                                    continue;
                                }

                                if (GroupMemberIsPartOfOverrideChain(overrideMemberObject, sourceMemberObject))
                                {
                                    using (new EditorGUI.DisabledScope(true))
                                        EditorGUILayout.Toggle(false, GUILayout.Width(tableColumnWidth));

                                    continue;
                                }

                                var canOverride = hasOverrides && overrideMembers.Contains(overrideMemberObject);
                                using (var check = new EditorGUI.ChangeCheckScope())
                                {
                                    var toggleValue = EditorGUILayout.Toggle(canOverride, GUILayout.Width(tableColumnWidth));
                                    if (check.changed)
                                    {
                                        if (toggleValue)
                                            AddStartingInteractionOverride(sourceMemberObject, overrideMemberObject);
                                        else
                                            RemoveStartingInteractionOverride(sourceMemberObject, overrideMemberObject);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void AddStartingInteractionOverride(Object sourceMember, Object overrideMember)
        {
            if (m_InteractionOverridesLookup.TryGetValue(sourceMember, out var overrideMembers))
                overrideMembers.Add(overrideMember);
            else
                m_InteractionOverridesLookup[sourceMember] = new HashSet<Object> { overrideMember };

            for (var i = 0; i < m_StartingInteractionOverridesMap.arraySize; i++)
            {
                var memberAndOverridesPairProperty = m_StartingInteractionOverridesMap.GetArrayElementAtIndex(i);
                var groupMemberProperty = memberAndOverridesPairProperty.FindPropertyRelative(k_MemberAndOverridesPairGroupMemberPropertyName);
                if (groupMemberProperty.objectReferenceValue == sourceMember)
                {
                    var overrideGroupMembersProperty = memberAndOverridesPairProperty.FindPropertyRelative(k_MemberAndOverridesPairOverrideGroupMembersPropertyName);
                    var addedOverrideProperty =
                        overrideGroupMembersProperty.GetArrayElementAtIndex(overrideGroupMembersProperty.arraySize++);

                    addedOverrideProperty.objectReferenceValue = overrideMember;
                    return;
                }
            }

            var addedMemberAndOverridesPairProperty =
                m_StartingInteractionOverridesMap.GetArrayElementAtIndex(
                    m_StartingInteractionOverridesMap.arraySize++);

            var newGroupMemberProperty = addedMemberAndOverridesPairProperty.FindPropertyRelative(k_MemberAndOverridesPairGroupMemberPropertyName);
            newGroupMemberProperty.objectReferenceValue = sourceMember;
            var newOverrideGroupMembersProperty = addedMemberAndOverridesPairProperty.FindPropertyRelative(k_MemberAndOverridesPairOverrideGroupMembersPropertyName);
            newOverrideGroupMembersProperty.arraySize = 1;
            var newOverrideProperty = newOverrideGroupMembersProperty.GetArrayElementAtIndex(0);
            newOverrideProperty.objectReferenceValue = overrideMember;
        }

        void RemoveStartingInteractionOverride(Object sourceMember, Object overrideMember)
        {
            if (m_InteractionOverridesLookup.TryGetValue(sourceMember, out var overrideMembers))
                overrideMembers.Remove(overrideMember);

            for (var i = 0; i < m_StartingInteractionOverridesMap.arraySize; i++)
            {
                var memberAndOverridesPairProperty = m_StartingInteractionOverridesMap.GetArrayElementAtIndex(i);
                var groupMemberProperty = memberAndOverridesPairProperty.FindPropertyRelative(k_MemberAndOverridesPairGroupMemberPropertyName);
                if (groupMemberProperty.objectReferenceValue == sourceMember)
                {
                    var overrideGroupMembersProperty = memberAndOverridesPairProperty.FindPropertyRelative(k_MemberAndOverridesPairOverrideGroupMembersPropertyName);
                    for (var j = overrideGroupMembersProperty.arraySize - 1; j >= 0; j--)
                    {
                        var overrideMemberProperty = overrideGroupMembersProperty.GetArrayElementAtIndex(j);
                        if (overrideMemberProperty.objectReferenceValue == overrideMember)
                        {
                            overrideGroupMembersProperty.DeleteArrayElementAtIndex(j);
                            return;
                        }
                    }
                }
            }
        }

        void UpdateInteractionOverridesLookup()
        {
            foreach (var groupAndOverridesPair in m_InteractionOverridesLookup)
            {
                groupAndOverridesPair.Value.Clear();
            }

            for (var i = 0; i < m_StartingInteractionOverridesMap.arraySize; i++)
            {
                var memberAndOverridesPairProperty = m_StartingInteractionOverridesMap.GetArrayElementAtIndex(i);
                var groupMemberProperty = memberAndOverridesPairProperty.FindPropertyRelative(k_MemberAndOverridesPairGroupMemberPropertyName);
                var overrideGroupMembersProperty = memberAndOverridesPairProperty.FindPropertyRelative(k_MemberAndOverridesPairOverrideGroupMembersPropertyName);
                var groupMember = groupMemberProperty.objectReferenceValue;
                if (groupMember == null)
                    continue;

                if (!m_InteractionOverridesLookup.TryGetValue(groupMember, out var overridesSet))
                    overridesSet = new HashSet<Object>();

                for (var j = 0; j < overrideGroupMembersProperty.arraySize; j++)
                {
                    overridesSet.Add(overrideGroupMembersProperty.GetArrayElementAtIndex(j).objectReferenceValue);
                }

                m_InteractionOverridesLookup[groupMember] = overridesSet;
            }
        }

        bool GroupMemberIsPartOfOverrideChain(Object sourceGroupMember, Object potentialOverrideGroupMember)
        {
            if (ReferenceEquals(potentialOverrideGroupMember, sourceGroupMember))
                return true;

            if (!m_InteractionOverridesLookup.TryGetValue(sourceGroupMember, out var receivers))
                return false;

            foreach (var nextReceiver in receivers)
            {
                if (GroupMemberIsPartOfOverrideChain(nextReceiver, potentialOverrideGroupMember))
                    return true;
            }

            return false;
        }
    }
}