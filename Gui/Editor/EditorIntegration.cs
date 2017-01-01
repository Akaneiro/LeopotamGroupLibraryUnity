﻿
// -------------------------------------------------------
// LeopotamGroupLibrary for unity3d
// Copyright (c) 2012-2017 Leopotam <leopotam@gmail.com>
// -------------------------------------------------------

using LeopotamGroup.Gui.Common.UnityEditors;
using LeopotamGroup.Gui.Common;
using LeopotamGroup.Gui.Layout;
using LeopotamGroup.Gui.Widgets;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEditor;
using UnityEngine;

namespace LeopotamGroup.Gui.UnityEditors {
    [InitializeOnLoad]
    public static class EditorIntegration {
        static Texture2D _whiteTexture;

        static readonly Color _panelColor = new Color (1f, 0.5f, 0.5f);

        static readonly Color _receiverColor = new Color (0.5f, 1f, 0.5f);

        static readonly Color _spriteColor = new Color (0.5f, 1f, 1f);

        static readonly Color _labelColor = new Color (1f, 1f, 0.5f);

        static EditorIntegration () {
            EditorApplication.hierarchyWindowItemOnGUI += OnDrawHierarchyItemIcon;
        }

        public static bool IsUndo () {
            return Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "UndoRedoPerformed";
        }

        static void OnDrawHierarchyItemIcon (int instanceID, Rect selectionRect) {
            try {
                GuiWidget w;
                GameObject obj;
                if (Event.current.type == EventType.DragPerform) {
                    foreach (var go in DragAndDrop.objectReferences) {
                        obj = go as GameObject;
                        if (obj != null && obj.activeSelf) {
                            foreach (var item in obj.GetComponentsInChildren<GuiWidget> ()) {
                                item.ResetPanel ();
                            }
                            if (Application.isPlaying) {
                                foreach (var item in obj.GetComponentsInChildren<GuiEventReceiver> ()) {
                                    item.ResetPanel ();
                                }
                            }
                        }
                    }
                    return;
                }

                obj = EditorUtility.InstanceIDToObject (instanceID) as GameObject;
                if (obj != null) {
                    var indent = 0;
                    var panel = obj.GetComponent<GuiPanel> ();
                    if (panel != null) {
                        DrawHierarchyLabel (selectionRect, indent++, "PNL: " + panel.Depth, Color.black, _panelColor);
                    }
                    var receiver = obj.GetComponent<GuiEventReceiver> ();
                    if (receiver != null) {
                        DrawHierarchyLabel (selectionRect, indent++, "RCV: " + receiver.Depth, Color.black, _receiverColor);
                    }
                    w = obj.GetComponent<GuiWidget> ();
                    if (w != null) {
                        if (w as GuiSprite) {
                            DrawHierarchyLabel (selectionRect, indent++, "SPR: " + w.Depth, Color.black, _spriteColor);
                        }
                        if (w is GuiLabel) {
                            DrawHierarchyLabel (selectionRect, indent++, "LBL: " + w.Depth, Color.black, _labelColor);
                        }
                    }
                }
            } catch (Exception ex) {
                Debug.LogWarning (ex);
            }
        }

        static void DrawHierarchyLabel (Rect rect, int indent, string text, Color textColor, Color backColor) {
            if (_whiteTexture == null) {
                _whiteTexture = new Texture2D (1, 1, TextureFormat.RGB24, false);
                _whiteTexture.hideFlags = HideFlags.HideAndDontSave;
                _whiteTexture.SetPixel (0, 0, Color.white);
                _whiteTexture.Apply (false);
            }
            rect.xMax -= 50 * indent;
            rect.xMin = rect.xMax - 50;
            var oldColor = GUI.color;
            GUI.color = backColor;
            GUI.DrawTexture (rect, _whiteTexture, ScaleMode.StretchToFill);
            GUI.color = textColor;
            EditorGUI.LabelField (rect, text);
            GUI.color = oldColor;
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/GuiSystem", false, 9999)]
        static void CreateGuiSystem () {
            GuiSystem.Instance.Validate ();
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Widgets/Sprite", false, 1)]
        static void CreateWidgetSprite () {
            SearchWindow.Open<GuiAtlas> ("Select atlas", "t:prefab", null, assetPath => {
                var spr = GuiControlFactory.CreateWidgetSprite ();
                Undo.RegisterCreatedObjectUndo (spr.gameObject, "leopotamgroup.gui.create-sprite");
                if (!string.IsNullOrEmpty (assetPath)) {
                    spr.SpriteAtlas = AssetDatabase.LoadAssetAtPath<GuiAtlas> (assetPath);
                    var sprNames = spr.SpriteAtlas.GetSpriteNames ();
                    spr.SpriteName = sprNames != null && sprNames.Length > 0 ? sprNames[0] : null;
                    spr.ResetSize ();
                }
                FixWidgetParent (spr);
                UpdateVisuals (spr);
            });
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Widgets/Label", false, 1)]
        static void CreateWidgetLabel () {
            SearchWindow.Open<Font> ("Select font", "t:font", null, assetPath => {
                var label = GuiControlFactory.CreateWidgetLabel ();
                Undo.RegisterCreatedObjectUndo (label.gameObject, "leopotamgroup.gui.create-label");
                label.Font = string.IsNullOrEmpty (assetPath) ?
                             Resources.GetBuiltinResource<Font> ("Arial.ttf") : AssetDatabase.LoadAssetAtPath<Font> (assetPath);
                label.Text = "Label";
                FixWidgetParent (label);
                UpdateVisuals (label);
            });
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Widgets/Button", false, 1)]
        static void CreateWidgetButton () {
            SearchWindow.Open<GuiAtlas> ("Select atlas", "t:prefab", null, assetPath => {
                var button = GuiControlFactory.CreateWidgetButton ();
                Undo.RegisterCreatedObjectUndo (button.gameObject, "leopotamgroup.gui.create-btn");
                FixWidgetParent (button);
                if (!string.IsNullOrEmpty (assetPath)) {
                    var spr = button.GetComponentInChildren<GuiSprite> ();
                    spr.SpriteAtlas = AssetDatabase.LoadAssetAtPath<GuiAtlas> (assetPath);
                    var sprNames = spr.SpriteAtlas.GetSpriteNames ();
                    spr.SpriteName = sprNames != null && sprNames.Length > 0 ? sprNames[0] : null;
                    spr.ResetSize ();
                    button.Width = spr.Width;
                    button.Height = spr.Height;
                    UpdateVisuals (spr);
                }
                UpdateVisuals (button);
            });
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Widgets/Button with label", false, 1)]
        static void CreateWidgetButtonWithLabel () {
            SearchWindow.Open<GuiAtlas> ("Select atlas", "t:prefab", null, sprAssetPath => {
                var button = GuiControlFactory.CreateWidgetButtonWithLabel ();
                Undo.RegisterCreatedObjectUndo (button.gameObject, "leopotamgroup.gui.create-btn");
                FixWidgetParent (button);
                if (!string.IsNullOrEmpty (sprAssetPath)) {
                    var spr = button.GetComponentInChildren<GuiSprite> ();
                    spr.SpriteAtlas = AssetDatabase.LoadAssetAtPath<GuiAtlas> (sprAssetPath);
                    var sprNames = spr.SpriteAtlas.GetSpriteNames ();
                    spr.SpriteName = sprNames != null && sprNames.Length > 0 ? sprNames[0] : null;
                    spr.ResetSize ();
                    button.Width = spr.Width;
                    button.Height = spr.Height;
                    UpdateVisuals (spr);
                }

                SearchWindow.Open<Font> ("Select font", "t:font", null, fontAssetPath => {
                    var label = button.GetComponentInChildren<GuiLabel> ();
                    label.Font = string.IsNullOrEmpty (fontAssetPath) ?
                                 Resources.GetBuiltinResource<Font> ("Arial.ttf") : AssetDatabase.LoadAssetAtPath<Font> (fontAssetPath);
                    label.Text = "Button";
                    UpdateVisuals (label);
                });

                UpdateVisuals (button);
            });
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Widgets/ProgressBar", false, 1)]
        static void CreateWidgetProgressBar () {
            SearchWindow.Open<GuiAtlas> ("Select atlas", "t:prefab", null, assetPath => {
                var slider = GuiControlFactory.CreateWidgetSlider ();
                slider.name = "ProgressBar";
                Undo.RegisterCreatedObjectUndo (slider.gameObject, "leopotamgroup.gui.create-progressbar");
                FixWidgetParent (slider);
                if (!string.IsNullOrEmpty (assetPath)) {
                    var atlas = AssetDatabase.LoadAssetAtPath<GuiAtlas> (assetPath);
                    var sprNames = atlas.GetSpriteNames ();
                    var name = sprNames != null && sprNames.Length > 0 ? sprNames[0] : null;
                    slider.Background.name = "Background";
                    slider.Foreground.name = "Foreground";
                    slider.Background.SpriteAtlas = atlas;
                    slider.Foreground.SpriteAtlas = atlas;
                    slider.Background.SpriteName = name;
                    slider.Foreground.SpriteName = name;
                    slider.Background.ResetSize ();
                    slider.Foreground.ResetSize ();
                }
                slider.Value = 0.5f;
                slider.UpdateVisuals ();
                UpdateVisuals (slider);
            });
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Widgets/Slider", false, 1)]
        static void CreateWidgetSlider () {
            SearchWindow.Open<GuiAtlas> ("Select atlas", "t:prefab", null, assetPath => {
                var slider = GuiControlFactory.CreateWidgetSlider (true, true);
                Undo.RegisterCreatedObjectUndo (slider.gameObject, "leopotamgroup.gui.create-slider");
                FixWidgetParent (slider);
                if (!string.IsNullOrEmpty (assetPath)) {
                    var atlas = AssetDatabase.LoadAssetAtPath<GuiAtlas> (assetPath);
                    var sprNames = atlas.GetSpriteNames ();
                    var name = sprNames != null && sprNames.Length > 0 ? sprNames[0] : null;
                    var thumb = slider.GetComponentInChildren<GuiBindPosition> ().GetComponent<GuiSprite> ();
                    slider.Background.name = "Background";
                    slider.Foreground.name = "Foreground";
                    thumb.name = "Thumb";
                    slider.Background.SpriteAtlas = atlas;
                    slider.Foreground.SpriteAtlas = atlas;
                    thumb.SpriteAtlas = atlas;
                    slider.Background.SpriteName = name;
                    slider.Foreground.SpriteName = name;
                    thumb.SpriteName = name;
                    slider.Background.ResetSize ();
                    slider.Foreground.ResetSize ();
                    thumb.ResetSize ();

                    var receiver = slider.Background.GetComponent<GuiEventReceiver> ();
                    receiver.Width = slider.Background.Width;
                    receiver.Height = slider.Background.Height;
                }
                slider.Value = 0.5f;
                slider.UpdateVisuals ();
                UpdateVisuals (slider);
            });
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Layout/Panel", false, 1)]
        static void CreateLayoutPanel () {
            FixWidgetParent (GuiControlFactory.CreateWidgetPanel ());
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Layout/BindPosition", false, 1)]
        static void CreateLayoutBindPosition () {
            var bind = GuiControlFactory.CreateLayoutBindPosition (Selection.activeGameObject);
            if (bind != null) {
                Undo.RegisterCreatedObjectUndo (bind.gameObject, "leopotamgroup.gui.create-bind-pos");
                FixWidgetParent (bind);
            }
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Layout/BindSize", false, 1)]
        static void CreateLayoutBindSize () {
            var bind = GuiControlFactory.CreateLayoutBindSize (Selection.activeGameObject);
            if (bind != null) {
                Undo.RegisterCreatedObjectUndo (bind.gameObject, "leopotamgroup.gui.create-bind-size");
                FixWidgetParent (bind);
            }
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Layout/BindPanelRange", false, 1)]
        static void CreateLayoutGuiBindPanelRange () {
            var bind = GuiControlFactory.CreateLayoutGuiBindPanelRange (Selection.activeGameObject);
            if (bind != null) {
                Undo.RegisterCreatedObjectUndo (bind.gameObject, "leopotamgroup.gui.create-bind-panel-range");
                FixWidgetParent (bind);
            }
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Layout/BindEventReceiverSize", false, 1)]
        static void CreateLayoutBindEventReceiverSize () {
            var bind = GuiControlFactory.CreateLayoutBindEventReceiverSize (Selection.activeGameObject);
            if (bind != null) {
                Undo.RegisterCreatedObjectUndo (bind.gameObject, "leopotamgroup.gui.create-bind-receiver-size");
                FixWidgetParent (bind);
            }
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Layout/ScrollView", false, 1)]
        static void CreateLayoutScrollView () {
            if (Selection.activeGameObject.GetComponent<GuiPanel> () != null) {
                if (!EditorUtility.DisplayDialog ("Warning",
                                                  "GuiScrollView should be created on child GameObject relative to GuiPanel. Do you want to continue?",
                                                  "Yes", "No")) {
                    return;
                }
            }
            var scroll = GuiControlFactory.CreateLayoutScrollView (Selection.activeGameObject);
            if (scroll != null) {
                Undo.RegisterCreatedObjectUndo (scroll.gameObject, "leopotamgroup.gui.create-scrollview");
                FixWidgetParent (scroll);
            }
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Layout/OverlayTransform", false, 1)]
        static void CreateLayoutOverlayTransform () {
            var bind = GuiControlFactory.CreateLayoutOverlayTransform (Selection.activeGameObject);
            if (bind != null) {
                Undo.RegisterCreatedObjectUndo (bind.gameObject, "leopotamgroup.gui.create-overlay-transform");
                FixWidgetParent (bind);
            }
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Interaction/Drag scroll view", false, 1)]
        static void CreateInteractionDragScrollView () {
            var scroller = GuiControlFactory.CreateInteractionDragScrollView (Selection.activeGameObject);
            if (scroller != null) {
                Undo.RegisterCreatedObjectUndo (scroller.gameObject, "leopotamgroup.gui.create-dragscrollview");
                FixWidgetParent (scroller);
            }
        }

        static void FixWidgetParent (MonoBehaviour widget) {
            if (Selection.activeGameObject != null && !AssetDatabase.Contains (Selection.activeGameObject)) {
                widget.transform.SetParent (Selection.activeGameObject.transform, false);
            }
            Selection.activeGameObject = widget.gameObject;
        }

        [MenuItem ("Assets/LeopotamGroup.Gui/Create new atlas asset", false, 1)]
        static void CreateAtlas () {
            string path = AssetDatabase.GetAssetPath (Selection.activeObject);

            string name;
            if (!string.IsNullOrEmpty (path) && AssetDatabase.Contains (Selection.activeObject)) {
                var isFolder = AssetDatabase.IsValidFolder (path);
                if (!isFolder) {
                    path = Path.GetDirectoryName (path);
                }
                name = Path.GetFileNameWithoutExtension (path);
            } else {
                path = "Assets";
                name = "GuiAtlas";
            }

            var asset = new GameObject ();
            asset.AddComponent<GuiAtlas> ();
            PrefabUtility.CreatePrefab (AssetDatabase.GenerateUniqueAssetPath (string.Format ("{0}/{1}.prefab", path, name)), asset);
            UnityEngine.Object.DestroyImmediate (asset);
            AssetDatabase.Refresh ();
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Layout/BindPosition", true)]
        static bool CanCreateLayoutBindPosition () {
            return Selection.activeGameObject != null && !AssetDatabase.Contains (Selection.activeGameObject);
        }

        [MenuItem ("GameObject/LeopotamGroup.Gui/Layout/BindSize", true)]
        static bool CanCreateLayoutBindSize () {
            return Selection.activeGameObject != null && !AssetDatabase.Contains (Selection.activeGameObject);
        }

        [MenuItem ("Assets/LeopotamGroup.Gui/Update atlas from folder...", true)]
        static bool CanBeAutoPacked () {
            var sel = Selection.activeGameObject;
            return sel != null && AssetDatabase.Contains (sel) && sel.GetComponent<GuiAtlas> () != null;
        }

        [MenuItem ("Assets/LeopotamGroup.Gui/Update atlas from folder...", false, 1)]
        static void UpdateAtlas () {
            var res = GuiAtlasInspector.BakeAtlas (Selection.activeGameObject.GetComponent<GuiAtlas> ());
            EditorUtility.DisplayDialog ("Atlas autopacker", res ?? "Completed", "Close");
        }

        public static void UpdateVisuals (UnityEngine.Object obj) {
            if (obj == null) {
                return;
            }
            var widget = obj as GuiWidget;
            if (widget != null) {
                widget.UpdateVisuals (GuiDirtyType.All);
            } else {
                var panel = obj as GuiPanel;
                if (panel != null) {
                    panel.UpdateVisuals ();
                } else {
//                    Debug.LogWarning ("Updating non-gui object", obj);
                    EditorUtility.SetDirty (obj);
                }
            }
            EditorApplication.RepaintHierarchyWindow ();
        }

        public static void SetLabelWidth (float width) {
            EditorGUIUtility.labelWidth = width;
        }
    }

    class SearchWindow : EditorWindow {
        Action<string> _cb;

        GUIContent[] _foundItems;

        string[] _foundItemPaths;

        int _defaultID;

        bool _needToClose;

        public static void Open<T> (string title, string filter, T active, Action<string> cb) where T : UnityEngine.Object {
            var win = GetWindow<SearchWindow> ();
            win.minSize = new Vector2 (800, 600);
            var pos = win.position;
            pos.position = (new Vector2 (Screen.currentResolution.width, Screen.currentResolution.height) - win.minSize) * 0.5f;
            win.position = pos;
            win.titleContent = new GUIContent (title);
            win._cb = cb;
            win._needToClose = false;
            win.Search<T> (filter, active);
            win.ShowUtility ();
        }

        void Update () {
            if (_needToClose) {
                Close ();
                return;
            }
        }

        void OnGUI () {
            if (_foundItems != null) {
                var id = GUILayout.SelectionGrid (_defaultID, _foundItems, 3);
                if (id != _defaultID) {
                    Finish (_foundItemPaths[id]);
                }
            }
        }

        void OnLostFocus () {
            Finish (null);
        }

        void Finish (string path) {
            _needToClose = true;

            if (_cb != null) {
                var cb = _cb;
                _cb = null;
                cb (path);
            }

            Repaint ();
        }

        void Search<T> (string filter, T active) where T : UnityEngine.Object {
            var prefabs = AssetDatabase.FindAssets (filter);
            var nameList = new List<GUIContent> ();
            var pathList = new List<string> ();
            _defaultID = -1;
            nameList.Add (new GUIContent ("None"));
            pathList.Add (string.Empty);
            foreach (var item in prefabs) {
                var path = AssetDatabase.GUIDToAssetPath (item);
                var asset = AssetDatabase.LoadAssetAtPath<T> (path);
                if (asset != null) {
                    nameList.Add (new GUIContent (asset.name, AssetDatabase.GetCachedIcon (path)));
                    pathList.Add (path);
                    if (asset == active) {
                        _defaultID = nameList.Count - 1;
                    }
                }
            }
            _foundItems = nameList.ToArray ();
            _foundItemPaths = pathList.ToArray ();
        }
    }
}