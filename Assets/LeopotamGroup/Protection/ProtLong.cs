﻿//-------------------------------------------------------
// LeopotamGroupLibrary for unity3d
// Copyright (c) 2012-2016 Leopotam <leopotam@gmail.com>
//-------------------------------------------------------

using System.Runtime.InteropServices;

namespace LeopotamGroup.Protection {
    [StructLayout (LayoutKind.Explicit)]
    public struct ProtLong {
        public long EncryptedValue {
            get {
                // Workaround for default struct constructor init.
                if (_conv == 0 && _encrypt == 0) {
                    _conv = XorMask;
                }
                return _encrypt;
            }
        }

        const ulong XorMask = 0xaaaaaaaaaaaaaaaa;

        [FieldOffset (0)]
        long _encrypt;

        [FieldOffset (0)]
        ulong _conv;

        public static implicit operator long (ProtLong v) {
            v._conv ^= XorMask;
            var f = v._encrypt;
            v._conv ^= XorMask;
            return f;
        }

        public static implicit operator ProtLong (long v) {
            var p = new ProtLong ();
            p._encrypt = v;
            p._conv ^= XorMask;
            return p;
        }
    }
}