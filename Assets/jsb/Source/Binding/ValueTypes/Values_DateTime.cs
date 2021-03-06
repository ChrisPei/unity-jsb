using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using UnityEngine;
    using Native;

    public partial class Values
    {
        public static unsafe JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, DateTime o, int type_id, bool disposable)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(long));
            if (!JSApi.JS_IsException(val))
            {
                JSApi.jsb_set_bytes(val, o.Ticks);
            }
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool js_rebind_this(JSContext ctx, JSValue this_obj, ref DateTime o)
        {
            return JSApi.jsb_set_bytes(this_obj, o.Ticks) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe JSValue js_push_structvalue(JSContext ctx, ref DateTime o)
        {
            var proto = FindPrototypeOf<DateTime>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(long));
            JSApi.jsb_set_bytes(val, o.Ticks);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool js_get_structvalue(JSContext ctx, JSValue val, out DateTime o)
        {
            long p;
            var ret = JSApi.jsb_get_bytes(val, out p);
            o = new DateTime(p);
            return ret != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool js_get_structvalue(JSContext ctx, JSValue val, out DateTime? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            long p;
            var ret = JSApi.jsb_get_bytes(val, out p);
            o = new DateTime(p);
            return ret != 0;
        }
    }
}
