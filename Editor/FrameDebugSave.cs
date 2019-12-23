using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace UTJ.FrameDebugSave
{
    public class FrameDebugSave
    {
        private static FrameInfoCrawler crawler;

        [MenuItem("Tools/FrameDebuggerSave")]
        public static void Execute()
        {
            var r = new ReflectionCache();
            var frameDebuggeUtil = r.GetTypeObject("UnityEditorInternal.FrameDebuggerUtility");

            // show FrameDebuggerWindow
            var frameDebuggerWindow = r.GetTypeObject("UnityEditor.FrameDebuggerWindow");
            var windowObj= frameDebuggerWindow.CallMethod<object>("ShowFrameDebuggerWindow", null, null);
            frameDebuggerWindow.CallMethod<object>("EnableIfNeeded", windowObj, null);
            if (crawler == null)
            {
                crawler = new FrameInfoCrawler();
                crawler.Setup(EndCrawler);
            }
        }


        private static void EndCrawler()
        {
            crawler = null;
        }
        
    }

}