﻿using System.Reflection;

namespace Example.Editor
{
    using QuickJS.Unity;
    using UnityEngine;

    public class CustomBinding : AbstractBindingProcess
    {
        public override string GetBindingProcessName()
        {
            return "Example";
        }

        public override void OnPreExporting(BindingManager bindingManager)
        {
            // bindingManager.AddTypePrefixBlacklist("SyntaxTree.");

            // bindingManager.AddExportedType(typeof(System.Threading.Tasks.Task))
            //     .Rename("ATask")
            //     .SetMemberBlocked("IsCompletedSuccessfully");
            // bindingManager.AddExportedType(typeof(System.Threading.Tasks.Task<System.Net.Sockets.Socket>));
            // bindingManager.AddExportedType(typeof(System.Threading.Tasks.Task<int>));

            // bindingManager.AddExportedType(typeof(System.Net.Sockets.Socket));
            // bindingManager.AddExportedType(typeof(System.Net.Sockets.SocketFlags));
            // bindingManager.AddExportedType(typeof(System.Net.Sockets.AddressFamily));
            // bindingManager.AddExportedType(typeof(System.Net.IPAddress));
            // bindingManager.AddExportedType(typeof(System.Net.IPEndPoint));

            bindingManager.AddExportedType(typeof(WaitForSeconds), true);
            bindingManager.AddExportedType(typeof(WaitForEndOfFrame), true);
            bindingManager.AddExportedType(typeof(Time));
            bindingManager.AddExportedType(typeof(Random));
            bindingManager.AddExportedType(typeof(Input));
            bindingManager.AddExportedType(typeof(Debug));
            bindingManager.AddExportedType(typeof(Ray));
            bindingManager.AddExportedType(typeof(Rect));
            bindingManager.AddExportedType(typeof(RaycastHit));
            bindingManager.AddExportedType(typeof(Physics));
            bindingManager.AddExportedType(typeof(System.Net.IPHostEntry)).SystemRuntime();

            bindingManager.AddExportedType(typeof(System.DateTime)).SystemRuntime().EnableOperatorOverloading(false);
            bindingManager.AddExportedType(typeof(System.Enum)).SystemRuntime();
            bindingManager.AddExportedType(typeof(System.IO.FileInfo)).SystemRuntime()
                .SetMemberBlocked("GetAccessControl")
                .SetMemberBlocked("SetAccessControl");
            bindingManager.AddExportedType(typeof(System.IO.File)).SystemRuntime()
                .SetMemberBlocked("GetAccessControl")
                .SetMemberBlocked("SetAccessControl")
                .OnFilter<MethodInfo>(info => info.GetParameters().Length == 4); // not available in .net standard 2.0

            bindingManager.AddExportedType(typeof(TWrapper<int>));
            bindingManager.AddExportedType(typeof(TWrapper<Vector3>));
            bindingManager.AddExportedType(typeof(DisposableObject)).SetDisposable();

            #if CUSTOM_DEF_FOO
            bindingManager.AddExportedType(typeof(FOO)).AddRequiredDefines("CUSTOM_DEF_FOO", "UNITY_EDITOR");
            #endif
            #if CUSTOM_DEF_BAR
            bindingManager.AddExportedType(typeof(BAR)).AddRequiredDefines("CUSTOM_DEF_BAR");
            #endif
        }
    }
}