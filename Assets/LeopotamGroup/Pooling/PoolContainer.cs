﻿//-------------------------------------------------------
// LeopotamGroupLibrary for unity3d
// Copyright (c) 2012-2016 Leopotam <leopotam@gmail.com>
//-------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace LeopotamGroup.Pooling {
    /// <summary>
    /// Pool container. Supports spawning of named prefab from Resources folder.
    /// </summary>
    public sealed class PoolContainer : MonoBehaviour {
        [SerializeField]
        string _prefabPath = "UnknownPrefab";

        [SerializeField]
        Transform _itemsRoot;

        readonly Stack<PoolObject> _store = new Stack<PoolObject> (64);

        GameObject _cachedAsset;

        Vector3 _cachedScale;

        bool LoadPrefab () {
            _cachedAsset = Resources.Load<GameObject> (_prefabPath);
            if (_cachedAsset == null) {
                Debug.LogWarning ("Cant load asset " + _prefabPath);
                return false;
            }
            #if UNITY_EDITOR
            if (_cachedAsset.GetComponent <PoolObject> () != null) {
                Debug.LogWarning ("PoolObject cant be used on prefabs");
                _cachedAsset = null;
                UnityEditor.EditorApplication.isPaused = true;
                return false;
            }
            #endif

            _cachedScale = _cachedAsset.transform.localScale;

            return true;
        }

        /// <summary>
        /// Get new instance of prefab from pool.
        /// </summary>
        public PoolObject Get () {
            if ((System.Object) _cachedAsset == null) {
                if (!LoadPrefab ()) {
                    return null;
                }
            }

            PoolObject obj;
            if (_store.Count > 0) {
                obj = _store.Pop ();
            } else {
                var go = Instantiate<GameObject> (_cachedAsset);
                obj = go.AddComponent<PoolObject> ();
                obj.Pool = this;
                go.transform.SetParent (_itemsRoot, false);
                go.transform.localScale = _cachedScale;
            }
            obj.SetActive (false);
            return obj;
        }

        /// <summary>
        /// Recycle specified instance to pool.
        /// </summary>
        /// <param name="obj">Instance to recycle.</param>
        public void Recycle (PoolObject obj) {
            if ((System.Object) obj != null) {
                #if UNITY_EDITOR
                if (obj.Pool != this) {
                    Debug.LogWarning ("Invalid obj to recycle", obj);
                    return;
                }
                #endif
                obj.SetActive (false);
                if (!_store.Contains (obj)) {
                    _store.Push (obj);
                }
            }
        }

        /// <summary>
        /// Creates new pool container for specified prefab.
        /// </summary>
        /// <returns>Created pool container.</returns>
        /// <param name="prefabPath">Prefab path at Resources folder.</param>
        /// <param name="itemsRoot">Root for new items.</param>
        public static PoolContainer CreatePool (string prefabPath, Transform itemsRoot = null) {
            if (string.IsNullOrEmpty (prefabPath)) {
                return null;
            }
            var container =
                new GameObject (
                #if UNITY_EDITOR
                    "_POOL_" + prefabPath
                #endif
                ).AddComponent <PoolContainer> ();
            container._prefabPath = prefabPath;
            container._itemsRoot = itemsRoot;
            return container;
        }
    }
}