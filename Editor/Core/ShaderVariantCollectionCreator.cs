using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UTJ.FrameDebugSave
{
    public class ShaderVariantCollectionCreator
    {
        [System.Flags]
        public enum EFlag : int
        {
            None = 0x00,
            Assets = 0x01,
            Packages = 0x02,
            BuiltIn = 0x04,
        }

        private class ShaderDrawInfo
        {
            public string keywords;
            public string passLightMode;

            public ShaderDrawInfo(string kw, string lm)
            {
                this.keywords = kw;
                this.passLightMode = lm;
            }

            public override int GetHashCode()
            {
                return keywords.GetHashCode() + passLightMode.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                var target = obj as ShaderDrawInfo;
                if (target == null) { return false; }
                return (this.keywords == target.keywords && this.passLightMode == target.passLightMode);
            }
        }

        private class ShaderVariantInfo
        {
            private string shader;
            private HashSet<ShaderDrawInfo> shaderDrawInfo;

            public ShaderVariantInfo(string sh)
            {
                this.shader = sh;
                this.shaderDrawInfo = new HashSet<ShaderDrawInfo>();
            }
            public void AddKeywords(string keywords, string passLightMode)
            {
                keywords = keywords.Trim();
                var info = new ShaderDrawInfo(keywords, passLightMode);
                if (!shaderDrawInfo.Contains(info))
                {
                    shaderDrawInfo.Add(info);
                }
            }

            public List<ShaderVariantCollection.ShaderVariant> CreateShaderVariantList(EFlag flags)
            {
                List<ShaderVariantCollection.ShaderVariant> shaderVariants = new List<ShaderVariantCollection.ShaderVariant>();
                var shaderInstance = Shader.Find(this.shader);

                if (shaderInstance == null)
                {
                    Debug.LogError("Shader not found " + this.shader);
                    return shaderVariants;
                }
                if (!ShouldExecute(shaderInstance, flags))
                {
                    return shaderVariants;
                }

                foreach (var info in shaderDrawInfo)
                {
                    string[] keywordArray;
                    if (string.IsNullOrEmpty(info.keywords))
                    {
                        keywordArray = new string[] { "" };
                    }
                    else
                    {
                        keywordArray = info.keywords.Split(' ');
                    }
                    try
                    {
                        var passType = GetPassType(info.passLightMode);
                        //                        Debug.Log(info.passLightMode + "->" + passType);
                        ShaderVariantCollection.ShaderVariant variant = new ShaderVariantCollection.ShaderVariant(shaderInstance, passType, keywordArray);
                        shaderVariants.Add(variant);
                    } catch (System.Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
                return shaderVariants;
            }

            private bool ShouldExecute(Shader shader, EFlag flags)
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(shader).ToLower();
                bool isBuiltIn = false;
                bool isPackage= false;

                isBuiltIn = IsBuiltInShader(path);
                if (flags.HasFlag(EFlag.BuiltIn) && isBuiltIn)
                {
                    return true;
                }
                isPackage = IsPackageShader(path);
                if (flags.HasFlag(EFlag.Packages) && isPackage)
                {
                    return true;
                }
                if( flags.HasFlag(EFlag.Assets) && !isBuiltIn && !isPackage)
                {
                    return true;
                }
                return false;
            }

            private static bool IsBuiltInShader(string lowerPath)
            {
                return (lowerPath == "resources/unity_builtin_extra") || (lowerPath == "resources/unity_builtin");
            }
            private static bool IsPackageShader(string lowerPath)
            {
                return (lowerPath.StartsWith("packages/"));
            }
            private static PassType GetPassType(string str)
            {
                str = str.ToUpper();
                switch (str)
                {
                    case "":
                    case "ALWAYS":
                        return PassType.Normal;
                    case "VERTEX":
                        return PassType.Vertex;
                    case "VERTEXLM":
                        return PassType.VertexLM;
                    case "VERTEXLMRGBM":
                        return PassType.VertexLMRGBM;
                    case "FORWARDBASE":
                        return PassType.ForwardBase;
                    case "FORWARDADD":
                        return PassType.ForwardAdd;
                    case "PREPASSBASE":
                        return PassType.LightPrePassBase;
                    case "PREPASSFINAL":
                        return PassType.LightPrePassFinal;
                    case "SHADOWCASTER":
                        return PassType.ShadowCaster;
                    case "DEFERRED":
                        return PassType.Deferred;
                    case "META":
                        return PassType.Meta;
                    case "MOTIONVECTORS":
                        return PassType.MotionVectors;
                    case "SRPDEFAULTUNLIT":
                        return PassType.ScriptableRenderPipelineDefaultUnlit;
                }
                //                PassType.ScriptableRenderPipelineDefaultUnlit
                return PassType.ScriptableRenderPipeline;
            }
        }

        private Dictionary<string, ShaderVariantInfo> variantDict;
        private EFlag variantFlags;


        public static void AddFromScannedData(ShaderVariantCollection shaderVariantCollection, EFlag flags)
        {
            var obj = new ShaderVariantCollectionCreator(flags);
            obj.AddToShaderVariantCollection(shaderVariantCollection);
        }

        private ShaderVariantCollectionCreator(EFlag flags)
        {
            this.variantFlags = flags;
        }

        private void AddToShaderVariantCollection(ShaderVariantCollection shaderVariantCollection)
        {
            CreateDictionary();
            AddShaderVariantFromDict(shaderVariantCollection);
        }

        private void AddShaderVariantFromDict(ShaderVariantCollection shaderVariantCollection) {

            foreach (var variantInfo in this.variantDict.Values)
            {
                var list = variantInfo.CreateShaderVariantList(this.variantFlags);
                AddShaderVariantList(shaderVariantCollection, list);
            }
        }

        private void AddShaderVariantList(ShaderVariantCollection shaderVariantCollection, List<ShaderVariantCollection.ShaderVariant> list)
        {
            foreach (var variant in list)
            {
                try
                {
                    if (!shaderVariantCollection.Contains(variant))
                    {
                        shaderVariantCollection.Add(variant);
                    }
                } catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        private void CreateDictionary()
        {
            this.variantDict = new Dictionary<string, ShaderVariantInfo>();
            var files = GetFiles();

            foreach (var file in files) {
                try
                {
                    var dumpInfo = FrameDebugDumpInfo.LoadFromFile(file);
                    ExecuteDumpInfo(dumpInfo);
                } catch (System.Exception e)
                {
                    Debug.LogError(file + e);
                }
            }
        }


        private void ExecuteDumpInfo(FrameDebugDumpInfo dumpInfo)
        {
            foreach (var eventInfo in dumpInfo.events)
            {
                ExecShaderInfo(eventInfo.shaderInfo);
            }
        }


        private void ExecShaderInfo(FrameDebugDumpInfo.ShaderInfo shaderInfo)
        {
            if (string.IsNullOrEmpty(shaderInfo.shaderName))
            {
                return;
            }
            ShaderVariantInfo info;
            if (!variantDict.TryGetValue(shaderInfo.shaderName, out info))
            {
                info = new ShaderVariantInfo(shaderInfo.shaderName);
                variantDict.Add(shaderInfo.shaderName, info);
            }
            info.AddKeywords(shaderInfo.shaderKeywords, shaderInfo.passLightMode);
        }


        private List<string> GetFiles()
        {
            var dirs = System.IO.Directory.GetDirectories(FrameInfoCrawler.RootSaveDir);
            List<string> list = new List<string>(dirs.Length);
            foreach (var dir in dirs)
            {
                string path = System.IO.Path.Combine(dir, "detail.json");
                if (System.IO.File.Exists(path))
                {
                    list.Add(path);
                }
            }
            return list;
        }
    }
    public static class Extentions
    {
        public static bool HasFlag(this ShaderVariantCollectionCreator.EFlag src, ShaderVariantCollectionCreator.EFlag flag)
        {
            return ((src & flag) == flag);
        }
    }
}