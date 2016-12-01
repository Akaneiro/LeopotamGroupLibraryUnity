﻿//-------------------------------------------------------
// LeopotamGroupLibrary for unity3d
// Copyright (c) 2012-2016 Leopotam <leopotam@gmail.com>
//-------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using LeopotamGroup.Gui.Common;
using LeopotamGroup.Gui.Layout;
using LeopotamGroup.Gui.Widgets;
using UnityEditor;
using UnityEngine;

namespace LeopotamGroup.Gui.Common.UnityEditors {
    [CustomEditor (typeof (GuiAtlas))]
    sealed class GuiAtlasInspector : Editor {
        SerializedProperty _colorTexProperty;

        SerializedProperty _alphaTexProperty;

        void OnEnable () {
            _colorTexProperty = serializedObject.FindProperty ("ColorTexture");
            _alphaTexProperty = serializedObject.FindProperty ("AlphaTexture");
        }

        public override void OnInspectorGUI () {
            EditorGUILayout.HelpBox ("Use context menu at atlas asset for options.\n\n" +
                "Add suffix .slice_A_B_C_D to texture asset name for init slice borders for left (A), top (B), right (C) and bottom (D) sides.\n\n" +
                "All coords - in pixels from each side!\n\n" +
                "For example: button.sliced_10_5_10_5", MessageType.Info);

            var atlas = target as GuiAtlas;

            if (atlas.AlphaTexture == null) {
                EditorGUILayout.Separator ();
                EditorGUILayout.HelpBox ("Without AlphaTexture sprites will be processed as opaque geometry - useful for backgrounds.\n\n" +
                    "Any clipping will not work!", MessageType.Warning);
            }

            EditorGUILayout.Separator ();

            serializedObject.Update ();
            EditorGUILayout.PropertyField (_colorTexProperty);
            EditorGUILayout.PropertyField (_alphaTexProperty);

            if (_alphaTexProperty.objectReferenceValue != null) {
                if (GUILayout.Button ("Remove alpha channel")) {
                    if (EditorUtility.DisplayDialog ("Warning", "This will turn atlas to opaque rendering, are you sure?", "Yes", "No")) {
                        var obj = _alphaTexProperty.objectReferenceValue;
                        _alphaTexProperty.objectReferenceValue = null;
                        serializedObject.ApplyModifiedPropertiesWithoutUndo ();
                        AssetDatabase.DeleteAsset (AssetDatabase.GetAssetPath (obj));
                    }
                }
            }

            EditorGUILayout.Separator ();

            EditorGUILayout.LabelField ("Sprites list", EditorStyles.boldLabel);
            foreach (var item in atlas.Sprites) {
                GUILayout.Box (item.Name, GUILayout.ExpandWidth (true));
            }

            if (serializedObject.ApplyModifiedProperties () || (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "UndoRedoPerformed")) {
                foreach (var panel in FindObjectsOfType <GuiPanel>()) {
                    panel.ResetMaterialCache ();
                }
                foreach (var vis in FindObjectsOfType <GuiWidget>()) {
                    vis.SetDirty (GuiDirtyType.Geometry);
                    EditorUtility.SetDirty (vis);
                }
            }
        }

        static readonly Regex _slicedMask = new Regex (".sliced_(?<left>\\d+)_(?<top>\\d+)_(?<right>\\d+)_(?<bottom>\\d+)");

        public static string BakeAtlas (GuiAtlas atlas) {
            var fileName = EditorUtility.OpenFolderPanel ("Select sprites folder", Application.dataPath, string.Empty);
            return BakeAtlas (atlas, fileName);
        }

        public static string BakeAtlas (GuiAtlas atlas, string fileName) {
            if (!AssetDatabase.Contains (atlas)) {
                return "Atlas should be saved as asset";
            }
            if (string.IsNullOrEmpty (fileName)) {
                return "Sprite folder not selected";
            }

            // cleanup.
            try {
                if (atlas.ColorTexture != null && AssetDatabase.Contains (atlas.ColorTexture)) {
                    var t = atlas.ColorTexture;
                    atlas.ColorTexture = null;
                    AssetDatabase.DeleteAsset (AssetDatabase.GetAssetPath (t));
                }
            } catch {
            }
            try {
                if (atlas.AlphaTexture != null && AssetDatabase.Contains (atlas.AlphaTexture)) {
                    var t = atlas.AlphaTexture;
                    atlas.AlphaTexture = null;
                    AssetDatabase.DeleteAsset (AssetDatabase.GetAssetPath (t));
                }
            } catch {
            }

            AssetDatabase.Refresh ();

            var sprites = new List<Texture2D> ();

            foreach (var spriteFileName in Directory.GetFiles (fileName, "*.png", SearchOption.TopDirectoryOnly)) {
                var spriteTex = new Texture2D (2, 2);
                spriteTex.LoadImage (File.ReadAllBytes (spriteFileName));
                spriteTex.Apply ();
                spriteTex.name = Path.GetFileNameWithoutExtension (spriteFileName);
                sprites.Add (spriteTex);
            }

            var atlasTex = new Texture2D (2, 2, TextureFormat.ARGB32, false);
            atlasTex.hideFlags = HideFlags.HideAndDontSave;

            Rect[] rects;

            if (sprites.Count == 1) {
                // special case, one texture
                rects = new Rect[] { new Rect (0, 0, 1f, 1f) };
                atlasTex.LoadImage (sprites[0].EncodeToPNG ());
                atlasTex.Apply ();
            } else {
                rects = atlasTex.PackTextures (sprites.ToArray (), 2, 2048);
            }

            var tex = new Texture2D (atlasTex.width, atlasTex.height, TextureFormat.RGB24, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            var srcColors = atlasTex.GetPixels32 ();
            DestroyImmediate (atlasTex);
            atlasTex = null;

            var dstAlphas = new Color32[srcColors.Length];
            Color32 c;
            for (int i = 0; i < srcColors.Length; i++) {
                c = srcColors[i];
                dstAlphas[i] = new Color32 (c.a, c.a, c.a, 255);
            }
            tex.SetPixels32 (srcColors);
            var colorData = tex.EncodeToPNG ();
            tex.SetPixels32 (dstAlphas);
            var alphaData = tex.EncodeToPNG ();
            DestroyImmediate (tex);
            tex = null;

            try {
                var srcAtlasPath = AssetDatabase.GetAssetPath (atlas);
                EditorUtility.DisplayProgressBar ("Save and import atlas data", "Color data processing...", 1f);
                var atlasPath = Path.ChangeExtension (srcAtlasPath, "color.png");
                File.WriteAllBytes (Path.Combine (Path.Combine (Application.dataPath, ".."), atlasPath), colorData);

                EditorUtility.DisplayProgressBar ("Save and import atlas data", "Alpha data processing...", 1f);
                var alphaAtlasPath = Path.ChangeExtension (srcAtlasPath, "alpha.png");
                File.WriteAllBytes (Path.Combine (Path.Combine (Application.dataPath, ".."), alphaAtlasPath), alphaData);

                EditorUtility.DisplayProgressBar ("Save and import atlas data", "Import processed data...", 1f);
                AssetDatabase.Refresh ();
                atlasTex = FixAtlasImport (atlasPath);
                atlas.ColorTexture = atlasTex;
                atlas.AlphaTexture = FixAtlasImport (alphaAtlasPath);

                var atlasSprites = new List<GuiSpriteData> (sprites.Count);
                float atlasWidth = atlasTex.width;
                float atlasHeight = atlasTex.height;
                for (int i = 0, iMax = sprites.Count; i < iMax; i++) {
                    var sprData = new GuiSpriteData ();
                    var sprName = sprites[i].name;
                    // slicing
                    var match = _slicedMask.Match (sprName.ToLowerInvariant ());
                    if (match.Success) {
                        sprName = sprName.Replace (match.Value, string.Empty);
                        sprData.BorderL = int.Parse (match.Groups["left"].Value) / atlasWidth;
                        sprData.BorderT = int.Parse (match.Groups["top"].Value) / atlasHeight;
                        sprData.BorderR = int.Parse (match.Groups["right"].Value) / atlasWidth;
                        sprData.BorderB = int.Parse (match.Groups["bottom"].Value) / atlasHeight;
                    } else {
                        sprData.BorderL = 0;
                        sprData.BorderT = 0;
                        sprData.BorderR = 0;
                        sprData.BorderB = 0;
                    }

                    sprData.Name = sprName;
                    sprData.CornerX = rects[i].x;
                    sprData.CornerY = rects[i].y;
                    sprData.CornerW = rects[i].width;
                    sprData.CornerH = rects[i].height;
                    atlasSprites.Add (sprData);
                }

                atlas.Sprites = atlasSprites.ToArray ();

                EditorUtility.SetDirty (atlas);

                AssetDatabase.SaveAssets ();
                AssetDatabase.Refresh ();

                atlas.ResetCache ();

                foreach (var panel in FindObjectsOfType <GuiPanel>()) {
                    panel.ResetMaterialCache ();
                }
                foreach (var vis in FindObjectsOfType <GuiWidget>()) {
                    vis.SetDirty (GuiDirtyType.Geometry);
                    EditorUtility.SetDirty (vis);
                }
            } finally {
                EditorUtility.ClearProgressBar ();

                for (var i = sprites.Count - 1; i >= 0; i--) {
                    DestroyImmediate (sprites[i]);
                    sprites.RemoveAt (i);
                }
            }

            return null;
        }

        static Texture2D FixAtlasImport (string atlasPath) {
            AssetDatabase.ImportAsset (atlasPath, ImportAssetOptions.ForceUpdate);
            var atlasTex = AssetDatabase.LoadAssetAtPath<Texture2D> (atlasPath);
            var splatSettings = AssetImporter.GetAtPath (atlasPath) as TextureImporter;
            splatSettings.wrapMode = TextureWrapMode.Clamp;
            splatSettings.anisoLevel = 2;
            splatSettings.filterMode = FilterMode.Bilinear;
            splatSettings.alphaIsTransparency = true;
            splatSettings.textureCompression = TextureImporterCompression.Compressed;
            splatSettings.compressionQuality = 100;
            splatSettings.mipmapEnabled = false;
            splatSettings.textureType = TextureImporterType.Default;
            return atlasTex;
        }
    }
}