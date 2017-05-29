﻿// ----------------------------------------------------------------------------
// The MIT License
// LeopotamGroupLibrary https://github.com/Leopotam/LeopotamGroupLibraryUnity
// Copyright (c) 2012-2017 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using UnityEngine;

// ReSharper disable RedundantCast.0

namespace LeopotamGroup.Tweening {
    /// <summary>
    /// Tweening position.
    /// </summary>
    public class TweeningPosition : TweeningBase {
        /// <summary>
        /// Target transform. If null on start - current transform will be used.
        /// </summary>
        public Transform Target;

        /// <summary>
        /// Start value of position.
        /// </summary>
        public Vector3 StartValue = Vector3.zero;

        /// <summary>
        /// End value of position.
        /// </summary>
        public Vector3 EndValue = Vector3.zero;

        protected override void OnInit () {
            if (Target == null) {
                Target = transform;
            }
        }

        protected override void OnUpdateValue () {
            if ((object) Target != null) {
                Target.localPosition = Vector3.Lerp (StartValue, EndValue, Value);
            }
        }

        /// <summary>
        /// Begin tweening.
        /// </summary>
        /// <param name="start">Start position.</param>
        /// <param name="end">End position.</param>
        /// <param name="time">Time for tweening.</param>
        public TweeningPosition Begin (Vector3 start, Vector3 end, float time) {
            enabled = false;
            StartValue = start;
            EndValue = end;
            TweenTime = time;
            enabled = true;
            return this;
        }

        /// <summary>
        /// Begin tweening at specified GameObject.
        /// </summary>
        /// <param name="go">Holder of tweener.</param>
        /// <param name="start">Start position.</param>
        /// <param name="end">End position.</param>
        /// <param name="time">Time for tweening.</param>
        public static TweeningPosition Begin (GameObject go, Vector3 start, Vector3 end, float time) {
            var tweener = Get<TweeningPosition> (go);
            if (tweener != null) {
                tweener.Begin (start, end, time);
            }
            return tweener;
        }
    }
}