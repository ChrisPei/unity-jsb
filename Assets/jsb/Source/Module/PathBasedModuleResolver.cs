using System;
using System.Collections.Generic;

namespace QuickJS.Module
{
    using Utils;
    using Native;

    public abstract class PathBasedModuleResolver : IModuleResolver
    {
        public PathBasedModuleResolver()
        {
        }

        // 验证模块名可接受
        protected abstract bool OnValidating(string module_id);

        protected abstract bool OnResolvingFile(IFileSystem fileSystem, IPathResolver pathResolver, string fileName, out string searchPath, out string resolvedFileName);

        public bool ResolveModule(IFileSystem fileSystem, IPathResolver pathResolver, string parent_module_id, string module_id, out string resolved_id)
        {
            if (OnValidating(module_id))
            {
                var parent_id = parent_module_id;
                var resolving = module_id;

                // 将相对目录展开
                if (module_id.StartsWith("./") || module_id.StartsWith("../") || module_id.Contains("/./") ||
                    module_id.Contains("/../"))
                {
                    // 显式相对路径直接从 parent 模块路径拼接
                    var parent_path = PathUtils.GetDirectoryName(parent_id);
                    try
                    {
                        resolving = PathUtils.ExtractPath(PathUtils.Combine(parent_path, module_id), '/');
                    }
                    catch
                    {
                        // 不能提升到源代码目录外面
                        throw new Exception(string.Format("invalid module path (out of sourceRoot): {0}", module_id));
                    }
                }

                string searchPath;
                if (OnResolvingFile(fileSystem, pathResolver, resolving, out searchPath, out resolved_id))
                {
                    return true;
                }
            }
            resolved_id = null;
            return false;
        }

        public abstract JSValue LoadModule(ScriptContext context, string resolved_id);
    }
}
