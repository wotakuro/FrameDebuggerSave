using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_2020_2_OR_NEWER
using UnityEngine.Experimental.Rendering;
#endif

namespace UTJ.FrameDebugSave
{
    public class FrameDebugDumpInfo
    {
        [SerializeField]
        public int captureFlag;
        [SerializeField]
        public FrameEventInfo[] events;

        [System.Serializable]
        public class FrameEventInfo
        {
            [SerializeField]
            public int frameEventIndex;
            [SerializeField]
            public string type;
            [SerializeField]
            public SavedTextureInfo screenshot;

            [SerializeField]
            public FrameRenderingInfo rendering;
            [SerializeField]
            public RenderTargetInfo renderTarget;
            [SerializeField]
            public ShaderInfo shaderInfo;

        }
        [System.Serializable]
        public class FrameRenderingInfo
        {
            [SerializeField]
            public int vertexCount;
            [SerializeField]
            public int indexCount;
            [SerializeField]
            public int instanceCount;
            [SerializeField]
            public int drawCallCount;
            [SerializeField]
            public int componentInstanceID;
            [SerializeField]
            public int meshInstanceID;
            [SerializeField]
            public int meshSubset;
            [SerializeField]
            public string batchBreakCauseStr;
            [SerializeField]
            public string gameobject;
        }

        [System.Serializable]
        public class RenderTargetInfo
        {
            [SerializeField]
            public string rtName;
            [SerializeField]
            public int rtWidth;
            [SerializeField]
            public int rtHeight;
            [SerializeField]
            public int rtCount;
            [SerializeField]
            public int rtHasDepthTexture;
        }

        [System.Serializable]
        public class ShaderInfo
        {
            [SerializeField]
            public string shaderName;
            [SerializeField]
            public int subShaderIndex;
            [SerializeField]
            public int shaderPassIndex;
            [SerializeField]
            public string passName;
            [SerializeField]
            public string passLightMode;
            [SerializeField]
            public string shaderKeywords;
            [SerializeField]
            public ShaderParamInfo shaderParams;
        }

        [System.Serializable]
        public class ShaderParamInfo
        {
            [SerializeField]
            public TextureParamInfo[] textures;
            [SerializeField]
            public FloatParamInfo[] floats;
            [SerializeField]
            public VectorParamInfo[] vectors;
            [SerializeField]
            public MatrixParamInfo[] matricies;
            [SerializeField]
            public BufferParamInfo[] buffers;
        }
        [System.Serializable]
        public class TextureParamInfo
        {
            [SerializeField]
            public string name;
            [SerializeField]
            public string textureName;
            [SerializeField]
            public string originFormat;
            [SerializeField]
            public int originWidth;
            [SerializeField]
            public int originHeight;
            [SerializeField]
            public int originMipCount;
            [SerializeField]
            public SavedTextureInfo saved;
        }
        [System.Serializable]
        public class FloatParamInfo
        {
            [SerializeField]
            public string name;
            [SerializeField]
            public float val;
        }
        [System.Serializable]
        public class VectorParamInfo
        {
            [SerializeField]
            public string name;
            [SerializeField]
            public float[] val;
        }
        [System.Serializable]
        public class MatrixParamInfo
        {
            [SerializeField]
            public string name;
            [SerializeField]
            public float[] val;
        }
        [System.Serializable]
        public class BufferParamInfo
        {
            [SerializeField]
            public string name;
        }
        [System.Serializable]
        public class SavedTextureInfo
        {
            public const int TYPE_PNG = 0;
            public const int TYPE_EXR = 1;
            public const int TYPE_RAWDATA = 2;
            public const int TYPE_RENDERTEXURE_RAWDATA = 3;
            public const int TYPE_NO_TEXTURE = 4;

            [SerializeField]
            public string path;
            [SerializeField]
            public int type;
            [SerializeField]
            public int width;
            [SerializeField]
            public int height;
            [SerializeField]
            public int mipCount;
            [SerializeField]
            public int textureFormat;

#if UNITY_2020_2_OR_NEWER
            [SerializeField]
            public int saveGraphicsFormat;
            [SerializeField]
            public int originGraphicsFormat;

            public GraphicsFormat savedFormat
            {
                get { return (GraphicsFormat)saveGraphicsFormat; }
            }
            public GraphicsFormat originFormat
            {
                get { return (GraphicsFormat)originGraphicsFormat; }
            }
#endif
            public SavedTextureInfo(string p, RenderTexture origin,
                RenderTexture capture)
            {
                this.width = capture.width;
                this.height = capture.height;
                this.mipCount = TextureUtility.GetMipMapCount(capture);
                this.type = TYPE_RENDERTEXURE_RAWDATA;
                p = p.Replace('\\', '/');
                int fileNameIdx = p.LastIndexOf('/');
                int lastDirIdx = 0;
                if (fileNameIdx > 0)
                {
                    lastDirIdx = p.LastIndexOf('/', fileNameIdx - 1);
                }
                lastDirIdx += 1;

                this.path = p.Substring(lastDirIdx);
                originGraphicsFormat = (int)origin.graphicsFormat;
                saveGraphicsFormat = (int)capture.graphicsFormat;
            }

            public SavedTextureInfo(string p, Texture tex, int t)
            {
                this.width = tex.width;
                this.height = tex.height;
                this.mipCount = TextureUtility.GetMipMapCount(tex);
                this.type = t;
                p = p.Replace('\\', '/');
                int fileNameIdx = p.LastIndexOf('/');
                int lastDirIdx = 0;
                if (fileNameIdx > 0)
                {
                    lastDirIdx = p.LastIndexOf('/', fileNameIdx - 1);
                }
                lastDirIdx += 1;

                this.path = p.Substring(lastDirIdx);
                if (t == TYPE_RAWDATA && tex.GetType() == typeof(Texture2D))
                {
                    this.textureFormat = (int) ((Texture2D)tex).format;
                }
            }


        }

        public static Texture2D LoadTexture(string basePath , SavedTextureInfo info)
        {
            return TextureUtility.LoadTexture(basePath , info) as Texture2D;
        }



        public static FrameDebugDumpInfo LoadFromFile(string path)
        {
            var str = File.ReadAllText(path);
            str = str.Replace("\"-Infinity\"", "-Infinity")
                .Replace("\"Infinity\"", "Infinity").Replace("\"Nan\"","Nan");
            var info = JsonUtility.FromJson<FrameDebugDumpInfo>(str);
            return info;
        }

    }
}
