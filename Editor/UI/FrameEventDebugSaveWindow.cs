using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityEngine.UIElements;
using UTJ.FrameDebugSave;
using UnityEditor.UIElements;


namespace UTJ.FrameDebugSave.UI
{

    public class FrameEventDebugSaveWindow : EditorWindow
    {
        private FrameInfoCrawler.CaptureFlag captureFlag;

        private FameDebugSave frameDebugSave = new FameDebugSave();

        private VisualElement textureParamElement;
        private VisualElement floatParamElement;
        private VisualElement vectorParamElement;
        private VisualElement matrixParamElement;


        [MenuItem("Tools/FrameDebugSave")]
        public static void Create()
        {
            EditorWindow.GetWindow<FrameEventDebugSaveWindow>();
        }
        private void OnEnable()
        {
            string path = "Packages/com.utj.framedebugger2csv/Editor/UI/UXML/FrameEventsViewer.uxml";
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            var visualElement = tree.CloneTree();

            this.rootVisualElement.Add(visualElement);
            visualElement.Q<FrameEventListView>().Initialize();
            this.InitShaderParamElements();

            this.RefreshCaptures();
        }

        private void InitShaderParamElements()
        {
            this.textureParamElement = this.rootVisualElement.Q<VisualElement>("TextureValueTemplate");
            this.floatParamElement = this.rootVisualElement.Q<VisualElement>("FloatValueTemplate");
            this.vectorParamElement = this.rootVisualElement.Q<VisualElement>("VectorValueTemplate");
            this.matrixParamElement = this.rootVisualElement.Q<VisualElement>("MatrixValueTemplate");
            // remove from parent
            this.textureParamElement.parent.Remove(this.textureParamElement);
            this.floatParamElement.parent.Remove(this.floatParamElement);
            this.vectorParamElement.parent.Remove(this.vectorParamElement);
            this.matrixParamElement.parent.Remove(this.matrixParamElement);
        }

        private void OnFrameEventChange(FrameDebugDumpInfo.FrameEventInfo evtInfo)
        {
            var shaderParams = evtInfo.shaderInfo.shaderParams;
            var parentElement = this.rootVisualElement.Q<Foldout>("ShaderParameters");
            RemoveAllChildren(parentElement);

            if (shaderParams.textures != null)
            {
                foreach (var textureParam in shaderParams.textures)
                {
                    var label = new Label(textureParam.name + " " + textureParam.textureName);
                    parentElement.Add(label);
                }
            }
            if (shaderParams.floats != null)
            {
                foreach (var floatParam in shaderParams.floats)
                {
                    var label = new Label(floatParam.name + " " + floatParam.val);
                    parentElement.Add(label);
                }
            }
            if (shaderParams.vectors != null)
            {
                foreach (var vectorParam in shaderParams.vectors)
                {
                    string str = vectorParam.name + " ";
                    for(int i = 0; i < vectorParam.val.Length; ++i)
                    {
                        str += vectorParam.val[i] + ",";
                    }
                    var label = new Label(str);
                    parentElement.Add(label);
                }
            }
        }

        private void RemoveAllChildren(VisualElement elem)
        {
            for( int i = elem.childCount -1; i>=0 ; --i)
            {
                elem.RemoveAt(i);
            }
        }

        private void LoadCaptureFile(string file)
        {
            string detail = file;
            var t = UTJ.FrameDebugSave.FrameDebugDumpInfo.LoadFromFile(detail);
            var eventListView = this.rootVisualElement.Q<FrameEventListView>();
            eventListView.OnChangedFrame = this.OnFrameEventChange;
            eventListView.SetItems(t.events);
        }

        private void RefreshCaptures()
        {
            var captures = GetCaptures();
            var scrollView = this.rootVisualElement.Q<ScrollView>("CaptureItems");
            scrollView.Clear();
            scrollView.Add(new IMGUIContainer(OnGUINewCapture));

            foreach (var capture in captures) {
                var btn = new Button();
                btn.text = capture;
                btn.clickable.clicked += ()=>{
                    LoadCaptureFile(FrameInfoCrawler.RootSaveDir + "/" + capture + "/detail.json");
                };
                scrollView.Add(btn);
             }
        }

        private void OnGUINewCapture()
        {
            captureFlag = (FrameInfoCrawler.CaptureFlag)EditorGUILayout.EnumFlagsField( captureFlag);
            if (GUILayout.Button("new Capture"))
            {
                frameDebugSave.Execute(this.captureFlag,this.RefreshCaptures);
            }
            GUILayout.Label("");

        }

        private List<string> GetCaptures()
        {
            List<string> captures = new List<string>();
            var dirs = System.IO.Directory.GetDirectories(FrameInfoCrawler.RootSaveDir);
            foreach (var dir in dirs){
                captures.Add(dir.Replace(FrameInfoCrawler.RootSaveDir, ""));
            }
            return captures;
        }


    }

    public class FrameEventListView : VisualElement
    {
        private ScrollView scrollView;
        private List<UIPair> uiDataBind;
        private int currentIdx = 0;

        public delegate void ChangedFrameEvent (FrameDebugDumpInfo.FrameEventInfo evtInfo);
        public FrameDebugDumpInfo.FrameEventInfo selectFrameInfo
        {
            get
            {
                return uiDataBind[currentIdx].eventInfo;
            }
        }

        public ChangedFrameEvent OnChangedFrame
        {
            set;get;
        }


        // UXMLからオブジェクトを生成するためのファクトリー
        public new class UxmlFactory : UxmlFactory<FrameEventListView, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            //ここで独自のAttributeを指定します
            UxmlStringAttributeDescription assetPathAttr =
                new UxmlStringAttributeDescription { name = "assetPath" };

            // 子要素を持つかどうか
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
            // パース時に呼び出されます
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
            }
        }


        private struct UIPair
        {
            public FrameDebugDumpInfo.FrameEventInfo eventInfo;
            public VisualElement element;

            public UIPair(FrameDebugDumpInfo.FrameEventInfo info , VisualElement elem)
            {
                this.eventInfo = info;
                this.element = elem;
            }
        }

        public void Initialize()
        {
            scrollView = new ScrollView();
            this.Add(scrollView);
        }
        public void SetItems(FrameDebugDumpInfo.FrameEventInfo[] infos)
        {
            this.uiDataBind = new List<UIPair>();
            this.scrollView.Clear();
            foreach( var info in infos) { 
                var label = new Label(info.frameEventIndex.ToString() + " " + info.type);
                label.focusable = true;
                label.RegisterCallback<MouseDownEvent>((evt) =>
                {
                    SetCurrentFrameEvent(label);
                });
                label.RegisterCallback<KeyDownEvent>((evt) =>
                {
                    if (evt.keyCode == KeyCode.UpArrow)
                    {
                        SetSelectedIndex(this.currentIdx - 1);
                    }else if(evt.keyCode == KeyCode.DownArrow)
                    {
                        SetSelectedIndex(this.currentIdx + 1);
                    }
                });
                this.scrollView.Add(label);
                UIPair uiPair = new UIPair(info, label);
                this.uiDataBind.Add(uiPair);
            }
            SetSelectedIndex(infos.Length - 1);
        }


        private void SetCurrentFrameEvent(VisualElement visualElement)
        {
            for( int i = 0; i < this.uiDataBind.Count; ++i)
            {
                if( uiDataBind[i].element == visualElement)
                {
                    SetSelectedIndex(i);
                    break;
                }
            }
        }


        private void SetSelectedIndex(int idx)
        {
            if( idx < 0 || idx >= this.uiDataBind.Count)
            {
                return;
            }
            if(idx == currentIdx) { return; }
            var oldElem = this.uiDataBind[this.currentIdx].element;
            oldElem.style.backgroundColor = new Color(0, 0, 0, 0);

            var newElem = this.uiDataBind[idx].element;
            newElem.style.backgroundColor = Color.blue;
            newElem.Focus();

            this.currentIdx = idx;

            this.scrollView.ScrollTo(newElem);
            if(OnChangedFrame != null)
            {
                OnChangedFrame(this.uiDataBind[idx].eventInfo);
            }
        }
        
    }
}
