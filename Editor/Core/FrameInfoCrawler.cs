using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace UTJ.FrameDebugSave
{
    public class FrameInfoCrawler
    {
        [System.Flags]
        public enum CaptureFlag
        {
            None = 0,
            FinalTexture = 1, // EditorOnly
            ScreenShotBySteps = 2,// EditorOnly
            CaptureFromOriginRT = 4,// EditorOnly
            CaptureScreenDepth = 8,// EditorOnly
            ShaderTexture = 16, //EditorOnly
        }

        public class ShaderPropertyInfo
        {
            public string name;
            public int flags;
            public string textureName;
            public object value;

            public TextureUtility.SaveTextureInfo saveTextureInfo;
        }


        public class ShaderProperties
        {
            public System.Array floats;
            public System.Array vectors;
            public System.Array matrices;
            public System.Array textures;
            public System.Array buffers;

            // NonSerialized
            public List<ShaderPropertyInfo> convertedFloats;
            public List<ShaderPropertyInfo> convertedVectors;
            public List<ShaderPropertyInfo> convertedMatricies;
            public List<ShaderPropertyInfo> convertedTextures;
            public List<ShaderPropertyInfo> convertedBuffers;
        }

        public class FrameDebuggerEventData
        {
            // inform
            public int frameEventIndex;
            public int vertexCount;
            public int indexCount;
            public int instanceCount;
            public int drawCallCount;
            public string shaderName;
            public string passName;
            public string passLightMode;
            public int shaderInstanceID;
            public int subShaderIndex;
            public int shaderPassIndex;
            public string shaderKeywords;
            public int componentInstanceID;
            public int meshInstanceID;
            public int meshSubset;

            // state for compute shader dispatches
            public int csInstanceID;
            public string csName;
            public string csKernel;
            public int csThreadGroupsX;
            public int csThreadGroupsY;
            public int csThreadGroupsZ;

            // active render target info
            public string rtName;
            public int rtWidth;
            public int rtHeight;
            public int rtFormat;
            public int rtDim;
            public int rtFace;
            public short rtCount;
            public short rtHasDepthTexture;

            public int batchBreakCause;

            public object shaderProperties;

            // non Serialized 
            public TextureUtility.SaveTextureInfo savedScreenShotInfo;
            public string batchBreakCauseStr;
            public ShaderProperties convertedProperties;
        }

        public class FrameDebuggerEvent
        {
            public object type;
            public GameObject gameObject;
        }


        public bool IsRunning
        {
            get;private set;
        }



        public List<FrameDebuggerEventData> frameDebuggerEventDataList { get; private set; }
        public List<FrameDebuggerEvent> frameDebuggerEventList { get; private set; }

        public string saveDirectory { get; private set; }

        private IEnumerator enumerator;
        private System.Action endCallback;

        private ReflectionCache reflectionCache;
        private ReflectionType frameDebuggeUtil;
        private ReflectionType frameEventData;
        private float currentProgress;


        private ReflectionClassWithObject currentFrameEventData;
        private ReflectionClassWithObject frameDebuggerWindowObj;

        private string[] breakReasons;
        private CaptureFlag captureFlag;

        private Dictionary<int, TextureUtility.SaveTextureInfo> alreadyWriteTextureDict;
        private Dictionary<int, int> renderTextureLastChanged;


        public FrameInfoCrawler(ReflectionCache rcache)
        {
            this.reflectionCache = rcache;
            this.frameDebuggeUtil = reflectionCache.GetTypeObject("UnityEditorInternal.FrameDebuggerUtility");
            this.frameEventData = reflectionCache.GetTypeObject("UnityEditorInternal.FrameDebuggerEventData");

            var frameDebuggerWindowType = this.reflectionCache.GetTypeObject("UnityEditor.FrameDebuggerWindow");
            var window = frameDebuggerWindowType.CallMethod<object>("ShowFrameDebuggerWindow", null, null);
            this.frameDebuggerWindowObj = new ReflectionClassWithObject(frameDebuggerWindowType, window);

            this.IsRunning = false;
            this.alreadyWriteTextureDict = new Dictionary<int, TextureUtility.SaveTextureInfo>();
            this.renderTextureLastChanged = new Dictionary<int, int>();
        }
        public void Request(CaptureFlag flag,System.Action callback)
        {
            if(this.IsRunning) { return; }
            this.captureFlag = flag;
            this.IsRunning = true;
            this.alreadyWriteTextureDict.Clear();
            this.renderTextureLastChanged.Clear();


            var date = System.DateTime.Now;
            var dateString = string.Format("{0:D4}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}_", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
            var profilerName = ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler);

            this.saveDirectory = "FrameDebugger/" + dateString + profilerName;
            System.IO.Directory.CreateDirectory(saveDirectory);

            endCallback = callback;
            EditorApplication.update += Update;
        }

        public void CloseFrameDebuggerWindow()
        {
            frameDebuggerWindowObj.CallMethod<object>("Close", null);
        }

        private void Update()
        {
            if (enumerator == null)
            {
                enumerator = Execute();
            }
            bool result = enumerator.MoveNext();
            if(!result)
            {
                endCallback();
                this.IsRunning = false;
                EditorApplication.update -= Update;
            }
        }

        private IEnumerator Execute()
        {
            currentProgress = 0.0f;
            var waitForConnect = WaitForRemoteConnect(10.0);
            while (waitForConnect.MoveNext())
            {
                yield return null;
            }
            yield return null;
            yield return null;

            var r = this.reflectionCache;
            var frameEvents = frameDebuggeUtil.CallMethod<System.Array>( "GetFrameEvents", null, null);
            var breakReasons = frameDebuggeUtil.CallMethod<string[]>("GetBatchBreakCauseStrings", null, null);
            var frameEventDatas = r.GetTypeObject("UnityEditorInternal.FrameDebuggerEventData");
            var evtData = frameDebuggeUtil.CreateInstance();
            int count = frameDebuggeUtil.GetPropertyValue<int>( "count", null);


            this.CreateFrameDebuggerEventList(frameEvents);
            this.frameDebuggerEventDataList = new List<FrameDebuggerEventData>(count);


            for ( int i = 0; i <= count; ++i)
            {
                yield return null;
                this.frameDebuggerWindowObj.CallMethod<object>("ChangeFrameEventLimit",new object[] { i });
                this.frameDebuggerWindowObj.CallMethod<object>("RepaintOnLimitChange",null);
                int targetFrameIdx = i - 1;
                if(targetFrameIdx < 0 || targetFrameIdx >= frameEvents.Length) { continue; }

                // getTargetFrameInfo
                var eventDataCoroutine = this.TryGetFrameEvnetData(targetFrameIdx, 2.0);
                while (eventDataCoroutine.MoveNext())
                {
                    yield return null;
                }
                var frameData = currentFrameEventData;
                if( frameData == null) {
                    UnityEngine.Debug.LogWarning("failed capture " + targetFrameIdx);
                    continue;
                }

                FrameDebuggerEventData frameInfo = new FrameDebuggerEventData();
                frameData.CopyFieldsToObjectByVarName<FrameDebuggerEventData>(ref frameInfo);
                frameInfo.batchBreakCauseStr = breakReasons[frameInfo.batchBreakCause];
                this.CreateShaderPropInfos(frameInfo);
                frameDebuggerEventDataList.Add(frameInfo);

                bool isRemoteEnalbed = frameDebuggeUtil.CallMethod<bool>("IsRemoteEnabled", null, null);
                if (!isRemoteEnalbed)
                {
                    SetRenderTextureLastChange(frameInfo);
                    // save shader texture
                    ExecuteShaderTextureSave(frameInfo);
                    // capture screen shot
                    ExecuteSaveScreenShot(frameInfo,(i==count) );
                }
            }
            yield return null;
        }

        private void SetRenderTextureLastChange(FrameDebuggerEventData frameInfo)
        {
            var rt = TextureUtility.GetTargetRenderTexture(frameInfo);
            if( rt == null)
            {
                return;
            }
            int instanceId = rt.GetInstanceID();
            if(this.renderTextureLastChanged.ContainsKey(instanceId))
            {
                renderTextureLastChanged[instanceId] = frameInfo.frameEventIndex;
            }
            else
            {
                renderTextureLastChanged.Add(instanceId, frameInfo.frameEventIndex);
            }
        }

        private void CreateShaderPropInfos(FrameDebuggerEventData frameInfo)
        {
            var originProps = frameInfo.shaderProperties;
            var originPropType = this.reflectionCache.GetTypeObject(originProps.GetType());
            var originPropReflection = new ReflectionClassWithObject( originPropType,originProps);
            frameInfo.convertedProperties = new ShaderProperties();
            originPropReflection.CopyFieldsToObjectByVarName<ShaderProperties>(ref frameInfo.convertedProperties);
            var props = frameInfo.convertedProperties;

            props.convertedFloats = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache,props.floats);
            props.convertedVectors = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache, props.vectors);
            props.convertedMatricies = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache,props.matrices);
            props.convertedTextures = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache,props.textures);
            props.convertedBuffers = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache,props.buffers);

        }

        private IEnumerator WaitForRemoteConnect(double deltaTime)
        {
            bool isRemoteEnalbed = frameDebuggeUtil.CallMethod<bool>("IsRemoteEnabled", null, null);
            bool isReceiving = frameDebuggeUtil.GetPropertyValue<bool>("receivingRemoteFrameEventData", null);

            double startTime = EditorApplication.timeSinceStartup;
            // wait for remote data
            if (isRemoteEnalbed && isReceiving)
            {
                while ( (EditorApplication.timeSinceStartup - startTime) < deltaTime)
                {
                    this.frameDebuggerWindowObj.CallMethod<object>("RepaintOnLimitChange", null);
                    isRemoteEnalbed = frameDebuggeUtil.CallMethod<bool>("IsRemoteEnabled", null, null);
                    isReceiving = frameDebuggeUtil.GetPropertyValue<bool>("receivingRemoteFrameEventData", null);
                    if (isRemoteEnalbed && isReceiving)
                    {
                        yield return null;
                    }
                    else
                    {
                        break;
                    }
                }
            }

        }

        private IEnumerator TryGetFrameEvnetData(int frameIdx,double deltaTime = 2 )
        {

            double startTime = EditorApplication.timeSinceStartup;

            int limit = frameDebuggeUtil.GetPropertyValue<int>("limit", null);
            while ((EditorApplication.timeSinceStartup - startTime) < deltaTime)
            {
                bool res = GetFrameEventData(frameIdx,out this.currentFrameEventData);
                if (res)
                {
                    break;
                }
                else { 
                    currentFrameEventData = null;
                    yield return null;
                }
            }
        }

        private bool GetFrameEventData(int frameIdx,out ReflectionClassWithObject ret)
        {
            object[] args = null;

#if UNITY_2019_3_OR_NEWER
            args = new object[]{ frameIdx,  this.frameEventData.CreateInstance()};
#else
            args = new object[] { frameIdx, null };
#endif
            bool result = this.frameDebuggeUtil.CallMethod<bool>( "GetFrameEventData", null, args);
            if (result)
            {
                ret = new ReflectionClassWithObject(frameEventData, args[1]);
            }
            else
            {
                ret = null;
            }
            return result;
        }

        private void CreateFrameDebuggerEventList(System.Array arr)
        {
            this.frameDebuggerEventList = ReflectionClassWithObject.CopyToListFromArray<FrameDebuggerEvent>(this.reflectionCache,arr);
        }


        private void ExecuteShaderTextureSave(FrameDebuggerEventData frameInfo)
        {
            if (!(this.captureFlag.HasFlag(CaptureFlag.ShaderTexture))){
                return;
            }
            var textureParams = frameInfo.convertedProperties.convertedTextures;
            foreach( var textureParam in textureParams)
            {
                TextureUtility.SaveTextureInfo saveTextureInfo = null;
                var texture = textureParam.value as Texture;
                if (texture == null) { continue; }
                // already saved
                if (alreadyWriteTextureDict.TryGetValue(texture.GetInstanceID(), out saveTextureInfo))
                {
                    textureParam.saveTextureInfo = saveTextureInfo;
                    continue;
                }
                // save texture
                string dir = System.IO.Path.Combine(this.saveDirectory, "shaderTexture");
                string path = null;
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
                if ( texture.GetType() == typeof(Texture2D))
                {
                    path = System.IO.Path.Combine(dir, texture.name); ;
                    saveTextureInfo = TextureUtility.SaveTexture((Texture2D)texture,path);
                    alreadyWriteTextureDict.Add(texture.GetInstanceID(), saveTextureInfo);
                }
                if( texture.GetType() == typeof(RenderTexture))
                {
                    int idx = -1;
                    renderTextureLastChanged.TryGetValue(texture.GetInstanceID(),out idx);
                    path = System.IO.Path.Combine(dir, "RT_" + idx + "_" + texture.name);
                    saveTextureInfo = TextureUtility.SaveRenderTexture((RenderTexture)texture, path);
                }
                textureParam.saveTextureInfo = saveTextureInfo;
            }
        }

        private void ExecuteSaveScreenShot(FrameDebuggerEventData frameInfo,bool isFinalFrameEvent)
        {
            if( !(this.captureFlag.HasFlag(CaptureFlag.FinalTexture)) && 
                !(this.captureFlag.HasFlag(CaptureFlag.ScreenShotBySteps))) {
                return;
            }
            if( !this.captureFlag.HasFlag(CaptureFlag.ScreenShotBySteps) && !isFinalFrameEvent)
            {
                return;
            }
            RenderTexture renderTexture = null;
            bool isGetFromTargetRT = this.captureFlag.HasFlag(CaptureFlag.CaptureFromOriginRT) && !isFinalFrameEvent;

            if (isGetFromTargetRT)
            {
                renderTexture = TextureUtility.GetTargetRenderTexture(frameInfo);
            }
            else
            {
                renderTexture = TextureUtility.GetGameViewRT();
            }
            if( renderTexture != null && renderTexture)
            {
                string dir = System.IO.Path.Combine(this.saveDirectory, "screenshot");
                string path = null;

                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                if (isFinalFrameEvent)
                {
                    path = System.IO.Path.Combine(dir, "final");
                }
                else
                {
                    path = System.IO.Path.Combine(dir, "ss-" + frameInfo.frameEventIndex );
                }

                TextureUtility.SaveRenderTexture(renderTexture, path);                
            }
        }
        
    }
}