using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Text;

namespace UTJ.FrameDebugSave
{
    public class FrameInfoCrawler
    {
        public static readonly string RootSaveDir = "FrameDebugger";

        [System.Flags]
        public enum CaptureFlag:int
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
#if UNITY_2022_1_OR_NEWER
            public string m_Name;
            public int m_Flags;
            public string m_TextureName;
            public object m_Value;

            public string name { get { return m_Name; } }
            public int flag { get { return m_Flags; } }
            public string textureName { get { return m_TextureName; } }
            public object value { get { return m_Value; } }
#else
            public string name;
            public int flags;
            public string textureName;
            public object value;
#endif

            public TextureUtility.SaveTextureInfo saveTextureInfo;
        }


        public class ShaderProperties
        {
#if UNITY_2022_2_OR_NEWER
            public System.Array m_Keywords;
            public System.Array m_Floats;
            public System.Array m_Vectors;
            public System.Array m_Matrices;
            public System.Array m_Textures;
            public System.Array m_Buffers;
            public System.Array m_CBuffers;

#else
            public System.Array floats;
            public System.Array vectors;
            public System.Array matrices;
            public System.Array textures;
            public System.Array buffers;
#endif

            // NonSerialized
            public List<ShaderPropertyInfo> convertedKeywords;
            public List<ShaderPropertyInfo> convertedFloats;
            public List<ShaderPropertyInfo> convertedVectors;
            public List<ShaderPropertyInfo> convertedMatricies;
            public List<ShaderPropertyInfo> convertedTextures;
            public List<ShaderPropertyInfo> convertedBuffers;
            public List<ShaderPropertyInfo> convertedCBuffers;
        }

        public class FrameDebuggerEventData
        {
#if UNITY_2022_2_OR_NEWER
            // inform
            public int m_FrameEventIndex;
            public int m_VertexCount;
            public int m_IndexCount;
            public int m_InstanceCount;
            public int m_DrawCallCount;
            public string m_OriginalShaderName;
            public string m_RealShaderName;
            public string m_PassName;
            public string m_PassLightMode;
            public int m_ShaderInstanceID;
            public int m_SubShaderIndex;
            public int m_ShaderPassIndex;

            public string shaderKeywords
            {
                get
                {
                    if (this.convertedProperties == null || this.convertedProperties.convertedKeywords == null)
                    {
                        return "";
                    }
                    StringBuilder sb = new StringBuilder();
                    foreach(var keywordInfo in convertedProperties.convertedKeywords)
                    {
                        sb.Append(keywordInfo.name).Append(" ");
                    }
                    return sb.ToString();
                }
            }


            public int m_ComponentInstanceID;
            public int m_MeshInstanceID;
            public int m_MeshSubset;

            // getter

            public int frameEventIndex { get { return m_FrameEventIndex; } }
            public int vertexCount { get { return m_VertexCount; } }
            public int indexCount { get { return m_IndexCount; } }
            public int instanceCount { get { return m_InstanceCount; } }
            public int drawCallCount { get { return m_DrawCallCount; } }
            public string shaderName { get { return m_RealShaderName; } }
            public string passName { get { return m_PassName; } }
            public string passLightMode { get { return m_PassLightMode; } }
            public int shaderInstanceID { get { return m_ShaderInstanceID; } }
            public int subShaderIndex { get { return m_SubShaderIndex; } }
            public int shaderPassIndex { get { return m_ShaderPassIndex; } }
            public int componentInstanceID { get { return m_ComponentInstanceID; } }
            public int meshInstanceID { get { return m_MeshInstanceID; } }
            public int meshSubset { get { return m_MeshSubset; } }

            // state for compute shader dispatches
            public int m_CsInstanceID;
            public string m_CsName;
            public string m_CsKernel;
            public int m_CsThreadGroupsX;
            public int m_CsThreadGroupsY;
            public int m_CsThreadGroupsZ;

            // getter
            public int csInstanceID { get { return m_CsInstanceID; } }
            public string csName { get { return m_CsName; } }
            public string csKernel { get { return m_CsKernel; } }
            public int csThreadGroupsX { get { return m_CsThreadGroupsX; } }
            public int csThreadGroupsY { get { return m_CsThreadGroupsY; } }
            public int csThreadGroupsZ { get { return m_CsThreadGroupsZ; } }

            // active render target info
            public string m_RenderTargetName;
            public int m_RenderTargetWidth;
            public int m_RenderTargetHeight;
            public int m_RenderTargetFormat;
            public int m_RenderTargetDimension;
            public int m_RtFace;
            public short m_RenderTargetCount;
            public short m_RenderTargetHasDepthTexture;
            // getter
            public string rtName { get { return m_RenderTargetName; } }
            public int rtWidth { get { return m_RenderTargetWidth; } }
            public int rtHeight { get { return m_RenderTargetHeight; } }
            public int rtFormat { get { return m_RenderTargetFormat; } }
            public int rtDim { get { return m_RenderTargetDimension; } }
            public int rtFace { get { return m_RtFace; } }
            public short rtCount { get { return m_RenderTargetCount; } }
            public short rtHasDepthTexture { get { return m_RenderTargetHasDepthTexture;} }




            public int m_BatchBreakCause;
            public object m_ShaderInfo;

            public object m_BlendState;
            public object m_RasterState;
            public object m_DepthState;
            public object m_StencilState;
            public int m_StencilRef;

            //geter
            public int batchBreakCause { get { return m_BatchBreakCause; } }
            public object shaderProperties { get { return m_ShaderInfo; } }

            public object blendState { get { return m_BlendState; } }
            public object rasterState { get { return m_RasterState; } }
            public object depthState { get { return m_DepthState; } }
            public object stencilState { get { return m_StencilState; } }
            public int stencilRef { get { return m_StencilRef; } }

            // non Serialized 
            public TextureUtility.SaveTextureInfo savedScreenShotInfo;
            public string batchBreakCauseStr;
            public ShaderProperties convertedProperties;

#else
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

            public object blendState;
            public object rasterState;
            public object depthState;
            public object stencilState;
            public int stencilRef;

            // non Serialized 
            public TextureUtility.SaveTextureInfo savedScreenShotInfo;
            public string batchBreakCauseStr;
            public ShaderProperties convertedProperties;
#endif
        }

        public class FrameDebuggerEvent
        {
#if UNITY_2022_2_OR_NEWER
            public object m_Type;
            public Object m_Obj;

            public object type { get { return m_Type; } }
            public Object gameObject { get { return m_Obj; } }
#else
            public object type;
            public GameObject gameObject;
#endif
        }

        private class SavedRenderTextureInfo
        {
            public int instanceId;
            public int lastChangedFrame;

            public SavedRenderTextureInfo( int instId,int lastChange)
            {
                this.instanceId = instId;
                this.lastChangedFrame = lastChange;
            }
            public override int GetHashCode()
            {
                return instanceId + lastChangedFrame;
            }
            public override bool Equals(object obj)
            {
                SavedRenderTextureInfo target = obj as SavedRenderTextureInfo;
                if( target == null) { return false; }
                return (this.instanceId == target.instanceId && this.lastChangedFrame == target.lastChangedFrame);
            }
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
#if UNITY_2021_2_OR_NEWER
        private ReflectionType frameDebugger;
#endif

        private float currentProgress;


        private ReflectionClassWithObject currentFrameEventData;
        private ReflectionClassWithObject frameDebuggerWindowObj;

        private string[] breakReasons;
        private CaptureFlag captureFlag;

        private Dictionary<int, TextureUtility.SaveTextureInfo> alreadyWriteTextureDict;

        private Dictionary<SavedRenderTextureInfo, TextureUtility.SaveTextureInfo> savedRenderTexture;
        private Dictionary<int, int> renderTextureLastChanged;


        public FrameInfoCrawler(ReflectionCache rcache)
        {
            this.reflectionCache = rcache;

#if UNITY_2022_2_OR_NEWER
            string frameDebuggerUtilName = "UnityEditorInternal.FrameDebuggerInternal.FrameDebuggerUtility";
            string frameEventDataName = "UnityEditorInternal.FrameDebuggerInternal.FrameDebuggerEventData";
#else
            string frameDebuggerUtilName = "UnityEditorInternal.FrameDebuggerUtility";
            string frameEventDataName = "UnityEditorInternal.FrameDebuggerEventData";
#endif

            this.frameDebuggeUtil = reflectionCache.GetTypeObject(frameDebuggerUtilName);
            this.frameEventData = reflectionCache.GetTypeObject(frameEventDataName);

#if UNITY_2021_2_OR_NEWER
            this.frameDebugger = reflectionCache.GetTypeObject(typeof(UnityEngine.FrameDebugger));
#endif

#if UNITY_2022_2_OR_NEWER
            string openWindowMethod = "OpenWindow";
#else
            string openWindowMethod = "ShowFrameDebuggerWindow";
#endif

            var frameDebuggerWindowType = this.reflectionCache.GetTypeObject("UnityEditor.FrameDebuggerWindow");
            var window = frameDebuggerWindowType.CallMethod<object>(openWindowMethod, null, null);
            this.frameDebuggerWindowObj = new ReflectionClassWithObject(frameDebuggerWindowType, window);

            this.IsRunning = false;
            this.alreadyWriteTextureDict = new Dictionary<int, TextureUtility.SaveTextureInfo>();
            this.savedRenderTexture = new Dictionary<SavedRenderTextureInfo, TextureUtility.SaveTextureInfo>();
            this.renderTextureLastChanged = new Dictionary<int, int>();
        }
        public void Request(CaptureFlag flag,System.Action callback)
        {
            if(this.IsRunning) { return; }
            this.captureFlag = flag;
            this.IsRunning = true;

            this.savedRenderTexture.Clear();
            this.alreadyWriteTextureDict.Clear();
            this.renderTextureLastChanged.Clear();


            var date = System.DateTime.Now;
            var dateString = string.Format("{0:D4}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}_", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
            var profilerName = ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler);

            this.saveDirectory = RootSaveDir + "/" + dateString + profilerName;
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
#if UNITY_2022_1_OR_NEWER
                this.frameDebuggerWindowObj.CallMethod<object>("ChangeFrameEventLimit", new object[] { i },new System.Type[] { typeof(int)});

#else
                this.frameDebuggerWindowObj.CallMethod<object>("ChangeFrameEventLimit",new object[] { i });
#endif
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

                bool isRemoteEnalbed = this.IsRemoteEnabled();
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
#if UNITY_2022_2_OR_NEWER
            props.convertedKeywords = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache, props.m_Keywords);
            props.convertedFloats = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache, props.m_Floats);
            props.convertedVectors = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache, props.m_Vectors);
            props.convertedMatricies = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache, props.m_Matrices);
            props.convertedTextures = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache, props.m_Textures);
            props.convertedBuffers = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache, props.m_Buffers);
            props.convertedCBuffers = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache, props.m_CBuffers);
#else
            props.convertedFloats = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache,props.floats);
            props.convertedVectors = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache, props.vectors);
            props.convertedMatricies = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache,props.matrices);
            props.convertedTextures = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache,props.textures);
            props.convertedBuffers = ReflectionClassWithObject.CopyToListFromArray<ShaderPropertyInfo>(this.reflectionCache,props.buffers);
#endif
        }

        private IEnumerator WaitForRemoteConnect(double deltaTime)
        {

            bool isRemoteEnalbed = this.IsRemoteEnabled();
            bool isReceiving = frameDebuggeUtil.GetPropertyValue<bool>("receivingRemoteFrameEventData", null);

            double startTime = EditorApplication.timeSinceStartup;
            // wait for remote data
            if (isRemoteEnalbed && isReceiving)
            {
                while ( (EditorApplication.timeSinceStartup - startTime) < deltaTime)
                {
                    this.frameDebuggerWindowObj.CallMethod<object>("RepaintOnLimitChange", null);
                    isRemoteEnalbed = this.IsRemoteEnabled();
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

                // save texture
                string dir = System.IO.Path.Combine(this.saveDirectory, "shaderTexture");
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
                if ( texture.GetType() == typeof(Texture2D))
                {
                    saveTextureInfo = SaveTexture2D((Texture2D) texture, dir);
                }
                if( texture.GetType() == typeof(RenderTexture))
                {
                    saveTextureInfo = SaveRenderTexture((RenderTexture)texture, dir);
                }
                textureParam.saveTextureInfo = saveTextureInfo;
            }
        }

        private TextureUtility.SaveTextureInfo SaveTexture2D(Texture2D texture,string dir)
        {
            TextureUtility.SaveTextureInfo saveTextureInfo = null;

            // already saved texture
            if (alreadyWriteTextureDict.TryGetValue(texture.GetInstanceID(), out saveTextureInfo))
            {
                return saveTextureInfo;
            }
            string path = System.IO.Path.Combine(dir, texture.name +"_" +texture.GetInstanceID() ); ;
            saveTextureInfo = TextureUtility.SaveTexture((Texture2D)texture, path);
            alreadyWriteTextureDict.Add(texture.GetInstanceID(), saveTextureInfo);
            return saveTextureInfo;
        }
        private TextureUtility.SaveTextureInfo SaveRenderTexture(RenderTexture texture, string dir)
        {
            TextureUtility.SaveTextureInfo saveTextureInfo = null;
            int renderTextureChangedIdx = -1;
            renderTextureLastChanged.TryGetValue(texture.GetInstanceID(), out renderTextureChangedIdx);
            SavedRenderTextureInfo savedRTInfo = new SavedRenderTextureInfo(texture.GetInstanceID(), renderTextureChangedIdx);
            // not saved
            if (!this.savedRenderTexture.TryGetValue(savedRTInfo, out saveTextureInfo))
            {
                string path = System.IO.Path.Combine(dir, "RT_" + renderTextureChangedIdx + "_" + texture.name);
                saveTextureInfo = TextureUtility.SaveRenderTexture((RenderTexture)texture, path);
                savedRenderTexture.Add(savedRTInfo, saveTextureInfo);
            }
            return saveTextureInfo;

        }

        private bool IsRemoteEnabled()
        {
#if UNITY_2021_2_OR_NEWER
            return frameDebugger.CallMethod<bool>("IsRemoteEnabled", null, null);
#else
            return frameDebuggeUtil.CallMethod<bool>("IsRemoteEnabled", null, null);
#endif
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
            if(renderTexture == null)
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
                frameInfo.savedScreenShotInfo = TextureUtility.SaveRenderTexture(renderTexture, path);                
            }
        }
        
    }
}