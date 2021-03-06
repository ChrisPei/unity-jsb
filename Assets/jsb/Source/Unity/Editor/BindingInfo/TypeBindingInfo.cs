using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public class TypeBindingInfo
    {
        public readonly BindingManager bindingManager;
        public readonly Type type;
        public readonly TypeTransform transform;

        public TypeBindingFlags bindingFlags => transform.bindingFlags;

        /// <summary>
        /// 是否完全由 JS 托管 (JS对象释放时, CS对象即释放)
        /// </summary>
        public bool disposable => transform.disposable;

        /// <summary>
        /// 是否生成绑定代码
        /// </summary>
        public bool genBindingCode => (bindingFlags & TypeBindingFlags.BindingCode) != 0;

        /// <summary>
        /// 所需编译选项的列表
        /// </summary>
        public HashSet<string> requiredDefines => transform.requiredDefines;

        // 父类类型
        public Type super => type.BaseType;

        /// <summary>
        /// 跳过此类型的导出
        /// </summary>
        public bool omit => _omit;

        /// <summary>
        /// 等价于 type.Assembly
        /// </summary>
        public Assembly Assembly => type.Assembly;

        /// <summary>
        /// 等价于 type.FullName
        /// </summary>
        public string FullName => type.FullName;

        /// <summary>
        /// 等价于 type.IsEnum
        /// </summary>
        public bool IsEnum => type.IsEnum;

        private string _csBindingName;

        /// <summary>
        /// 绑定代码名
        /// </summary>
        public string csBindingName => _csBindingName;

        public List<OperatorBindingInfo> operators = new List<OperatorBindingInfo>();

        public Dictionary<string, MethodBindingInfo> methods = new Dictionary<string, MethodBindingInfo>();
        public Dictionary<string, MethodBindingInfo> staticMethods = new Dictionary<string, MethodBindingInfo>();

        public Dictionary<string, PropertyBindingInfo> properties = new Dictionary<string, PropertyBindingInfo>();
        public Dictionary<string, FieldBindingInfo> fields = new Dictionary<string, FieldBindingInfo>();
        public Dictionary<string, EventBindingInfo> events = new Dictionary<string, EventBindingInfo>();
        public Dictionary<string, DelegateBindingInfo> delegates = new Dictionary<string, DelegateBindingInfo>();
        public ConstructorBindingInfo constructors;

        private TSTypeNaming _tsTypeNaming;
        public TSTypeNaming tsTypeNaming => _tsTypeNaming;

        private bool _omit;

        public TypeBindingInfo(BindingManager bindingManager, Type type, TypeTransform typeTransform)
        {
            this.bindingManager = bindingManager;
            this.type = type;
            this.transform = typeTransform;
            this._omit = type.IsDefined(typeof(JSOmitAttribute));
        }

        public void Initialize()
        {
            _tsTypeNaming = bindingManager.GetTSTypeNaming(type, true);
            _csBindingName = bindingManager.prefs.typeBindingPrefix 
                + this.tsTypeNaming.jsFullName
                    .Replace('.', '_')
                    .Replace('+', '_')
                    .Replace('<', '_')
                    .Replace('>', '_')
                    .Replace(' ', '_')
                    .Replace(',', '_')
                    .Replace('=', '_');

            var module = this.bindingManager.GetExportedModule(this.tsTypeNaming.jsModule);

            module.Add(this);
        }

        // 将类型名转换成简单字符串 (比如用于文件名)
        public string GetFileName()
        {
            if (type.IsGenericType)
            {
                var selfname = string.IsNullOrEmpty(type.Namespace) ? "" : (type.Namespace + "_");
                selfname += type.Name.Substring(0, type.Name.IndexOf('`'));
                foreach (var gp in type.GetGenericArguments())
                {
                    selfname += "_" + gp.Name;
                }

                return selfname.Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace("`", "_");
            }

            var filename = type.FullName.Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace("`", "_");
            return filename;
        }

        public void AddEvent(EventInfo eventInfo)
        {
            try
            {
                bindingManager.CollectDelegate(eventInfo.EventHandlerType);
                events.Add(eventInfo.Name, new EventBindingInfo(this, eventInfo));
                bindingManager.Info("[AddEvent] {0}.{1}", type, eventInfo.Name);
            }
            catch (Exception exception)
            {
                bindingManager.Error("AddEvent failed {0} @ {1}: {2}", eventInfo, type, exception.Message);
            }
        }

        public void AddField(FieldInfo fieldInfo)
        {
            try
            {
                bindingManager.CollectDelegate(fieldInfo.FieldType);
                if (fieldInfo.FieldType.BaseType == typeof(MulticastDelegate))
                {
                    delegates.Add(fieldInfo.Name, new DelegateBindingInfo(this, fieldInfo));
                    bindingManager.Info("[AddField] As Delegate: {0}.{1}", type, fieldInfo.Name);
                }
                else
                {
                    fields.Add(fieldInfo.Name, new FieldBindingInfo(this, fieldInfo));
                    bindingManager.Info("[AddField] {0}.{1}", type, fieldInfo.Name);
                }
            }
            catch (Exception exception)
            {
                bindingManager.Error("AddField failed {0} @ {1}: {2}", fieldInfo, type, exception.Message);
            }
        }

        public void AddProperty(PropertyInfo propInfo)
        {
            try
            {
                bindingManager.CollectDelegate(propInfo.PropertyType);
                if (propInfo.PropertyType.BaseType == typeof(MulticastDelegate))
                {
                    delegates.Add(propInfo.Name, new DelegateBindingInfo(this, propInfo));
                    bindingManager.Info("[AddProperty] As Delegate: {0}.{1}", type, propInfo.Name);
                }
                else
                {
                    properties.Add(propInfo.Name, new PropertyBindingInfo(this, propInfo));
                    bindingManager.Info("[AddProperty] {0}.{1}", type, propInfo.Name);
                }
            }
            catch (Exception exception)
            {
                bindingManager.Error("AddProperty failed {0} @ {1}: {2}", propInfo, type, exception.Message);
            }
        }

        public bool IsSupportedOperators(MethodInfo methodInfo)
        {
            return methodInfo.IsSpecialName && methodInfo.Name.StartsWith("op_");
        }

        public bool IsOperatorOverloadingEnabled(MethodInfo methodInfo)
        {
            return bindingManager.prefs.enableOperatorOverloading && transform.enableOperatorOverloading && IsSupportedOperators(methodInfo);
        }

        public void AddMethod(MethodInfo methodInfo, bool asExtensionAnyway)
        {
            if (this.transform.IsBlocked(methodInfo))
            {
                bindingManager.Info("skip blocked method: {0}", methodInfo.Name);
                return;
            }

            // if (type.IsConstructedGenericType)
            // {
            //     var gTransform = bindingManager.GetTypeTransform(type.GetGenericTypeDefinition());
            //     if (gTransform != null && gTransform.IsBlocked(methodInfo.??))
            //     {
            //         bindingManager.Info("skip blocked method in generic definition: {0}", methodInfo.Name);
            //         return;
            //     }
            // }

            var isExtension = asExtensionAnyway || BindingManager.IsExtensionMethod(methodInfo);
            var isStatic = methodInfo.IsStatic && !isExtension;

            if (isStatic && type.IsGenericTypeDefinition)
            {
                bindingManager.Info("skip static method in generic type definition: {0}", methodInfo.Name);
                return;
            }

            var methodCSName = methodInfo.Name;
            var methodJSName = this.bindingManager.GetNamingAttribute(this.transform, methodInfo);
            if (IsOperatorOverloadingEnabled(methodInfo))
            {
                var parameters = methodInfo.GetParameters();
                var declaringType = methodInfo.DeclaringType;
                OperatorBindingInfo operatorBindingInfo = null;
                switch (methodCSName)
                {
                    case "op_LessThan":
                        if (parameters.Length == 2)
                        {
                            if (parameters[0].ParameterType == declaringType && parameters[1].ParameterType == declaringType)
                            {
                                operatorBindingInfo = new OperatorBindingInfo(bindingManager, methodInfo, isExtension, isStatic, methodCSName, "<", "<", 2);
                            }
                        }
                        break;
                    case "op_Addition":
                        if (parameters.Length == 2)
                        {
                            if (parameters[0].ParameterType == declaringType && parameters[1].ParameterType == declaringType)
                            {
                                operatorBindingInfo = new OperatorBindingInfo(bindingManager, methodInfo, isExtension, isStatic, methodCSName, "+", "+", 2);
                            }
                        }
                        break;
                    case "op_Subtraction":
                        if (parameters.Length == 2)
                        {
                            if (parameters[0].ParameterType == declaringType && parameters[1].ParameterType == declaringType)
                            {
                                operatorBindingInfo = new OperatorBindingInfo(bindingManager, methodInfo, isExtension, isStatic, methodCSName, "-", "-", 2);
                            }
                        }
                        break;
                    case "op_Equality":
                        if (parameters.Length == 2)
                        {
                            if (parameters[0].ParameterType == declaringType && parameters[1].ParameterType == declaringType)
                            {
                                operatorBindingInfo = new OperatorBindingInfo(bindingManager, methodInfo, isExtension, isStatic, methodCSName, "==", "==", 2);
                            }
                        }
                        break;
                    case "op_Multiply":
                        if (parameters.Length == 2)
                        {
                            var op0 = bindingManager.GetExportedType(parameters[0].ParameterType);
                            var op1 = bindingManager.GetExportedType(parameters[1].ParameterType);
                            if (op0 != null && op1 != null)
                            {
                                var bindingName = methodCSName + "_" + op0.csBindingName + "_" + op1.csBindingName;
                                operatorBindingInfo = new OperatorBindingInfo(bindingManager, methodInfo, isExtension, isStatic, bindingName, "*", "*", 2);
                            }
                        }
                        break;
                    case "op_Division":
                        if (parameters.Length == 2)
                        {
                            var op0 = bindingManager.GetExportedType(parameters[0].ParameterType);
                            var op1 = bindingManager.GetExportedType(parameters[1].ParameterType);
                            if (op0 != null && op1 != null)
                            {
                                var bindingName = methodCSName + "_" + op0.csBindingName + "_" + op1.csBindingName;
                                operatorBindingInfo = new OperatorBindingInfo(bindingManager, methodInfo, isExtension, isStatic, bindingName, "/", "/", 2);
                            }
                        }
                        break;
                    case "op_UnaryNegation":
                        {
                            operatorBindingInfo = new OperatorBindingInfo(bindingManager, methodInfo, isExtension, isStatic, methodCSName, "neg", "-", 1);
                        }
                        break;
                }

                if (operatorBindingInfo != null)
                {
                    operators.Add(operatorBindingInfo);
                    CollectDelegate(methodInfo);
                    bindingManager.Info("[AddOperator] {0}.{1}", type, methodInfo);
                    return;
                }

                // fallback to normal method binding
            }

            var group = isStatic ? staticMethods : methods;
            MethodBindingInfo methodBindingInfo;
            if (!group.TryGetValue(methodCSName, out methodBindingInfo))
            {
                methodBindingInfo = new MethodBindingInfo(bindingManager, isStatic, methodCSName, methodJSName);
                group.Add(methodCSName, methodBindingInfo);
            }

            if (!methodBindingInfo.Add(methodInfo, isExtension))
            {
                bindingManager.Info("fail to add method: {0}", methodInfo.Name);
                return;
            }

            CollectDelegate(methodInfo);
            bindingManager.Info("[AddMethod] {0}.{1}", type, methodInfo);
        }

        private void CollectDelegate(MethodBase method)
        {
            var parameters = method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                bindingManager.CollectDelegate(parameters[i].ParameterType);
            }
        }

        public void AddConstructor(ConstructorInfo constructorInfo)
        {
            if (this.transform.IsBlocked(constructorInfo))
            {
                bindingManager.Info("skip blocked constructor: {0}", constructorInfo.Name);
                return;
            }

            if (!constructors.Add(constructorInfo, false))
            {
                bindingManager.Info("add constructor failed: {0}", constructorInfo.Name);
                return;
            }
            CollectDelegate(constructorInfo);
            this.bindingManager.Info("[AddConstructor] {0}.{1}", type, constructorInfo);
        }

        // 收集所有 字段,属性,方法
        public void Collect()
        {
            this.constructors = new ConstructorBindingInfo(bindingManager, type);

            var bindingFlags = Binding.DynamicType.PublicFlags;
            var fields = type.GetFields(bindingFlags);
            foreach (var field in fields)
            {
                if (field.IsSpecialName || field.Name.StartsWith("_JSFIX_"))
                {
                    bindingManager.Info("skip special field: {0}", field.Name);
                    continue;
                }

                if (field.IsStatic && type.IsGenericTypeDefinition)
                {
                    bindingManager.Info("skip static field in generic type definition: {0}", field.Name);
                    return;
                }

                if (field.FieldType.IsPointer)
                {
                    bindingManager.Info("skip pointer field: {0}", field.Name);
                    continue;
                }

                if (field.IsDefined(typeof(JSOmitAttribute), false))
                {
                    bindingManager.Info("skip omitted field: {0}", field.Name);
                    continue;
                }

                if (field.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    bindingManager.Info("skip obsolete field: {0}", field.Name);
                    continue;
                }

                if (transform.IsMemberBlocked(field.Name))
                {
                    bindingManager.Info("skip blocked field: {0}", field.Name);
                    continue;
                }

                if (transform.Filter(field))
                {
                    bindingManager.Info("skip filtered field: {0}", field.Name);
                    continue;
                }

                AddField(field);
            }

            var events = type.GetEvents(bindingFlags);
            foreach (var evt in events)
            {
                if (evt.IsSpecialName)
                {
                    bindingManager.Info("skip special event: {0}", evt.Name);
                    continue;
                }

                if ((evt.AddMethod?.IsStatic == true || evt.RemoveMethod?.IsStatic == true) && type.IsGenericTypeDefinition)
                {
                    bindingManager.Info("skip static event in generic type definition: {0}", evt.Name);
                    return;
                }

                if (evt.EventHandlerType.IsPointer)
                {
                    bindingManager.Info("skip pointer event: {0}", evt.Name);
                    continue;
                }

                if (evt.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    bindingManager.Info("skip obsolete event: {0}", evt.Name);
                    continue;
                }

                if (evt.IsDefined(typeof(JSOmitAttribute), false))
                {
                    bindingManager.Info("skip omitted event: {0}", evt.Name);
                    continue;
                }

                if (transform.IsMemberBlocked(evt.Name))
                {
                    bindingManager.Info("skip blocked event: {0}", evt.Name);
                    continue;
                }

                if (transform.Filter(evt))
                {
                    bindingManager.Info("skip filtered event: {0}", evt.Name);
                    continue;
                }

                AddEvent(evt);
            }

            var properties = type.GetProperties(bindingFlags);
            foreach (var property in properties)
            {
                if (property.IsSpecialName)
                {
                    bindingManager.Info("skip special property: {0}", property.Name);
                    continue;
                }

                if (property.PropertyType.IsPointer)
                {
                    bindingManager.Info("skip pointer property: {0}", property.Name);
                    continue;
                }

                if ((property.GetMethod?.IsStatic == true || property.SetMethod?.IsStatic == true) && type.IsGenericTypeDefinition)
                {
                    bindingManager.Info("skip static property in generic type definition: {0}", property.Name);
                    return;
                }

                if (property.IsDefined(typeof(JSOmitAttribute), false))
                {
                    bindingManager.Info("skip omitted property: {0}", property.Name);
                    continue;
                }

                if (property.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    bindingManager.Info("skip obsolete property: {0}", property.Name);
                    continue;
                }

                if (transform.IsMemberBlocked(property.Name))
                {
                    bindingManager.Info("skip blocked property: {0}", property.Name);
                    continue;
                }

                if (transform.Filter(property))
                {
                    bindingManager.Info("skip filtered property: {0}", property.Name);
                    continue;
                }

                //NOTE: 索引访问
                if (property.Name == "Item")
                {
                    if (property.CanRead && property.GetMethod != null && property.GetMethod.IsPublic)
                    {
                        if (BindingManager.IsUnsupported(property.GetMethod))
                        {
                            bindingManager.Info("skip unsupported get-method: {0}", property.GetMethod);
                            continue;
                        }

                        AddMethod(property.GetMethod, false);
                    }

                    if (property.CanWrite && property.SetMethod != null && property.SetMethod.IsPublic)
                    {
                        if (BindingManager.IsUnsupported(property.SetMethod))
                        {
                            bindingManager.Info("skip unsupported set-method: {0}", property.SetMethod);
                            continue;
                        }

                        AddMethod(property.SetMethod, false);
                    }

                    // bindingManager.Info("skip indexer property: {0}", property.Name);
                    continue;
                }

                AddProperty(property);
            }

            if (!type.IsAbstract)
            {
                var constructors = type.GetConstructors();
                foreach (var constructor in constructors)
                {
                    if (constructor.IsDefined(typeof(JSOmitAttribute), false))
                    {
                        bindingManager.Info("skip omitted constructor: {0}", constructor);
                        continue;
                    }

                    if (constructor.IsDefined(typeof(ObsoleteAttribute), false))
                    {
                        bindingManager.Info("skip obsolete constructor: {0}", constructor);
                        continue;
                    }

                    if (BindingManager.ContainsPointer(constructor))
                    {
                        bindingManager.Info("skip pointer-param constructor: {0}", constructor);
                        continue;
                    }

                    if (BindingManager.ContainsByRefParameters(constructor))
                    {
                        bindingManager.Info("skip byref-param constructor: {0}", constructor);
                        continue;
                    }

                    if (transform.Filter(constructor))
                    {
                        bindingManager.Info("skip filtered constructor: {0}", constructor.Name);
                        continue;
                    }

                    AddConstructor(constructor);
                }
            }

            CollectMethods(type.GetMethods(bindingFlags), false);
            CollectMethods(bindingManager.GetTypeTransform(type).extensionMethods, true);
            CollectMethods(bindingManager.GetTypeTransform(type).staticMethods, false);
        }

        private void CollectMethods(IEnumerable<MethodInfo> methods, bool asExtensionAnyway)
        {
            foreach (var method in methods)
            {
                if (BindingManager.IsGenericMethod(method))
                {
                    bindingManager.Info("skip generic method: {0}", method);
                    continue;
                }

                if (BindingManager.ContainsPointer(method))
                {
                    bindingManager.Info("skip unsafe (pointer) method: {0}", method);
                    continue;
                }

                if (method.IsSpecialName)
                {
                    if (!IsSupportedOperators(method))
                    {
                        bindingManager.Info("skip special method: {0}", method);
                        continue;
                    }
                }

                if (method.IsDefined(typeof(JSOmitAttribute), false))
                {
                    bindingManager.Info("skip omitted method: {0}", method);
                    continue;
                }

                if (method.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    bindingManager.Info("skip obsolete method: {0}", method);
                    continue;
                }

                if (transform.IsMemberBlocked(method.Name))
                {
                    bindingManager.Info("skip blocked method: {0}", method.Name);
                    continue;
                }

                if (transform.Filter(method))
                {
                    bindingManager.Info("skip filtered method: {0}", method.Name);
                    continue;
                }

                if (asExtensionAnyway || BindingManager.IsExtensionMethod(method))
                {
                    var targetType = method.GetParameters()[0].ParameterType;
                    var targetInfo = bindingManager.GetExportedType(targetType);
                    if (targetInfo != null)
                    {
                        targetInfo.AddMethod(method, true);
                        continue;
                    }
                }

                AddMethod(method, false);
            }
        }
    }
}