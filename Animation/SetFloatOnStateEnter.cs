﻿// ----------------------------------------------------------------------------
// The MIT License
// LeopotamGroupLibrary https://github.com/Leopotam/LeopotamGroupLibraryUnity
// Copyright (c) 2012-2017 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using UnityEngine;

namespace LeopotamGroup.Animation {
    /// <summary>
    /// Set Animator float parameter to new state on node enter.
    /// </summary>
    public sealed class SetFloatOnStateEnter : StateMachineBehaviour {
        [SerializeField]
        string _floatName;

        [SerializeField]
        float _floatValue;

        int _fieldHash = -1;

        public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter (animator, stateInfo, layerIndex);
            if (_fieldHash == -1) {
#if UNITY_EDITOR
                if (string.IsNullOrEmpty (_floatName)) {
                    Debug.LogWarning ("Float field name is empty", animator);
                    return;
                }
#endif
                _fieldHash = Animator.StringToHash (_floatName);
            }
            animator.SetFloat (_fieldHash, _floatValue);
        }
    }
}