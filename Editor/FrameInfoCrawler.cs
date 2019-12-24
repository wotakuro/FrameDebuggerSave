using System.Collections;
using System;
using UnityEditor;

namespace UTJ.FrameDebugSave
{
    public class FrameInfoCrawler
    {
        public struct FrameInfo
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

            public string batchBreakCauseStr;
        }

        private IEnumerator enumerator;
        private System.Action endCallback;

        private ReflectionCache reflectionCache;
        private ReflectionType frameDebuggeUtil;
        private ReflectionType frameEventData;
        private float currentProgress;


        private ReflectionClassWithObject currentFrameEventData;
        private ReflectionClassWithObject frameDebuggerWindowObj;

        private string[] breakReasons;

        public FrameInfoCrawler()
        {
            this.reflectionCache = new ReflectionCache();
            this.frameDebuggeUtil = reflectionCache.GetTypeObject("UnityEditorInternal.FrameDebuggerUtility");
            this.frameEventData = reflectionCache.GetTypeObject("UnityEditorInternal.FrameDebuggerEventData");

            var frameDebuggerWindowType = this.reflectionCache.GetTypeObject("UnityEditor.FrameDebuggerWindow");
            var window = frameDebuggerWindowType.CallMethod<object>("ShowFrameDebuggerWindow", null, null);
            this.frameDebuggerWindowObj = new ReflectionClassWithObject(frameDebuggerWindowType, window);
        }
        public void Setup(System.Action callback)
        {
            endCallback = callback;
            EditorApplication.update += Update;
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
                EditorApplication.update -= Update;
            }
        }

        public IEnumerator Execute()
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




            for ( int i = 0; i <= count; ++i)
            {
                yield return null;
                //                this.frameDebuggeUtil.SetPropertyValue( "limit",null, i);
                this.frameDebuggerWindowObj.CallMethod<object>("ChangeFrameEventLimit",new object[] { i });
                this.frameDebuggerWindowObj.CallMethod<object>("RepaintOnLimitChange",null);
                int targetFrameIdx = i - 1;
                if(targetFrameIdx < 0 || targetFrameIdx >= frameEvents.Length) { continue; }
                // wait for remote dataveNext()) { }

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

                FrameInfo frameInfo = new FrameInfo();
                frameData.CopyFieldsToObjectByVarName<FrameInfo>(ref frameInfo);
                frameInfo.batchBreakCauseStr = breakReasons[frameInfo.batchBreakCause];


                currentProgress = i / (float)count;
                UnityEngine.Debug.Log(frameInfo.frameEventIndex + "  " +
                    frameInfo.shaderName + "\n" +
                    frameInfo.passName + "\n" +
                    frameInfo.shaderKeywords + "\n" +
                    frameInfo.batchBreakCauseStr);
            }
            yield return null;
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
            var args = new object[] { frameIdx, null };
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

    }
}