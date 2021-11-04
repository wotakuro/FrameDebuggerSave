using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Text;

namespace UTJ.FrameDebugSave
{
    public class FameDebugSave {
        private FrameInfoCrawler crawler;
        private ReflectionCache reflectionCache;
        private FrameInfoCrawler.CaptureFlag captureFlag;
        private StringBuilder stringBuilder = new StringBuilder();
        private System.Action OnEndAct;


        public void Execute(FrameInfoCrawler.CaptureFlag flag,System.Action endCall = null)
        {
            this.captureFlag = flag;
            if ( this.reflectionCache == null)
            {
                this.reflectionCache = new ReflectionCache();
            }
            this.OnEndAct = endCall;

            var frameDebuggeUtil = reflectionCache.GetTypeObject("UnityEditorInternal.FrameDebuggerUtility");

            // show FrameDebuggerWindow
            var frameDebuggerWindow = reflectionCache.GetTypeObject("UnityEditor.FrameDebuggerWindow");
            object windowObj = frameDebuggerWindow.CallMethod<object>("ShowFrameDebuggerWindow", null, null);
            frameDebuggerWindow.CallMethod<object>("EnableIfNeeded", windowObj, null);
            if (crawler == null)
            {
                crawler = new FrameInfoCrawler(this.reflectionCache);
            }
            crawler.Request(flag, EndCrawler);
        }




        private void EndCrawler()
        {
            string dirPath = crawler.saveDirectory;
            // directory
            SaveFrameDebuggerEventsCsv(dirPath);
            SaveDetailJsonData(dirPath);
            crawler = null;
            OnEndAct?.Invoke();
        }

        private void SaveFrameDebuggerEventsCsv(string dirPath)
        {
            CsvStringGenerator csvStringGenerator = new CsvStringGenerator();
            csvStringGenerator.AppendColumn("frameEventIndex");
            csvStringGenerator.AppendColumn("type");

            csvStringGenerator.AppendColumn("rtName").
                AppendColumn("rtWidth").AppendColumn("rtHeight").
                AppendColumn("rtCount").AppendColumn("rtHasDepthTexture");

            csvStringGenerator.AppendColumn("vertexCount").
                AppendColumn("indexCount").
                AppendColumn("instanceCount").
                AppendColumn("drawCallCount").
                AppendColumn("shaderName").
                AppendColumn("passName").
                AppendColumn("passLightMode").
                AppendColumn("subShaderIndex").
                AppendColumn("shaderPassIndex").
                AppendColumn("shaderKeywords").
                AppendColumn("componentInstanceID").
                AppendColumn("meshInstanceID").
                AppendColumn("meshSubset");

            csvStringGenerator.AppendColumn("batchBreakCause");
            csvStringGenerator.NextRow();


            for (int i = 0; i < crawler.frameDebuggerEventDataList.Count; ++i)
            {
                var evtData = crawler.frameDebuggerEventDataList[i];
                var evt = crawler.frameDebuggerEventList[i];

                csvStringGenerator.AppendColumn(evtData.frameEventIndex);
                csvStringGenerator.AppendColumn(evt.type.ToString());

                csvStringGenerator.AppendColumn(evtData.rtName).
                    AppendColumn(evtData.rtWidth).AppendColumn(evtData.rtHeight).
                    AppendColumn(evtData.rtCount).AppendColumn(evtData.rtHasDepthTexture);

                csvStringGenerator.AppendColumn(evtData.vertexCount).
                    AppendColumn(evtData.indexCount).
                    AppendColumn(evtData.instanceCount).
                    AppendColumn(evtData.drawCallCount).
                    AppendColumn(evtData.shaderName).
                    AppendColumn(evtData.passName).
                    AppendColumn(evtData.passLightMode).
                    AppendColumn(evtData.subShaderIndex).
                    AppendColumn(evtData.shaderPassIndex).
                    AppendColumn(evtData.shaderKeywords).
                    AppendColumn(evtData.componentInstanceID).
                    AppendColumn(evtData.meshInstanceID).
                    AppendColumn(evtData.meshSubset);
                csvStringGenerator.AppendColumn(evtData.batchBreakCauseStr);
                csvStringGenerator.NextRow();
            }
            File.WriteAllText(Path.Combine(dirPath, "events.csv"), csvStringGenerator.ToString());
        }

        private void SaveDetailJsonData(string dirPath)
        {
            JsonStringGenerator jsonStringGenerator = new JsonStringGenerator();
            try
            {
                using (new JsonStringGenerator.ObjectScope(jsonStringGenerator))
                {
                    jsonStringGenerator.AddObjectValue("captureFlag", (int)captureFlag);
                    using (new JsonStringGenerator.ObjectArrayValueScope(jsonStringGenerator, "events"))
                    {
                        for (int i = 0; i < crawler.frameDebuggerEventDataList.Count; ++i)
                        {
                            var evt = crawler.frameDebuggerEventList[i];
                            var evtData = crawler.frameDebuggerEventDataList[i];
                            AppendFrameEvent(jsonStringGenerator, evt, evtData);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            File.WriteAllText(Path.Combine(dirPath, "detail.json"), jsonStringGenerator.ToString());
        }

        private void AppendFrameEvent(JsonStringGenerator jsonStringGenerator,
            FrameInfoCrawler.FrameDebuggerEvent evt,
            FrameInfoCrawler.FrameDebuggerEventData evtData)
        {
            using (new JsonStringGenerator.ObjectScope(jsonStringGenerator))
            {
                jsonStringGenerator.AddObjectValue("frameEventIndex",evtData.frameEventIndex);
                jsonStringGenerator.AddObjectValue("type",evt.type.ToString());
                using (var objArray = new JsonStringGenerator.ObjectArrayValueScope(jsonStringGenerator, "screenshots")) {
                    foreach (var screenshotInfo in evtData.savedScreenShotInfo)
                    {
                        AppendSavedTextureInfoWithoutName(jsonStringGenerator, screenshotInfo);
                    }
                }
                AppendRenderingInfo(jsonStringGenerator, evt, evtData);
                AppendRenderTargetInfo(jsonStringGenerator, evtData);
                AppendShaderInfo(jsonStringGenerator, evtData);
            }
        }
        private void AppendRenderingInfo(JsonStringGenerator jsonStringGenerator,
            FrameInfoCrawler.FrameDebuggerEvent evt,
            FrameInfoCrawler.FrameDebuggerEventData evtData)
        {
            using (new JsonStringGenerator.ObjectScopeWithName(jsonStringGenerator, "rendering"))
            {
                jsonStringGenerator.AddObjectValue("vertexCount", evtData.vertexCount).
                    AddObjectValue("indexCount", evtData.indexCount).
                    AddObjectValue("instanceCount", evtData.instanceCount).
                    AddObjectValue("drawCallCount", evtData.drawCallCount).
                    AddObjectValue("componentInstanceID", evtData.componentInstanceID).
                    AddObjectValue("meshInstanceID", evtData.meshInstanceID).
                    AddObjectValue("meshSubset", evtData.meshSubset).
                    AddObjectValue("batchBreakCauseStr", evtData.batchBreakCauseStr);
                if(evt.gameObject)
                {
                    stringBuilder.Length = 0;
                    GetGameObjectName(evt.gameObject, stringBuilder);
                    jsonStringGenerator.AddObjectValue("gameobject", stringBuilder.ToString() );
                }
            }
        }
        private void GetGameObjectName(GameObject gameObject,StringBuilder sb)
        {
            var parent = gameObject.transform.parent;
            if( parent != null)
            {
                GetGameObjectName(parent.gameObject, sb);
            }
            if (sb.Length > 0)
            {
                sb.Append('/');
            }
            sb.Append(gameObject.name);
        }

        private void AppendRenderTargetInfo(JsonStringGenerator jsonStringGenerator,
            FrameInfoCrawler.FrameDebuggerEventData evtData)
        {
            using (new JsonStringGenerator.ObjectScopeWithName(jsonStringGenerator, "renderTarget"))
            {
                jsonStringGenerator.AddObjectValue("rtName",evtData.rtName).
                    AddObjectValue("rtWidth", evtData.rtWidth).AddObjectValue("rtHeight", evtData.rtHeight).
                    AddObjectValue("rtCount",evtData.rtCount).AddObjectValue("rtHasDepthTexture",evtData.rtHasDepthTexture);
            }
        }


        private void AppendShaderInfo(JsonStringGenerator jsonStringGenerator,
            FrameInfoCrawler.FrameDebuggerEventData evtData)
        {

            using (new JsonStringGenerator.ObjectScopeWithName(jsonStringGenerator, "shaderInfo"))
            {
                jsonStringGenerator.AddObjectValue("shaderName", evtData.shaderName);
                jsonStringGenerator.AddObjectValue("subShaderIndex", evtData.subShaderIndex);
                jsonStringGenerator.AddObjectValue("shaderPassIndex", evtData.shaderPassIndex);
                jsonStringGenerator.AddObjectValue("passName", evtData.passName);
                jsonStringGenerator.AddObjectValue("passLightMode", evtData.passLightMode);
                jsonStringGenerator.AddObjectValue("shaderKeywords", evtData.shaderKeywords);

                AppendShaderParam(jsonStringGenerator, evtData.convertedProperties);
            }
        }

        private void AppendShaderParam(JsonStringGenerator jsonStringGenerator, FrameInfoCrawler.ShaderProperties shaderParams)
        {
            using (new JsonStringGenerator.ObjectScopeWithName(jsonStringGenerator, "shaderParams"))
            {
                AppendShaderParamTextures(jsonStringGenerator, shaderParams);
                AppendShaderParamFloats(jsonStringGenerator, shaderParams);
                AppendShaderParamVectors(jsonStringGenerator, shaderParams);
                AppendShaderParamMatricies(jsonStringGenerator, shaderParams);
                
            }
        }
        private void AppendShaderParamTextures(JsonStringGenerator jsonStringGenerator, FrameInfoCrawler.ShaderProperties shaderParams)
        {
            if (shaderParams.convertedTextures == null) { return; }

            using (new JsonStringGenerator.ObjectArrayValueScope(jsonStringGenerator, "textures"))
            {
                foreach (var textureParam in shaderParams.convertedTextures)
                {
                    using (new JsonStringGenerator.ObjectScope(jsonStringGenerator))
                    {
                        var val = textureParam.value as Texture;
                        jsonStringGenerator.AddObjectValue("name", textureParam.name);
                        jsonStringGenerator.AddObjectValue("textureName", textureParam.textureName);
                        if (val != null)
                        {
#if UNITY_2019_1_OR_NEWER || UNITY_2019_OR_NEWER
                            jsonStringGenerator.AddObjectValue("originFormat", val.graphicsFormat.ToString());
#endif
                            jsonStringGenerator.AddObjectValue("originWidth", val.width);
                            jsonStringGenerator.AddObjectValue("originHeight", val.height);
                            jsonStringGenerator.AddObjectValue("originMipCount", TextureUtility.GetMipMapCount(val) );
                        }
                        var saveInfo = textureParam.saveTextureInfo;
                        AppendSavedTextureInfo(jsonStringGenerator, "saved", saveInfo);
                    }
                }
            }
        }
        private void AppendSavedTextureInfo(JsonStringGenerator jsonStringGenerator, string objName, FrameDebugDumpInfo.SavedTextureInfo saveInfo)
        {
            if (saveInfo == null) { return; }
            using (new JsonStringGenerator.ObjectScopeWithName(jsonStringGenerator, objName))
            {
                AppendSavedTextureInfoBody(jsonStringGenerator, saveInfo);
            }
        }
        private void AppendSavedTextureInfoWithoutName(JsonStringGenerator jsonStringGenerator, FrameDebugDumpInfo.SavedTextureInfo saveInfo)
        {
            if (saveInfo == null) { return; }
            using (new JsonStringGenerator.ObjectScope(jsonStringGenerator))
            {
                AppendSavedTextureInfoBody(jsonStringGenerator, saveInfo);
            }
        }

        private void AppendSavedTextureInfoBody(JsonStringGenerator jsonStringGenerator, FrameDebugDumpInfo.SavedTextureInfo saveInfo)
        {
            jsonStringGenerator.AddObjectValue("path", saveInfo.path);
            jsonStringGenerator.AddObjectValue("type", saveInfo.type);
            jsonStringGenerator.AddObjectValue("width", saveInfo.width);
            jsonStringGenerator.AddObjectValue("height", saveInfo.height);
            jsonStringGenerator.AddObjectValue("mipCount", saveInfo.mipCount);
            jsonStringGenerator.AddObjectValue("textureFormat", saveInfo.textureFormat);

#if UNITY_2020_2_OR_NEWER
            jsonStringGenerator.AddObjectValue("originGraphicsFormat", saveInfo.originGraphicsFormat);
            jsonStringGenerator.AddObjectValue("saveGraphicsFormat", saveInfo.saveGraphicsFormat);
#endif

        }

        private void AppendShaderParamFloats(JsonStringGenerator jsonStringGenerator, FrameInfoCrawler.ShaderProperties shaderParams)
        {
            if (shaderParams.convertedFloats == null) { return; }

            using (new JsonStringGenerator.ObjectArrayValueScope(jsonStringGenerator, "floats"))
            {
                foreach (var floatParam in shaderParams.convertedFloats)
                {
                    using (new JsonStringGenerator.ObjectScope(jsonStringGenerator))
                    {
                        jsonStringGenerator.AddObjectValue("name", floatParam.name);
                        jsonStringGenerator.AddObjectValue("val", (float)floatParam.value);
                    }
                }
            }
        }
        private void AppendShaderParamVectors(JsonStringGenerator jsonStringGenerator, FrameInfoCrawler.ShaderProperties shaderParams)
        {
            if (shaderParams.convertedVectors == null) { return; }

            using (new JsonStringGenerator.ObjectArrayValueScope(jsonStringGenerator, "vectors"))
            {
                foreach (var vectorParam in shaderParams.convertedVectors)
                {
                    using (new JsonStringGenerator.ObjectScope(jsonStringGenerator))
                    {
                        var val = (Vector4)vectorParam.value;
                        jsonStringGenerator.AddObjectValue("name",vectorParam.name);
                        jsonStringGenerator.AddObjectVector("val", ref val);
                    }
                }

            }
        }
        private void AppendShaderParamMatricies(JsonStringGenerator jsonStringGenerator, FrameInfoCrawler.ShaderProperties shaderParams)
        {
            if (shaderParams.convertedMatricies == null) { return; }
            using (new JsonStringGenerator.ObjectArrayValueScope(jsonStringGenerator, "matricies"))
            {
                foreach (var matrixParam in shaderParams.convertedMatricies)
                {
                    using (new JsonStringGenerator.ObjectScope(jsonStringGenerator))
                    {
                        var val = (Matrix4x4)matrixParam.value;
                        jsonStringGenerator.AddObjectValue("name",matrixParam.name);
                        jsonStringGenerator.AddObjectMatrix("val", ref val);
                    }
                }
            }
        }
        private void AppendShaderParamBuffers(JsonStringGenerator jsonStringGenerator, FrameInfoCrawler.ShaderProperties shaderParams)
        {
            if (shaderParams.convertedBuffers == null) { return; }
            using (new JsonStringGenerator.ObjectArrayValueScope(jsonStringGenerator, "buffers"))
            {
                foreach (var bufferParam in shaderParams.convertedBuffers)
                {
                    jsonStringGenerator.AddObjectValue(bufferParam.name, "No Implements");
                }
            }
        }

    }
}