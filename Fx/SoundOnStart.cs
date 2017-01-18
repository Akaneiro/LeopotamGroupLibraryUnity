﻿// -------------------------------------------------------
// LeopotamGroupLibrary for unity3d
// Copyright (c) 2012-2017 Leopotam <leopotam@gmail.com>
// -------------------------------------------------------

using System.Collections;
using LeopotamGroup.Common;
using UnityEngine;

#pragma warning disable 649

namespace LeopotamGroup.Fx {
    /// <summary>
    /// Setup FX parameters on start.
    /// </summary>
    public sealed class SoundOnStart : MonoBehaviour {
        [SerializeField]
        AudioClip _sound;

        [SerializeField]
        SoundFxChannel _channel = SoundFxChannel.First;

        /// <summary>
        /// Should new FX force interrupts FX at channel or not.
        /// </summary>
        public bool IsInterrupt = false;

        IEnumerator Start () {
            yield return null;
            Singleton.Get<SoundManager> ().PlayFx (_sound, _channel, IsInterrupt);
        }
    }
}