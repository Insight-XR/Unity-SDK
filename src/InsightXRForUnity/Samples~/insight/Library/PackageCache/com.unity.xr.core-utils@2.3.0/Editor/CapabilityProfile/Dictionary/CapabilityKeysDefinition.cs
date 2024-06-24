using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if UNITY_2020_1_OR_NEWER
using UnityEditor;
#endif

namespace Unity.XR.CoreUtils.Capabilities.Editor
{
    static class CapabilityKeysDefinition
    {
        const string k_InvalidCapabilityErrorFormat = "Invalid capability key, the capability cannot be null or empty, in {0}.{1}.";
        const string k_DuplicatedCapabilityErrorFormat = "Capability key \"{0}\" is already defined.";

        static readonly string k_InvalidFieldErrorFormat = $"Use {nameof(CustomCapabilityKeyAttribute)} with constant string fields, in {{0}}.{{1}}.";

        static readonly string[] k_CapabilityKeys;
        internal static string[] CapabilityKeys => k_CapabilityKeys;

#if !UNITY_2020_1_OR_NEWER
        static readonly List<FieldInfo> k_Fields = new List<FieldInfo>();
#endif

        static CapabilityKeysDefinition()
        {
            var capabilityKeys = new List<string>();
            var capabilityOrders = new List<int>();

            DefineBaseCapabilities(capabilityKeys, capabilityOrders);
            DefineCustomCapabilities(capabilityKeys, capabilityOrders);

            k_CapabilityKeys = capabilityKeys.ToArray();
        }

        static void DefineBaseCapabilities(List<string> capabilityKeys, List<int> capabilityOrders)
        {
            DefineCapability(capabilityKeys, capabilityOrders, StandardCapabilityKeys.ControllersInput, int.MinValue);
            DefineCapability(capabilityKeys, capabilityOrders, StandardCapabilityKeys.HandsInput, int.MinValue);
            DefineCapability(capabilityKeys, capabilityOrders, StandardCapabilityKeys.EyeGazeInput, int.MinValue);
            DefineCapability(capabilityKeys, capabilityOrders, StandardCapabilityKeys.WorldDataInput, int.MinValue);
            DefineCapability(capabilityKeys, capabilityOrders, StandardCapabilityKeys.FaceTracking, int.MinValue);
        }

        static void DefineCustomCapabilities(List<string> capabilityKeys, List<int> capabilityOrders)
        {
#if UNITY_2020_1_OR_NEWER
            var extractedFields = TypeCache.GetFieldsWithAttribute<CustomCapabilityKeyAttribute>();
#else
            k_Fields.Clear();
            ReflectionUtils.GetFieldsWithAttribute(typeof(CustomCapabilityKeyAttribute), k_Fields);
            var extractedFields = k_Fields;
#endif
            foreach (var fieldInfo in extractedFields)
            {
                if (!IsCustomCapabilityFieldValid(fieldInfo, out var attribute))
                    continue;

                var customCapability = fieldInfo.GetValue(null) as string;
                if (!IsCustomCapabilityValid(customCapability, fieldInfo))
                    continue;

                DefineCapability(capabilityKeys, capabilityOrders, customCapability, attribute.Order);
            }
        }

        static bool IsCustomCapabilityFieldValid(FieldInfo fieldInfo, out CustomCapabilityKeyAttribute attribute)
        {
            attribute = (CustomCapabilityKeyAttribute)fieldInfo.GetCustomAttributes(typeof(CustomCapabilityKeyAttribute), false)[0];

            if (fieldInfo.IsLiteral && fieldInfo.FieldType == typeof(string))
                return attribute != null;

            Debug.LogErrorFormat(k_InvalidFieldErrorFormat, fieldInfo.DeclaringType?.Name, fieldInfo.Name);
            return false;
        }

        static bool IsCustomCapabilityValid(string capability, FieldInfo fieldInfo)
        {
            if (string.IsNullOrEmpty(capability))
            {
                Debug.LogErrorFormat(k_InvalidCapabilityErrorFormat, fieldInfo.DeclaringType?.Name, fieldInfo.Name);
                return false;
            }

            return true;
        }

        static void DefineCapability(List<string> capabilityKeys, List<int> capabilityOrders, string capability, int order)
        {
            if (capabilityKeys.Contains(capability))
            {
                Debug.LogErrorFormat(k_DuplicatedCapabilityErrorFormat, capability);
                return;
            }

            var indexToInsert = 0;
            while (indexToInsert < capabilityKeys.Count && capabilityOrders[indexToInsert] <= order)
                indexToInsert++;

            capabilityKeys.Insert(indexToInsert, capability);
            capabilityOrders.Insert(indexToInsert, order);
        }
    }
}
