using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Text;

namespace UTJ.FrameDebugSave
{
    public class FrameDebugSaveWindow : EditorWindow
    {
        private FrameInfoCrawler.CaptureFlag captureFlag;

        private FameDebugSave frameDebugSave = new FameDebugSave();


        [MenuItem("Tools/FrameDebuggerSave")]
        public static void CreateWindow()
        {
            EditorWindow.GetWindow<FrameDebugSaveWindow>();
        }

        private void OnEnable()
        {
        }

        private void OnGUI()
        {
            captureFlag = (FrameInfoCrawler.CaptureFlag)EditorGUILayout.EnumFlagsField("CaptureFlag", captureFlag);
            if (GUILayout.Button("Capture via FrameDebugger"))
            {
                frameDebugSave.Execute(this.captureFlag);
            }
        }
    }
}