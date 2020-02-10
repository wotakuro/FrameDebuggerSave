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

    public class ScalableImageView : VisualElement
    {
        private int currentIdx = 0;

        private VisualElement pointer;

        // UXMLからオブジェクトを生成するためのファクトリー
        public new class UxmlFactory : UxmlFactory<ScalableImageView, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            // 子要素を持つかどうか
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
            // パース時に呼び出されます
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var view = ve as ScalableImageView;
                if (view != null) { view.Initialize(); }
            }
        }        

        void Initialize()
        {
            this.pointer = new VisualElement();
            pointer.AddToClassList("scalable-image-point");
            this.Add(pointer);
            RegisterCallBack();
        }

        void RegisterCallBack()
        {
            this.pointer.RegisterCallback<MouseDownEvent>(OnMouseDown);
            this.pointer.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            this.pointer.RegisterCallback<MouseUpEvent>(OnMouseUp);

        }

        private bool isOnClicked = false;
        private Vector2 mousePoint;
        private Vector2 currentSize;
        private Vector2 originTextureSize = new Vector2(0,0);

        public void SetTexture(Texture2D tex)
        {
            this.style.backgroundImage = tex;
            if( tex == null)
            {
                return;
            }
            if( tex.width == originTextureSize.x || tex.height == originTextureSize.y)
            {
                return;
            }
            originTextureSize = new Vector2(tex.width, tex.height);

            this.currentSize = this.CalcSize(this.originTextureSize,Vector2.zero,150,150);

            this.style.width = currentSize.x;
            this.style.height = currentSize.y;
        }

        private Vector2 CalcSize(Vector2 size,Vector2 expand ,float maxWidth,float maxHeight)
        {
            Vector2 retVal = new Vector2();

            float widthParam = size.x / maxWidth;
            float heightParam = size.y / maxHeight;

            if (widthParam > 1.0f && widthParam > heightParam)
            {
                retVal.x = size.x / widthParam;
                retVal.y = size.y / widthParam;
            }
            else if (heightParam > 1.0f && widthParam <= heightParam)
            {
                retVal.x = size.x / heightParam;
                retVal.y = size.y / heightParam;
            }
            else
            {
                retVal = size;
            }

            if (expand.x != 0 && expand.y != 0)
            {
                retVal += expand;
                retVal.x = Mathf.Max(retVal.x, 1);
                retVal.y = Mathf.Max(retVal.y, 1);

                widthParam = retVal.x / maxWidth;
                heightParam = retVal.y / maxHeight;
                if (widthParam > heightParam)
                {
                    retVal.y = retVal.x * maxHeight / maxWidth;
                }
                else
                {
                    retVal.x = retVal.y * maxWidth / maxHeight;
                }
                retVal.x = Mathf.Min(retVal.x, maxWidth);
                retVal.y = Mathf.Min(retVal.y, maxHeight);
            }
            return retVal;

        }

        private void OnMouseDown(MouseDownEvent e)
        {
            isOnClicked = true;
            mousePoint = e.mousePosition;
            this.pointer.CaptureMouse();
        }

        private void OnMouseMove(MouseMoveEvent e)
        {
            if (isOnClicked)
            {
                SetupScale(e.mousePosition - mousePoint,false);
            }

        }
        private void OnMouseUp(MouseUpEvent e)
        {
            isOnClicked = false;
            SetupScale(e.mousePosition - mousePoint,true);
            this.pointer.ReleaseMouse();
        }

        private void SetupScale(Vector2 delta,bool mousereleased)
        {

            var size = CalcSize(currentSize, delta, originTextureSize.x, originTextureSize.y);
            this.style.width = size.x;
            this.style.height= size.y;

            if (mousereleased)
            {
                this.currentSize = size;
            }
        }
    }
}