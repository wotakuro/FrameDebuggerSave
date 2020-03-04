using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UTJ.FrameDebugSave
{
    public class ShaderVariantCollectionCreator
    {
        

        private class ShaderDrawInfo
        {
            public string keywords;
            public string passLightMode;

            public ShaderDrawInfo(string kw,string lm)
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
                if( target == null) { return false; }
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
            public List<ShaderVariantCollection.ShaderVariant> CreateShaderVariantList()
            {
                List<ShaderVariantCollection.ShaderVariant> shaderVariants = new List<ShaderVariantCollection.ShaderVariant>();
                var shaderInstance = Shader.Find(this.shader);


                //                UnityEngine.Debug.Log(shaderInstance);
                if (shaderInstance == null)
                {
                    Debug.LogError("Shader not found " + this.shader);
                    return shaderVariants;
                }
                foreach (var info in shaderDrawInfo)
                {
                    string[] keywordArray; 
                    if(string.IsNullOrEmpty(info.keywords))
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


        public static void AddFromScannedData(ShaderVariantCollection shaderVariantCollection)
        {
            var obj = new ShaderVariantCollectionCreator();
            obj.AddToShaderVariantCollection(shaderVariantCollection);
        }

        private ShaderVariantCollectionCreator()
        {

        }

        private void AddToShaderVariantCollection(ShaderVariantCollection shaderVariantCollection)
        {
            CreateDictionary();
            AddShaderVariantFromDict(shaderVariantCollection);
        }

        private void AddShaderVariantFromDict(ShaderVariantCollection shaderVariantCollection) {

            foreach( var variantInfo in this.variantDict.Values)
            {
                var list = variantInfo.CreateShaderVariantList();
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
                }catch(System.Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        private void CreateDictionary()
        {
            this.variantDict = new Dictionary<string, ShaderVariantInfo>();
            var files = GetFiles();

            foreach( var file in files) {
                try
                {
                    var dumpInfo = FrameDebugDumpInfo.LoadFromFile(file);
                    ExecuteDumpInfo(dumpInfo);
                }catch(System.Exception e)
                {
                    Debug.LogError(file + e);
                }
            }
        }


        private void ExecuteDumpInfo(FrameDebugDumpInfo dumpInfo)
        {
            foreach(var eventInfo in dumpInfo.events)
            {
                ExecShaderInfo(eventInfo.shaderInfo);
            }
        }
        

        private void ExecShaderInfo(FrameDebugDumpInfo.ShaderInfo shaderInfo)
        {
            if(string.IsNullOrEmpty(shaderInfo.shaderName))
            {
                return;
            }
            ShaderVariantInfo info;
            if(!variantDict.TryGetValue(shaderInfo.shaderName,out info))
            {
                info = new ShaderVariantInfo(shaderInfo.shaderName);
                variantDict.Add(shaderInfo.shaderName, info);
            }
            info.AddKeywords(shaderInfo.shaderKeywords,shaderInfo.passLightMode);
        }


        private List<string> GetFiles()
        {
            var dirs = System.IO.Directory.GetDirectories(FrameInfoCrawler.RootSaveDir);
            List<string> list = new List<string>(dirs.Length);
            foreach( var dir in dirs)
            {
                string path = System.IO.Path.Combine(dir, "detail.json");
                if( System.IO.File.Exists(path))
                {
                    list.Add(path);
                }
            }
            return list;
        }
    }
}