using UnityEditor;
using UnityEngine;

namespace Unity.XR.CoreUtils.Editor
{
    /// <summary>
    /// Utility methods for use in Editor GUI code.
    /// </summary>
    public static class EditorGUIUtils
    {
        /// <summary>
        /// A class that holds styles for urls and word wrap mini labels.
        /// </summary>
        public static class Styles
        {
            /// <summary>
            /// Style for word wrap mini labels.
            /// </summary>
            public static readonly GUIStyle WordWrapMiniLabel = new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true,
                richText = true
            };

            internal static readonly GUIStyle UrlLabelPersonal = new GUIStyle(EditorStyles.miniLabel)
            {
                name = "url-label",
                richText = true,
                normal = new GUIStyleState {textColor = new Color(8 / 255f, 8 / 255f, 252 / 255f)},
            };

            internal static readonly GUIStyle UrlLabelProfessional = new GUIStyle(EditorStyles.miniLabel)
            {
                name = "url-label",
                richText = true,
                normal = new GUIStyleState {textColor = new Color(79 / 255f, 128 / 255f, 248 / 255f)},
            };
        }

        /// <summary>
        /// Creates a link for richtext.
        /// </summary>
        /// <param name="linkTitle">Title of the link, this is what the user will see and click on.</param>
        /// <param name="linkUrl">The url of the link.</param>
        public static void DrawLink(GUIContent linkTitle, string linkUrl)
        {
            var labelStyle = EditorGUIUtility.isProSkin ? Styles.UrlLabelProfessional : Styles.UrlLabelPersonal;
            var size = labelStyle.CalcSize(linkTitle);
            var uriRect = GUILayoutUtility.GetRect(linkTitle, labelStyle);
            uriRect.x += 2;
            uriRect.width = size.x;
            if (UnityEngine.GUI.Button(uriRect, linkTitle, labelStyle))
                Application.OpenURL(linkUrl);

            EditorGUIUtility.AddCursorRect(uriRect, MouseCursor.Link);
            EditorGUI.DrawRect(new Rect(uriRect.x, uriRect.y + uriRect.height - 1, uriRect.width, 1),
                labelStyle.normal.textColor);
        }
    }
}
