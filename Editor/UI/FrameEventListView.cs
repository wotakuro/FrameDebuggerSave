using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
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

    public class FrameEventListView : VisualElement
    {
        private ScrollView scrollView;
        private List<UIPair> uiDataBind;
        private int currentIdx = 0;

        public delegate void ChangedFrameEvent(FrameDebugDumpInfo.FrameEventInfo evtInfo);
        public FrameDebugDumpInfo.FrameEventInfo selectFrameInfo
        {
            get
            {
                return uiDataBind[currentIdx].eventInfo;
            }
        }

        public ChangedFrameEvent OnChangedFrame
        {
            set; get;
        }
#if !UNITY_2019_1_OR_NEWER && !UNITY_2019_OR_NEWER

        public void SetupScrollViewHeight(float height)
        {
            this.scrollView.style.height = height - 20;
        }
#endif


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
                if (view != null) { view.Initialize(); }
            }
        }


        private struct UIPair
        {
            public FrameDebugDumpInfo.FrameEventInfo eventInfo;
            public VisualElement element;

            public UIPair(FrameDebugDumpInfo.FrameEventInfo info, VisualElement elem)
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
            this.currentIdx = 0;
            this.uiDataBind = new List<UIPair>();
            this.scrollView.Clear();
            foreach (var info in infos)
            {
                var label = new Label(info.frameEventIndex.ToString() + " " + info.type);
#if UNITY_2019_1_OR_NEWER || UNITY_2019_OR_NEWER
                label.focusable = true;
#else
#endif
                label.RegisterCallback<MouseDownEvent>((evt) =>
                {
                    SetCurrentFrameEvent(label);
                });
                label.RegisterCallback<KeyDownEvent>((evt) =>
                {
                    if (evt.keyCode == KeyCode.UpArrow)
                    {
                        SetSelectedIndex(this.currentIdx - 1);
                    }
                    else if (evt.keyCode == KeyCode.DownArrow)
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
            for (int i = 0; i < this.uiDataBind.Count; ++i)
            {
                if (uiDataBind[i].element == visualElement)
                {
                    SetSelectedIndex(i);
                    break;
                }
            }
        }


        private void SetSelectedIndex(int idx)
        {
            if (idx < 0 || idx >= this.uiDataBind.Count)
            {
                return;
            }
            if (idx == currentIdx) { return; }
            var oldElem = this.uiDataBind[this.currentIdx].element;
            oldElem.style.backgroundColor = new Color(0, 0, 0, 0);

            var newElem = this.uiDataBind[idx].element;
            newElem.style.backgroundColor = Color.blue;
            newElem.Focus();

            this.currentIdx = idx;

            this.scrollView.ScrollTo(newElem);
            if (OnChangedFrame != null)
            {
                OnChangedFrame(this.uiDataBind[idx].eventInfo);
            }
        }

    }
}