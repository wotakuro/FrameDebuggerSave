using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
            public string rawFormat;
        }

        public static Texture2D LoadTexture(string basePath , SavedTextureInfo info)
        {
            var converted = Convert(info);
            return TextureUtility.LoadTexture(basePath , converted) as Texture2D;
        }

        private static TextureUtility.SaveTextureInfo Convert(SavedTextureInfo info)
        {
            TextureFormat format = TextureFormat.R8;
            foreach ( var val in System.Enum.GetValues(typeof(TextureFormat)))
            {
                if(val.ToString() == info.rawFormat){
                    format = (TextureFormat)val;
                    break;
                }
            }
            TextureUtility.SaveTextureInfo convert = new TextureUtility.SaveTextureInfo(info.path,info.type,
                info.width,info.height,format,info.mipCount);
            return convert;
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
