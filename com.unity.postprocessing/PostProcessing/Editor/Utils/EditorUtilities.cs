using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEditor.Rendering.PostProcessing
{
    /// <summary>
    /// A set of editor utilities used in post-processing editors.
    /// </summary>
    public static class EditorUtilities
    {
        static Dictionary<string, GUIContent> s_GUIContentCache;
        static Dictionary<Type, AttributeDecorator> s_AttributeDecorators;

        static PostProcessEffectSettings s_ClipboardContent;

        /// <summary>
        /// Returns <c>true</c> if the current target is a console, <c>false</c> otherwise.
        /// </summary>
        public static bool isTargetingConsoles
        {
            get
            {
                var t = EditorUserBuildSettings.activeBuildTarget;
                return t == BuildTarget.PS4
#if UNITY_PS5
                    || t == BuildTarget.PS5
#endif
                    || t == BuildTarget.XboxOne
                    || t == BuildTarget.Switch;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the current target is a mobile, <c>false</c> otherwise.
        /// </summary>
        public static bool isTargetingMobiles
        {
            get
            {
                var t = EditorUserBuildSettings.activeBuildTarget;
                return t == BuildTarget.Android
                    || t == BuildTarget.iOS
                    || t == BuildTarget.tvOS
#if !UNITY_2018_2_OR_NEWER
                    || t == BuildTarget.Tizen
#endif
#if !UNITY_2018_3_OR_NEWER
                    || t == BuildTarget.N3DS
                    || t == BuildTarget.PSP2
#endif
                ;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the current target is a console or a mobile, <c>false</c>
        /// otherwise.
        /// </summary>
        public static bool isTargetingConsolesOrMobiles
        {
            get { return isTargetingConsoles || isTargetingMobiles; }
        }

        static EditorUtilities()
        {
            s_GUIContentCache = new Dictionary<string, GUIContent>();
            s_AttributeDecorators = new Dictionary<Type, AttributeDecorator>();
            ReloadDecoratorTypes();
        }

        [Callbacks.DidReloadScripts]
        static void OnEditorReload()
        {
            ReloadDecoratorTypes();
        }

        static void ReloadDecoratorTypes()
        {
            s_AttributeDecorators.Clear();

            // Look for all the valid attribute decorators
            var types = RuntimeUtilities.GetAllTypesDerivedFrom<AttributeDecorator>()
                .Where(
                    t => t.IsDefined(typeof(DecoratorAttribute), false)
                    && !t.IsAbstract
                );

            // Store them
            foreach (var type in types)
            {
                var attr = type.GetAttribute<DecoratorAttribute>();
                var decorator = (AttributeDecorator)Activator.CreateInstance(type);
                s_AttributeDecorators.Add(attr.attributeType, decorator);
            }
        }

        internal static AttributeDecorator GetDecorator(Type attributeType)
        {
            AttributeDecorator decorator;
            return !s_AttributeDecorators.TryGetValue(attributeType, out decorator)
                ? null
                : decorator;
        }

        /// <summary>
        /// Gets a <see cref="GUIContent"/> for the given label and tooltip. These are recycled
        /// internally and help reduce the garbage collector pressure in the editor.
        /// </summary>
        /// <param name="textAndTooltip">The label and tooltip separated by a <c>|</c>
        /// character</param>
        /// <returns>A recycled <see cref="GUIContent"/></returns>
        public static GUIContent GetContent(string textAndTooltip)
        {
            if (string.IsNullOrEmpty(textAndTooltip))
                return GUIContent.none;

            GUIContent content;

            if (!s_GUIContentCache.TryGetValue(textAndTooltip, out content))
            {
                var s = textAndTooltip.Split('|');
                content = new GUIContent(s[0]);

                if (s.Length > 1 && !string.IsNullOrEmpty(s[1]))
                    content.tooltip = s[1];

                s_GUIContentCache.Add(textAndTooltip, content);
            }

            return content;
        }

        /// <summary>
        /// Draws a UI box with a description and a "Fix Me" button next to it.
        /// </summary>
        /// <param name="text">The description</param>
        /// <param name="action">The action to execute when the button is clicked</param>
        public static void DrawFixMeBox(string text, Action action)
        {
            Assert.IsNotNull(action);

            EditorGUILayout.HelpBox(text, MessageType.Warning);

            GUILayout.Space(-32);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Fix", GUILayout.Width(60)))
                    action();

                GUILayout.Space(8);
            }
            GUILayout.Space(11);
        }

        /// <summary>
        /// Draws a horizontal split line.
        /// </summary>
        public static void DrawSplitter()
        {
            var rect = GUILayoutUtility.GetRect(1f, 1f);

            // Splitter rect should be full-width
            rect.xMin = 0f;
            rect.width += 4f;

            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(rect, Styling.splitter);
        }

        /// <summary>
        /// Draws a toggle using the "override checkbox" style.
        /// </summary>
        /// <param name="rect">The position and size of the toggle</param>
        /// <param name="property">The override state property for the toggle</param>
        public static void DrawOverrideCheckbox(Rect rect, SerializedProperty property)
        {
            property.boolValue = GUI.Toggle(rect, property.boolValue, GetContent("|Override this setting for this volume."), Styling.smallTickbox);
        }

        /// <summary>
        /// Draws a header label.
        /// </summary>
        /// <param name="title">The label to display as a header</param>
        public static void DrawHeaderLabel(string title)
        {
            EditorGUILayout.LabelField(title, Styling.headerLabel);
        }

        internal static bool DrawHeader(string title, bool state)
        {
            var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

            var labelRect = backgroundRect;
            labelRect.xMin += 16f;
            labelRect.xMax -= 20f;

            var foldoutRect = backgroundRect;
            foldoutRect.y += 1f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            // Background rect should be full-width
            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;

            // Background
            EditorGUI.DrawRect(backgroundRect, Styling.headerBackground);

            // Title
            EditorGUI.LabelField(labelRect, GetContent(title), EditorStyles.boldLabel);

            // Foldout
            state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);

            var e = Event.current;
            if (e.type == EventType.MouseDown && backgroundRect.Contains(e.mousePosition) && e.button == 0)
            {
                state = !state;
                e.Use();
            }

            return state;
        }

        internal static bool DrawHeader(string title, SerializedProperty group, SerializedProperty activeField, PostProcessEffectSettings target, Action resetAction, Action removeAction)
        {
            Assert.IsNotNull(group);
            Assert.IsNotNull(activeField);
            Assert.IsNotNull(target);

            var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

            var labelRect = backgroundRect;
            labelRect.xMin += 32f;
            labelRect.xMax -= 20f;

            var foldoutRect = backgroundRect;
            foldoutRect.y += 1f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            var toggleRect = backgroundRect;
            toggleRect.x += 16f;
            toggleRect.y += 2f;
            toggleRect.width = 13f;
            toggleRect.height = 13f;

            var menuIcon = Styling.paneOptionsIcon;
#if UNITY_2019_3_OR_NEWER
            var menuRect = new Rect(labelRect.xMax + 4f, labelRect.y, menuIcon.width, menuIcon.height);
#else
            var menuRect = new Rect(labelRect.xMax + 4f, labelRect.y + 4f, menuIcon.width, menuIcon.height);
#endif

            // Background rect should be full-width
            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;

            // Background
            EditorGUI.DrawRect(backgroundRect, Styling.headerBackground);

            // Title
            using (new EditorGUI.DisabledScope(!activeField.boolValue))
                EditorGUI.LabelField(labelRect, GetContent(title), EditorStyles.boldLabel);

            // foldout
            group.serializedObject.Update();
            group.isExpanded = GUI.Toggle(foldoutRect, group.isExpanded, GUIContent.none, EditorStyles.foldout);
            group.serializedObject.ApplyModifiedProperties();

            // Active checkbox
            activeField.serializedObject.Update();
            activeField.boolValue = GUI.Toggle(toggleRect, activeField.boolValue, GUIContent.none, Styling.smallTickbox);
            activeField.serializedObject.ApplyModifiedProperties();

            // Dropdown menu icon
            GUI.DrawTexture(menuRect, menuIcon);

            // Handle events
            var e = Event.current;

            if (e.type == EventType.MouseDown)
            {
                if (menuRect.Contains(e.mousePosition))
                {
                    ShowHeaderContextMenu(new Vector2(menuRect.x, menuRect.yMax), target, resetAction, removeAction);
                    e.Use();
                }
                else if (labelRect.Contains(e.mousePosition))
                {
                    if (e.button == 0)
                        group.isExpanded = !group.isExpanded;
                    else
                        ShowHeaderContextMenu(e.mousePosition, target, resetAction, removeAction);

                    e.Use();
                }
            }

            return group.isExpanded;
        }

        static void ShowHeaderContextMenu(Vector2 position, PostProcessEffectSettings target, Action resetAction, Action removeAction)
        {
            Assert.IsNotNull(resetAction);
            Assert.IsNotNull(removeAction);

            var menu = new GenericMenu();
            menu.AddItem(GetContent("Reset"), false, () => resetAction());
            menu.AddItem(GetContent("Remove"), false, () => removeAction());
            menu.AddSeparator(string.Empty);
            menu.AddItem(GetContent("Copy Settings"), false, () => CopySettings(target));

            if (CanPaste(target))
                menu.AddItem(GetContent("Paste Settings"), false, () => PasteSettings(target));
            else
                menu.AddDisabledItem(GetContent("Paste Settings"));

            menu.DropDown(new Rect(position, Vector2.zero));
        }

        static void CopySettings(PostProcessEffectSettings target)
        {
            Assert.IsNotNull(target);

            if (s_ClipboardContent != null)
            {
                RuntimeUtilities.Destroy(s_ClipboardContent);
                s_ClipboardContent = null;
            }

            s_ClipboardContent = (PostProcessEffectSettings)ScriptableObject.CreateInstance(target.GetType());
            EditorUtility.CopySerializedIfDifferent(target, s_ClipboardContent);
        }

        static void PasteSettings(PostProcessEffectSettings target)
        {
            Assert.IsNotNull(target);
            Assert.IsNotNull(s_ClipboardContent);
            Assert.AreEqual(s_ClipboardContent.GetType(), target.GetType());

            Undo.RecordObject(target, "Paste Settings");
            EditorUtility.CopySerializedIfDifferent(s_ClipboardContent, target);
        }

        static bool CanPaste(PostProcessEffectSettings target)
        {
            return s_ClipboardContent != null
                && s_ClipboardContent.GetType() == target.GetType();
        }
    }
}
