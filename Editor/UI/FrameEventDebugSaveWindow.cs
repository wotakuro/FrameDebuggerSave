using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UTJ.FrameDebugSave;
#if UNITY_2019_1_OR_NEWER || UNITY_2019_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif


namespace UTJ.FrameDebugSave.UI
{

    public class FrameEventDebugSaveWindow : EditorWindow
    {
        private static readonly string[] shaderParamTypes = { "Texture", "Float", "Vector", "Matrix" };

        private FrameInfoCrawler.CaptureFlag captureFlag;
        private FameDebugSave frameDebugSave = new FameDebugSave();

        private VisualTreeAsset shaderParamTemplate;
        private VisualTreeAsset namedValueParamTemplate;
        private TextureLoader textureLoader;

        private VisualTreeAsset screenshotTemplate;


        private ShaderVariantCollection currentVariantCollection;

        private ShaderVariantCollectionCreator.EFlag variantFlag;


        [MenuItem("Tools/FrameDebugSave")]
        public static void Create()
        {
            EditorWindow.GetWindow<FrameEventDebugSaveWindow>();
        }
        private void OnEnable()
        {
            string windowLayoutPath = "Packages/com.utj.framedebuggersave/Editor/UI/UXML/FrameEventsViewer.uxml";
            string screenShotPath = "Packages/com.utj.framedebuggersave/Editor/UI/UXML/ScreenshotTemplate.uxml";
            string shaderParamPath = "Packages/com.utj.framedebuggersave/Editor/UI/UXML/ShaderParameterTemplate.uxml";
            string namedValuePath = "Packages/com.utj.framedebuggersave/Editor/UI/UXML/NamedValueTemplate.uxml";

            this.variantFlag = (ShaderVariantCollectionCreator.EFlag)(-1);
            this.captureFlag = (FrameInfoCrawler.CaptureFlag)(-1);


            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(windowLayoutPath);
            var visualElement = CloneTree(tree);

            this.rootVisualElement.Add(visualElement);
            this.screenshotTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(screenShotPath);

            this.textureLoader = new TextureLoader();
            this.shaderParamTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(shaderParamPath);
            this.namedValueParamTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(namedValuePath);

#if !UNITY_2019_1_OR_NEWER && !UNITY_2019_OR_NEWER
            InitIMGUIArea();
#else
            this.InitNewCaptureUI();
            this.InitShaderVariantCollectionUI();
#endif
            this.RefreshCaptures();
        }




        private void OnFrameEventChange(FrameDebugDumpInfo.FrameEventInfo evtInfo)
        {
            /*
            var screenShotTex = textureLoader.LoadTexture(evtInfo.screenshots[0]);
            this.rootVisualElement.Q<ScalableImageView>("ScreenShot").SetTexture( screenShotTex );
            */

            //ChangeRenderTargetInfo(evtInfo);
            ChangeRTInfo(evtInfo);
            ChangeRenderInfo(evtInfo);
            ChangeShaderInfo(evtInfo);
            ChangeShaderParam(evtInfo);
        }

        private void ChangeRTInfo(FrameDebugDumpInfo.FrameEventInfo evtInfo)
        {
            var scrollElem = this.rootVisualElement.Q<ScrollView>("ScreenshotScroll");
            scrollElem.Clear();
            if (evtInfo.screenshot != null &&
                evtInfo.screenshot.width != 0 && evtInfo.screenshot.height != 0)
            {
                scrollElem.Add( CreateScreenshotInfoView(evtInfo.screenshot) );
                return;
            }
            if( evtInfo.screenshots != null){
                foreach(var screenshot in evtInfo.screenshots)
                {
                    scrollElem.Add(CreateScreenshotInfoView(screenshot));
                }
            }
        }


        private VisualElement CreateScreenshotInfoView(FrameDebugDumpInfo.SavedTextureInfo savedTexInfo)
        {
            var screenShotTex = textureLoader.LoadTexture(savedTexInfo);
            var element = this.screenshotTemplate.CloneTree();
            var imageView = element.Q<ScalableImageView>("ScreenShot");

            imageView.SetTexture(screenShotTex);

            var parentElement = element.Q<Foldout>("RenderTargetInfo");
            parentElement.Add(CreateNameValueElement("RT Name", "name"));
            parentElement.Add(CreateNameValueElement("Size", savedTexInfo.width + " X " + savedTexInfo.height));
            parentElement.Add(CreateNameValueElement("MipCount", savedTexInfo.mipCount.ToString()));
#if UNITY_2020_2_OR_NEWER
            parentElement.Add(CreateNameValueElement("Depth", savedTexInfo.originFormat.ToString()));
#endif
            return element;
        }


        private void ChangeRenderTargetInfo(FrameDebugDumpInfo.FrameEventInfo evtInfo)
        {
            var parentElement = this.rootVisualElement.Q<Foldout>("RenderTargetInfo");
            RemoveAllChildren(parentElement);
            var rtInfo = evtInfo.renderTarget;
            parentElement.Add(CreateNameValueElement("RT Name", rtInfo.rtName));
            parentElement.Add(CreateNameValueElement("Size", rtInfo.rtWidth + " X " + rtInfo.rtHeight));
            parentElement.Add(CreateNameValueElement("Count", rtInfo.rtCount.ToString()));
            parentElement.Add(CreateNameValueElement("Depth", (rtInfo.rtHasDepthTexture >0).ToString()));

        }

        private void ChangeRenderInfo(FrameDebugDumpInfo.FrameEventInfo evtInfo)
        {
            var parentElement = this.rootVisualElement.Q<Foldout>("RenderingInfo");
            RemoveAllChildren(parentElement);
            var renderInfo = evtInfo.rendering;


            parentElement.Add(CreateNameValueElement("BreakReson", renderInfo.batchBreakCauseStr));
            parentElement.Add(CreateNameValueElement("Vertex", renderInfo.vertexCount.ToString()));
            parentElement.Add(CreateNameValueElement("Index", renderInfo.indexCount.ToString()));
            parentElement.Add(CreateNameValueElement("DrawCount", renderInfo.drawCallCount.ToString()));
            parentElement.Add(CreateNameValueElement("GameObject", renderInfo.gameobject));
            parentElement.Add(CreateNameValueElement("ComponentId", renderInfo.componentInstanceID.ToString()));
            parentElement.Add(CreateNameValueElement("MeshInstanceId", renderInfo.meshInstanceID.ToString()));
            parentElement.Add(CreateNameValueElement("InstanceCount", renderInfo.instanceCount.ToString()));
            parentElement.Add(CreateNameValueElement("MeshSubset", renderInfo.meshSubset.ToString()));


        }
        private void ChangeShaderInfo(FrameDebugDumpInfo.FrameEventInfo evtInfo)
        {
            var parentElement = this.rootVisualElement.Q<Foldout>("ShaderInfo");
            var shaderInfo = evtInfo.shaderInfo;
            RemoveAllChildren(parentElement);

            parentElement.Add(CreateNameValueElement("ShaderName", shaderInfo.shaderName));
            parentElement.Add(CreateNameValueElement("SubShader", shaderInfo.subShaderIndex.ToString()));
            parentElement.Add(CreateNameValueElement("Keyword", shaderInfo.shaderKeywords));
            parentElement.Add(CreateNameValueElement("Pass", shaderInfo.passName + "(" + shaderInfo.shaderPassIndex + ")"));
            parentElement.Add(CreateNameValueElement("LightMode", shaderInfo.passLightMode));
        }

        private VisualElement CreateNameValueElement(string name,string val)
        {
            var tree = CloneTree(namedValueParamTemplate);
            tree.Q<Label>("name").text = name;
            tree.Q<Label>("val").text = val;
            return tree;
        }

        private void ChangeShaderParam(FrameDebugDumpInfo.FrameEventInfo evtInfo)
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
                    if(floatParam.name == "<noninit>"){
                        continue;
                    }
                    var elem = CreateShaderParamVE(floatParam);
                    parentElement.Add(elem);
                }
            }
            if (shaderParams.vectors != null)
            {
                foreach (var vectorParam in shaderParams.vectors)
                {
                    if (vectorParam.name == "<noninit>")
                    {
                        continue;
                    }
                    var elem = CreateShaderParamVE(vectorParam);
                    parentElement.Add(elem);
                }
            }
            if (shaderParams.matricies != null)
            {
                foreach (var matrixParam in shaderParams.matricies)
                {
                    if (matrixParam.name == "<noninit>") {
                        continue;
                    }
                    var elem = CreateShaderParamVE(matrixParam);
                    parentElement.Add(elem);
                }
            }

        }

        private VisualElement CreateShaderParamVE(FrameDebugDumpInfo.TextureParamInfo textureParam)
        {
            var element = CloneTree(this.shaderParamTemplate);
            var valElem = InitShaderParamValueElement(element, "Texture",textureParam.name);
            var tex = textureLoader.LoadTexture(textureParam.saved);
            var texBody = valElem.Q<ScalableImageView>("texbody");
            var info = valElem.Q<Label>("val");
            if (tex != null)
            {
                texBody.SetTexture(tex);
            }
            else
            {
                texBody.parent.Remove(texBody);
            }
            info.text = textureParam.textureName + "\n" +
                textureParam.originWidth + " X " + textureParam.originHeight + "\n" +
                textureParam.originFormat + "\n" +
                "mipcount:" + textureParam.originMipCount;
            return element;
        }

        private VisualElement CreateShaderParamVE(FrameDebugDumpInfo.FloatParamInfo floatParam)
        {
            var element = CloneTree(this.shaderParamTemplate);
            var valElem = InitShaderParamValueElement(element, "Float", floatParam.name);
            valElem.Q<Label>("val").text = floatParam.val.ToString();
            return element;
        }
        private VisualElement CreateShaderParamVE(FrameDebugDumpInfo.VectorParamInfo vectorParam)
        {
            var element = CloneTree(this.shaderParamTemplate);
            var valElem = InitShaderParamValueElement(element, "Vector", vectorParam.name);
            for( int i = 0; i < 4; ++i)
            {
                valElem.Q<Label>("val-"+i).text = vectorParam.val[i].ToString();
            }
            return element;
        }
        private VisualElement CreateShaderParamVE(FrameDebugDumpInfo.MatrixParamInfo matrixParam)
        {
            var element = CloneTree(this.shaderParamTemplate);
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
            this.textureLoader.SetDirectory(System.IO.Path.GetDirectoryName(file));

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
            
            foreach (var capture in captures) {
                var btn = new Button();
                btn.text = capture;
                btn.clickable.clicked += ()=>{
                    LoadCaptureFile(FrameInfoCrawler.RootSaveDir + "/" + capture + "/detail.json");
                };
                scrollView.Add(btn);
             }
        }

        private void InitShaderVariantCollectionUI()
        {
            var enumField = this.rootVisualElement.Q<EnumFlagsField>("VariantFlag");
            enumField.Init(this.variantFlag);
            enumField.RegisterValueChangedCallback((val) =>
            {
                this.variantFlag = (ShaderVariantCollectionCreator.EFlag)val.newValue;
            });

            var objectField = this.rootVisualElement.Q<ObjectField>("VariantCollectionObject");
            objectField.objectType = typeof(ShaderVariantCollection);
            

            objectField.RegisterValueChangedCallback((obj) =>
            {
                currentVariantCollection = obj.newValue as ShaderVariantCollection;
            });
            var btn = this.rootVisualElement.Q<Button>("AddToCollectionBtn");
            btn.clickable.clicked += ExecuteAddShaderVariantCollection;
        }

        private void InitNewCaptureUI()
        {
            var enumField = this.rootVisualElement.Q<EnumFlagsField>("CaptureFlag");
            enumField.Init(this.captureFlag);
            enumField.RegisterValueChangedCallback((val) =>
            {
                this.captureFlag = (FrameInfoCrawler.CaptureFlag)val.newValue;
            });
            var btn = this.rootVisualElement.Q<Button>("CaptureBtn");
            btn.clickable.clicked += () =>
            {
                frameDebugSave.Execute(this.captureFlag, ()=> {
                    EditorUtility.DisplayDialog("Saved", "Saved FrameDebugger Information", "ok");
                    this.RefreshCaptures();
                });
            };
        }

        private void ExecuteAddShaderVariantCollection()
        {
            if( this.currentVariantCollection == null)
            {
                var file = EditorUtility.SaveFilePanelInProject("Create ShaderVariant", "ShaderVariantCollection", "shadervariants","please set create shadervariants");
                if( string.IsNullOrEmpty(file))
                {
                    return;
                }
                ShaderVariantCollection data = new ShaderVariantCollection();
                ShaderVariantCollectionCreator.AddFromScannedData(data, variantFlag);
                AssetDatabase.CreateAsset(data, file);
                //                
                EditorUtility.DisplayDialog("Complete", "create shader variant collection.", "ok");
                return;
            }
            ShaderVariantCollectionCreator.AddFromScannedData(currentVariantCollection, variantFlag);
            EditorUtility.SetDirty(currentVariantCollection);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Complete", "Add shader variants to collection.", "ok");
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



        private static VisualElement CloneTree(VisualTreeAsset asset)
        {
            return asset.CloneTree();
        }
    }

}
