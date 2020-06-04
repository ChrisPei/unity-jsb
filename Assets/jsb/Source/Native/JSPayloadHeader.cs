﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using UnityEngine;

namespace QuickJS.Native
{
    using int32_t = Int32;
    using uint32_t = UInt32;

    public enum BridgeObjectType : int32_t
    {
        None = 0,
        TypeRef = 1,
        ObjectRef = 2,
        ValueType = 3,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JSPayloadHeader
    {
        public BridgeObjectType type_id;
        public int32_t value;

        public override int GetHashCode()
        {
            return ((int32_t)type_id & 0x3) & (value << 2);
        }

        public override bool Equals(object obj)
        {
            if (obj is JSPayloadHeader)
            {
                var t = (JSPayloadHeader)obj;
                return t.type_id == type_id && t.value == value;
            }

            return false;
        }
    }
}
