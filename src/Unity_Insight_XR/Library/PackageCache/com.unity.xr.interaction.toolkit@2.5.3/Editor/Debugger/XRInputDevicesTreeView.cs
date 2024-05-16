using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.XR;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Multi-column <see cref="TreeView"/> that shows Input Devices.
    /// </summary>
    class XRInputDevicesTreeView : TreeView
    {
        public static XRInputDevicesTreeView Create(ref TreeViewState treeState, ref MultiColumnHeaderState headerState)
        {
            if (treeState == null)
                treeState = new TreeViewState();

            var newHeaderState = CreateHeaderState();
            if (headerState != null)
                MultiColumnHeaderState.OverwriteSerializedFields(headerState, newHeaderState);
            headerState = newHeaderState;

            var header = new MultiColumnHeader(headerState);
            return new XRInputDevicesTreeView(treeState, header);
        }

        const float k_RowHeight = 20f;

        /// <summary>
        /// Temporary scratch list to store the method call results into.
        /// </summary>
        static readonly List<InputDevice> s_InputDevices = new List<InputDevice>();

        /// <summary>
        /// Temporary scratch list to store the method call results into.
        /// </summary>
        static readonly List<InputFeatureUsage> s_FeatureUsages = new List<InputFeatureUsage>();

        /// <summary>
        /// Dictionary containing all valid <see cref="InputDevice"/> instances and the <see cref="XRNode"/> values.
        /// </summary>
        static readonly Dictionary<InputDevice, List<XRNode>> s_NodesForAllInputDevices = new Dictionary<InputDevice, List<XRNode>>();

        /// <summary>
        /// Array containing all values of the <see cref="XRNode"/> <see langword="enum"/>.
        /// </summary>
        static XRNode[] s_XRNodes;

        class DeviceItem : TreeViewItem
        {
            public string characteristics;
            public string role;
            public string xrNodes;
            public string manufacturer;
            public string subsystem;
        }

        class FeatureItem : TreeViewItem
        {
            public InputDevice inputDevice;
            public InputFeatureUsage featureUsage;
            public string typeString;
        }

        enum ColumnId
        {
            Name,
            Type,
            Value,
            Characteristics,
            Role,
            Node,
            Manufacturer,
            Subsystem,

            Count,
        }

        static MultiColumnHeaderState CreateHeaderState()
        {
            var columns = new MultiColumnHeaderState.Column[(int)ColumnId.Count];

            columns[(int)ColumnId.Name] =
                new MultiColumnHeaderState.Column
                {
                    width = 240f,
                    minWidth = 60f,
                    headerContent = EditorGUIUtility.TrTextContent("Name"),
                };
            columns[(int)ColumnId.Type] =
                new MultiColumnHeaderState.Column
                {
                    width = 200f,
                    headerContent = EditorGUIUtility.TrTextContent("Type"),
                };
            columns[(int)ColumnId.Value] =
                new MultiColumnHeaderState.Column
                {
                    width = 250f,
                    headerContent = EditorGUIUtility.TrTextContent("Value"),
                };
            columns[(int)ColumnId.Characteristics] =
                new MultiColumnHeaderState.Column
                {
                    width = 200f,
                    minWidth = 60f,
                    headerContent = EditorGUIUtility.TrTextContent("Characteristics"),
                };
            columns[(int)ColumnId.Role] =
                new MultiColumnHeaderState.Column
                {
                    width = 150f,
                    headerContent = EditorGUIUtility.TrTextContent("Role (Deprecated)"),
                };
            columns[(int)ColumnId.Node] =
                new MultiColumnHeaderState.Column
                {
                    width = 150f,
                    headerContent = EditorGUIUtility.TrTextContent("XR Nodes"),
                };
            columns[(int)ColumnId.Manufacturer] =
                new MultiColumnHeaderState.Column
                {
                    width = 150f,
                    headerContent = EditorGUIUtility.TrTextContent("Manufacturer"),
                };
            columns[(int)ColumnId.Subsystem] =
                new MultiColumnHeaderState.Column
                {
                    width = 200f,
                    headerContent = EditorGUIUtility.TrTextContent("Input Subsystem"),
                };

            return new MultiColumnHeaderState(columns);
        }

        XRInputDevicesTreeView(TreeViewState state, MultiColumnHeader header)
            : base(state, header)
        {
            showBorder = false;
            rowHeight = k_RowHeight;
            Reload();

            InputDevices.deviceConnected += OnDeviceConnected;
            InputDevices.deviceDisconnected += OnDeviceDisconnected;
            InputDevices.deviceConfigChanged += OnDeviceConfigChanged;
        }

        /// <summary>
        /// Call this when this tree has no more use.
        /// </summary>
        public void Release()
        {
            InputDevices.deviceConnected -= OnDeviceConnected;
            InputDevices.deviceDisconnected -= OnDeviceDisconnected;
            InputDevices.deviceConfigChanged -= OnDeviceConfigChanged;
        }

        void OnDeviceConnected(InputDevice inputDevice) => Reload();

        void OnDeviceDisconnected(InputDevice inputDevice) => Reload();

        void OnDeviceConfigChanged(InputDevice inputDevice) => Reload();

        protected override TreeViewItem BuildRoot()
        {
            // Wrap root control in invisible item required by TreeView.
            var root = new TreeViewItem
            {
                id = 0,
                depth = -1,
            };
            root.children = BuildInputDevicesTree(root);

            return root;
        }

        static List<TreeViewItem> BuildInputDevicesTree(TreeViewItem rootItem)
        {
            // Initialize XRNodes array with all enum values.
            if (s_XRNodes == null)
            {
                var array = Enum.GetValues(typeof(XRNode));
                s_XRNodes = new XRNode[array.Length];
                Array.Copy(array, s_XRNodes, array.Length);
            }

            // To identify all the XRNode values associated with each InputDevice, we have to
            // piece it together by getting all input devices for each XRNode value.
            s_NodesForAllInputDevices.Clear();
            foreach (var xrNode in s_XRNodes)
            {
                s_InputDevices.Clear();
                InputDevices.GetDevicesAtXRNode(xrNode, s_InputDevices);

                foreach (var device in s_InputDevices)
                {
                    if (!s_NodesForAllInputDevices.TryGetValue(device, out var deviceXRNodes))
                    {
                        deviceXRNodes = new List<XRNode>();
                        s_NodesForAllInputDevices[device] = deviceXRNodes;
                    }

                    deviceXRNodes.Add(xrNode);
                }
            }

            // Build children
            var items = new List<TreeViewItem>();

            // Build device items
            foreach (var kvp in s_NodesForAllInputDevices)
            {
                var device = kvp.Key;
                var xrNodes = kvp.Value;

                var deviceItem = new DeviceItem
                {
                    id = UniqueIdGenerator.GetUniqueTreeViewId(device),
                    displayName = device.name,
                    characteristics = device.characteristics.ToString(),
#pragma warning disable 612, 618
                    role = device.role.ToString(),
#pragma warning restore 612, 618
                    xrNodes = string.Join(", ", xrNodes),
                    manufacturer = device.manufacturer,
                    subsystem = device.subsystem.subsystemDescriptor.id,
                    depth = 0,
                    parent = rootItem,
                };

                // Build feature items
                s_FeatureUsages.Clear();
                device.TryGetFeatureUsages(s_FeatureUsages);

                var featureChildren = new List<TreeViewItem>();
                for (var index = 0; index < s_FeatureUsages.Count; ++index)
                {
                    var featureUsage = s_FeatureUsages[index];
                    var featureItem = new FeatureItem
                    {
                        id = UniqueIdGenerator.GetUniqueTreeViewId(device, featureUsage, index),
                        displayName = featureUsage.name,
                        inputDevice = device,
                        featureUsage = featureUsage,
                        typeString = featureUsage.type.ToString(),
                        depth = 1,
                        parent = deviceItem,
                    };
                    featureChildren.Add(featureItem);
                }

                deviceItem.children = featureChildren;
                items.Add(deviceItem);
            }

            // Sort devices by name, then by id (to create a stable order when the names match)
            items.Sort((a, b) =>
            {
                var nameCompare = string.Compare(a.displayName, b.displayName);
                return nameCompare != 0 ? nameCompare : a.id.CompareTo(b.id);
            });

            return items;
        }

        static string GetFeatureValue(InputDevice device, InputFeatureUsage featureUsage)
        {
            // InputFeatureType.Custom
            if (featureUsage.type == typeof(byte[]))
                return "System.Byte[]";

            // InputFeatureType.Binary
            if (featureUsage.type == typeof(bool))
                return device.TryGetFeatureValue(featureUsage.As<bool>(), out var boolValue) ? boolValue.ToString() : string.Empty;

            // InputFeatureType.DiscreteStates
            if (featureUsage.type == typeof(uint))
            {
                if (device.TryGetFeatureValue(featureUsage.As<uint>(), out var uintValue))
                {
                    return featureUsage.name.Contains("TrackingState") ? ((InputTrackingState)uintValue).ToString() : uintValue.ToString();
                }

                return string.Empty;
            }

            // InputFeatureType.Axis1D
            if (featureUsage.type == typeof(float))
                return device.TryGetFeatureValue(featureUsage.As<float>(), out var floatValue) ? floatValue.ToString() : string.Empty;

            // InputFeatureType.Axis2D
            if (featureUsage.type == typeof(Vector2))
                return device.TryGetFeatureValue(featureUsage.As<Vector2>(), out var vector2Value) ? vector2Value.ToString() : string.Empty;

            // InputFeatureType.Axis3D
            if (featureUsage.type == typeof(Vector3))
                return device.TryGetFeatureValue(featureUsage.As<Vector3>(), out var vector3Value) ? vector3Value.ToString() : string.Empty;

            // InputFeatureType.Rotation
            if (featureUsage.type == typeof(Quaternion))
                return device.TryGetFeatureValue(featureUsage.As<Quaternion>(), out var quaternionValue) ? quaternionValue.ToString() : string.Empty;

            // InputFeatureType.Hand
            if (featureUsage.type == typeof(Hand))
                return device.TryGetFeatureValue(featureUsage.As<Hand>(), out var handValue) ? handValue.ToString() : string.Empty;

            // InputFeatureType.Bone
            if (featureUsage.type == typeof(Bone))
            {
                if (device.TryGetFeatureValue(featureUsage.As<Bone>(), out var boneValue))
                {
                    if (boneValue.TryGetPosition(out var bonePosition) &&
                        boneValue.TryGetRotation(out var boneRotation))
                        return $"{bonePosition}, {boneRotation}";
                }
            }

            // InputFeatureType.Eyes
            if (featureUsage.type == typeof(Eyes))
            {
                if (device.TryGetFeatureValue(featureUsage.As<Eyes>(), out var eyesValue))
                {
                    if (eyesValue.TryGetFixationPoint(out var fixation) &&
                        eyesValue.TryGetLeftEyeOpenAmount(out var leftOpen) &&
                        eyesValue.TryGetLeftEyePosition(out var leftPosition) &&
                        eyesValue.TryGetLeftEyeRotation(out var leftRotation) &&
                        eyesValue.TryGetRightEyeOpenAmount(out var rightOpen) &&
                        eyesValue.TryGetRightEyePosition(out var rightPosition) &&
                        eyesValue.TryGetRightEyeRotation(out var rightRotation))
                        return $"{fixation}, Left {{{leftOpen}, {leftPosition}, {leftRotation}}}, Right {{{rightOpen}, {rightPosition}, {rightRotation}}}";
                }
            }

            return string.Empty;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (!Application.isPlaying)
                return;

            var columnCount = args.GetNumVisibleColumns();
            for (var i = 0; i < columnCount; ++i)
            {
                ColumnGUI(args.GetCellRect(i), args.item, args.GetColumn(i), ref args);
            }
        }

        void ColumnGUI(Rect cellRect, TreeViewItem item, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            if (column == (int)ColumnId.Name)
            {
                args.rowRect = cellRect;
                base.RowGUI(args);
            }

            var deviceItem = item as DeviceItem;
            var featureItem = item as FeatureItem;

            switch (column)
            {
                case (int)ColumnId.Type:
                    if (item is FeatureItem)
                        GUI.Label(cellRect, featureItem.typeString);
                    break;
                case (int)ColumnId.Value:
                    if (item is FeatureItem)
                        GUI.Label(cellRect, GetFeatureValue(featureItem.inputDevice, featureItem.featureUsage));
                    break;
                case (int)ColumnId.Characteristics:
                    if (item is DeviceItem)
                        GUI.Label(cellRect, deviceItem.characteristics);
                    break;
                case (int)ColumnId.Role:
                    if (item is DeviceItem)
                        GUI.Label(cellRect, deviceItem.role);
                    break;
                case (int)ColumnId.Node:
                    if (item is DeviceItem)
                        GUI.Label(cellRect, deviceItem.xrNodes);
                    break;
                case (int)ColumnId.Manufacturer:
                    if (item is DeviceItem)
                        GUI.Label(cellRect, deviceItem.manufacturer);
                    break;
                case (int)ColumnId.Subsystem:
                    if (item is DeviceItem)
                        GUI.Label(cellRect, deviceItem.subsystem);
                    break;
            }
        }

        /// <summary>
        /// Alternate version of <see cref="XRInteractionDebuggerWindow.GetUniqueTreeViewId"/> which works
        /// with multiple values that seed each row in this tree.
        /// </summary>
        static class UniqueIdGenerator
        {
            // Incrementing unique ID counter, which is shared by all the row types in this tree
            static int s_UniqueIdCounter = 1;

            // Maps from the source for each row to the unique ID for the row
            static readonly Dictionary<InputDevice, int> s_DeviceGeneratedIds = new Dictionary<InputDevice, int>();
            static readonly Dictionary<(InputDevice, InputFeatureUsage, int), int> s_FeatureGeneratedIds = new Dictionary<(InputDevice, InputFeatureUsage, int), int>();

            public static int GetUniqueTreeViewId(InputDevice inputDevice)
            {
                if (s_DeviceGeneratedIds.TryGetValue(inputDevice, out var id))
                    return id;


                id = CreateId();
                s_DeviceGeneratedIds.Add(inputDevice, id);

                return id;
            }

            public static int GetUniqueTreeViewId(InputDevice inputDevice, InputFeatureUsage featureUsage, int index)
            {
                if (s_FeatureGeneratedIds.TryGetValue((inputDevice, featureUsage, index), out var id))
                    return id;


                id = CreateId();
                s_FeatureGeneratedIds.Add((inputDevice, featureUsage, index), id);

                return id;
            }

            static int CreateId()
            {
                return s_UniqueIdCounter++;
            }
        }
    }
}
