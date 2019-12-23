using System.Collections;
using System;
using UnityEditor;

namespace UTJ.FrameDebugSave
{
    public class FrameInfoCrawler
    {
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
            yield return null;
            yield return null;

            var r = this.reflectionCache;
            var frameEvents = frameDebuggeUtil.CallMethod<System.Array>( "GetFrameEvents", null, null);
            var breakReasons = frameDebuggeUtil.CallMethod<string[]>("GetBatchBreakCauseStrings", null, null);
            var frameEventDatas = r.GetTypeObject("UnityEditorInternal.FrameDebuggerEventData");


            var evtData = frameDebuggeUtil.CreateInstance();
            int count = frameDebuggeUtil.GetPropertyValue<int>( "count", null);
            UnityEngine.Debug.Log("count " + count);



            for ( int i = 0; i <= count; ++i)
            {
                yield return null;
                //                this.frameDebuggeUtil.SetPropertyValue( "limit",null, i);
                this.frameDebuggerWindowObj.CallMethod<object>("ChangeFrameEventLimit",new object[] { i });
                this.frameDebuggerWindowObj.CallMethod<object>("RepaintOnLimitChange",null);
                int targetFrameIdx = i - 1;
                if(targetFrameIdx < 0 || targetFrameIdx >= frameEvents.Length) { continue; }
                var eventDataCoroutine = this.TryGetFrameEvnetData(targetFrameIdx, 3);
                while (eventDataCoroutine.MoveNext())
                {
                    yield return null;
                }
                var frameData = currentFrameEventData;
                if( frameData == null) {
                    UnityEngine.Debug.LogWarning("failed capture " + targetFrameIdx);
                    continue;
                }

                int frameIdx = frameData.GetFieldValue<int>("frameEventIndex");
                string shaderName = frameData.GetFieldValue<string>("shaderName");
                string passName = frameData.GetFieldValue<string>("passName");
                string shaderKeywords = frameData.GetFieldValue<string>("shaderKeywords");
                int reasonIdx = frameData.GetFieldValue<int>("batchBreakCause"); 

                currentProgress = i / (float)count;
                UnityEngine.Debug.Log(frameIdx +"  " + shaderName + "\n" + passName + "\n" + shaderKeywords + "\n" + breakReasons[reasonIdx]);
            }
            yield return null;
        }

        private IEnumerator TryGetFrameEvnetData(int frameIdx,int tryNum = 3 )
        {
            int limit = frameDebuggeUtil.GetPropertyValue<int>("limit", null);
            for (int i = 0; i < tryNum; ++i)
            {
                bool res = GetFrameEventData(frameIdx,out this.currentFrameEventData);
                if (!res)
                {
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
                ret = new ReflectionClassWithObject(frameEventData, args[1]);
                return true;
            }
            return result;
        }

    }
}