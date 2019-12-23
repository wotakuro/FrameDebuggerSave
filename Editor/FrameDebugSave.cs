using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace UTJ.FrameDebugSave
{
    public class FrameDebugSave : EditorWindow
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

            var date = System.DateTime.Now;
            var dateString = string.Format("{0:D4}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}", date.Year, date.Month, date.Day, date.Hour, date.Minute,date.Second);

            crawler = null;
        }
        
    }

}