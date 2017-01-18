﻿// ----------------------------------------------------------------------------
// The MIT License
// LeopotamGroupLibrary https://github.com/Leopotam/LeopotamGroupLibraryUnity
// Copyright (c) 2012-2017 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;

namespace LeopotamGroup.Mobile {
    /// <summary>
    /// Behaviour on android hardware "back"-button / "escape"-button press.
    /// </summary>
    public sealed class BackButtonBehaviour : MonoBehaviour {
        /// <summary>
        /// List of reactions on event.
        /// </summary>
        public UnityEvent OnBackPressed = null;

        void FixedUpdate () {
            if (Input.GetKeyDown (KeyCode.Escape)) {
                if (OnBackPressed != null) {
                    OnBackPressed.Invoke ();
                }
            }
        }
    }
}