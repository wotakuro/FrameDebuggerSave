using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;

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
            var dateString = string.Format("{0:D4}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}_", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
            var profilerStr = ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler);
            string dirPath = "FrameDebugger/" + dateString + profilerStr;
            // directory
            Directory.CreateDirectory(dirPath);

            CsvStringGenerator csvStringGenerator = new CsvStringGenerator();
            csvStringGenerator.AppendColumn("frameEventIndex");

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


            foreach( var evt in crawler.frameDebuggerEventDataList)
            {

                csvStringGenerator.AppendColumn(evt.frameEventIndex);

                csvStringGenerator.AppendColumn(evt.rtName).
                    AppendColumn(evt.rtWidth).AppendColumn(evt.rtHeight).
                    AppendColumn(evt.rtCount).AppendColumn(evt.rtHasDepthTexture);

                csvStringGenerator.AppendColumn(evt.vertexCount).
                    AppendColumn(evt.indexCount).
                    AppendColumn(evt.instanceCount).
                    AppendColumn(evt.drawCallCount).
                    AppendColumn(evt.shaderName).
                    AppendColumn(evt.passName).
                    AppendColumn(evt.passLightMode).
                    AppendColumn(evt.subShaderIndex).
                    AppendColumn(evt.shaderPassIndex).
                    AppendColumn(evt.shaderKeywords).
                    AppendColumn(evt.componentInstanceID).
                    AppendColumn(evt.meshInstanceID).
                    AppendColumn(evt.meshSubset);


                csvStringGenerator.AppendColumn(evt.batchBreakCauseStr);
                csvStringGenerator.NextRow();
            }
            File.WriteAllText(Path.Combine(dirPath, "events.csv"), csvStringGenerator.ToString());

            crawler = null;
        }
        
    }

}