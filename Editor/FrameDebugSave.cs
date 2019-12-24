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
            string dirPath =  crawler.saveDirectory;
            // directory

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


            for( int i = 0; i< crawler.frameDebuggerEventDataList.Count;++i )
            {
                var evtData = crawler.frameDebuggerEventDataList[i];

                csvStringGenerator.AppendColumn(evtData.frameEventIndex);

                var evt = crawler.frameDebuggerEventList[i];
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

            EditorUtility.DisplayDialog("Saved", dirPath, "ok");
            crawler = null;
        }
        
    }

}