using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTJ.FrameDebugSave
{
    public class ShaderVariantCollectionCreator
    {

        private class ShaderVariantInfo
        {
            public string shader;
            public HashSet<string> keywordsList;

            public ShaderVariantInfo(string sh)
            {
                this.shader = sh;
                this.keywordsList = new HashSet<string>();
            }
            public void AddKeywords(string keywords)
            {
                if (!keywordsList.Contains(keywords))
                {
                    keywordsList.Add(keywords);
                }
            }
            public List<ShaderVariantCollection.ShaderVariant> CreateShaderVariantList(UnityEngine.Rendering.PassType passType)
            {
                List<ShaderVariantCollection.ShaderVariant> shaderVariants = new List<ShaderVariantCollection.ShaderVariant>();
                var shaderInstance = Shader.Find(this.shader);
                foreach( var keywords in keywordsList)
                {
                    var keywordArray = keywords.Split(' ');
                    ShaderVariantCollection.ShaderVariant variant = new ShaderVariantCollection.ShaderVariant(shaderInstance, passType, keywordArray);
                }
                return shaderVariants;
            }
        }

        private Dictionary<string, ShaderVariantInfo> variantDict;


        public void AddToShaderVariantCollection(ShaderVariantCollection shaderVariantCollection)
        {
            CreateDictionary();
            AddShaderVariantFromDict(shaderVariantCollection);
        }

        private void AddShaderVariantFromDict(ShaderVariantCollection shaderVariantCollection) {

            foreach( var variantInfo in this.variantDict.Values)
            {
                var list = variantInfo.CreateShaderVariantList(UnityEngine.Rendering.PassType.Normal);
                AddShaderVariantList(shaderVariantCollection, list);
            }
        }

        private void AddShaderVariantList(ShaderVariantCollection shaderVariantCollection, List<ShaderVariantCollection.ShaderVariant> list)
        {
            foreach (var variant in list)
            {
                if (!shaderVariantCollection.Contains(variant))
                {
                    shaderVariantCollection.Add(variant);
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
                    Debug.LogError(e);
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
            }
            info.AddKeywords(shaderInfo.shaderKeywords);
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