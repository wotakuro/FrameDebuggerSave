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
        private static readonly string[] shaderParamTypes = { "Texture", "Float", "Vector", "Matrix" };

        private FrameInfoCrawler.CaptureFlag captureFlag;
        private FameDebugSave frameDebugSave = new FameDebugSave();

        private VisualTreeAsset shaderParamTemplate;
        private string currentLoadPath;


        [MenuItem("Tools/FrameDebugSave")]
        public static void Create()
        {
            EditorWindow.GetWindow<FrameEventDebugSaveWindow>();
        }
        private void OnEnable()
        {
            string windowLayoutPath = "Packages/com.utj.framedebugger2csv/Editor/UI/UXML/FrameEventsViewer.uxml";
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(windowLayoutPath);
            var visualElement = tree.CloneTree();

            this.rootVisualElement.Add(visualElement);

            string shaderParamPath = "Packages/com.utj.framedebugger2csv/Editor/UI/UXML/ShaderParameterTemplate.uxml";
            this.shaderParamTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(shaderParamPath);

            this.RefreshCaptures();
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
                    var elem = CreateShaderParamVE(textureParam);
                    parentElement.Add(elem);
                }
            }
            if (shaderParams.floats != null)
            {
                foreach (var floatParam in shaderParams.floats)
                {
                    var elem = CreateShaderParamVE(floatParam);
                    parentElement.Add(elem);                    
                }
            }
            if (shaderParams.vectors != null)
            {
                foreach (var vectorParam in shaderParams.vectors)
                {
                    var elem = CreateShaderParamVE(vectorParam);
                    parentElement.Add(elem);
                }
            }
            if (shaderParams.matricies != null)
            {
                foreach (var matrixParam in shaderParams.matricies)
                {
                    var elem = CreateShaderParamVE(matrixParam);
                    parentElement.Add(elem);
                }
            }

            var screenShotTex = FrameDebugDumpInfo.LoadTexture(this.currentLoadPath, evtInfo.screenshot);

            this.rootVisualElement.Q<VisualElement>("ScreenShot").style.backgroundImage = screenShotTex;
        }
        
        private VisualElement CreateShaderParamVE(FrameDebugDumpInfo.TextureParamInfo textureParam)
        {
            var element = this.shaderParamTemplate.CloneTree();
            var valElem = InitShaderParamValueElement(element, "Texture",textureParam.name);
            var tex = FrameDebugDumpInfo.LoadTexture(this.currentLoadPath, textureParam.saved);
            var texBody = valElem.Q<VisualElement>("texbody");
            var info = valElem.Q<Label>("val");
            if (tex != null)
            {
                texBody.style.backgroundImage = tex;
            }
            else
            {
                texBody.parent.Remove(texBody);
            }
            info.text = textureParam.textureName;
            return element;
        }

        private VisualElement CreateShaderParamVE(FrameDebugDumpInfo.FloatParamInfo floatParam)
        {
            var element = this.shaderParamTemplate.CloneTree();
            var valElem = InitShaderParamValueElement(element, "Float", floatParam.name);
            valElem.Q<Label>("val").text = floatParam.val.ToString();
            return element;
        }
        private VisualElement CreateShaderParamVE(FrameDebugDumpInfo.VectorParamInfo vectorParam)
        {
            var element = this.shaderParamTemplate.CloneTree();
            var valElem = InitShaderParamValueElement(element, "Vector", vectorParam.name);
            for( int i = 0; i < 4; ++i)
            {
                valElem.Q<Label>("val-"+i).text = vectorParam.val[i].ToString();
            }
            return element;
        }
        private VisualElement CreateShaderParamVE(FrameDebugDumpInfo.MatrixParamInfo matrixParam)
        {
            var element = this.shaderParamTemplate.CloneTree();
            var valElem = InitShaderParamValueElement(element, "Matrix", matrixParam.name);
            for (int i = 0; i < 16; ++i)
            {
                valElem.Q<Label>("val-" + i).text = matrixParam.val[i].ToString();
            }
            return element;
        }

        private VisualElement InitShaderParamValueElement(VisualElement elem,string typeStr,string name)
        {
            VisualElement retNode = null;
            elem.Q<Label>("name").text = name;
            foreach( var type in shaderParamTypes)
            {
                var currentVE = elem.Q<VisualElement>(type);
                if ( type == typeStr)
                {
                    retNode = currentVE;
                }
                else
                {
                    currentVE.parent.Remove(currentVE);
                }
            }
            return retNode;
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
            this.currentLoadPath = System.IO.Path.GetDirectoryName(file);

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
            if( !System.IO.Directory.Exists(FrameInfoCrawler.RootSaveDir))
            {
                return captures;
            }
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
                var view = ve as FrameEventListView;
                if( view != null) { view.Initialize(); }
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

        void Initialize()
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
