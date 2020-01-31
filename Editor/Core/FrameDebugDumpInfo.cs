using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace UTJ.FrameDebugSave
{
    public class FrameDebugDumpInfo
    {
        [SerializeField]
        private int captureFlag;
        [SerializeField]
        private FrameInfo[] frames;

        [System.Serializable]
        public class FrameInfo
        {
            [SerializeField]
            int frameEventIndex;
            [SerializeField]
            string type;
            [SerializeField]
            SavedTextureInfo screenshot;

            [SerializeField]
            FrameRenderingInfo rendering;
            [SerializeField]
            RenderTargetInfo renderTarget;
            [SerializeField]
            ShaderInfo shaderInfo;

        }
        [System.Serializable]
        public class FrameRenderingInfo
        {
            [SerializeField]
            int vertexCount;
            [SerializeField]
            int indexCount;
            [SerializeField]
            int instanceCount;
            [SerializeField]
            int drawCallCount;
            [SerializeField]
            int componentInstanceID;
            [SerializeField]
            int meshInstanceID;
            [SerializeField]
            int meshSubset;
            [SerializeField]
            string batchBreakCauseStr;
            [SerializeField]
            string gameobject;
        }

        [System.Serializable]
        public class RenderTargetInfo
        {
            [SerializeField]
            string rtName;
            [SerializeField]
            int rtWidth;
            [SerializeField]
            int rtHeight;
            [SerializeField]
            int rtCount;
            [SerializeField]
            int rtHasDepthTexture;
        }

        [System.Serializable]
        public class ShaderInfo
        {
            [SerializeField]
            string shaderName;
            [SerializeField]
            int subShaderIndex;
            [SerializeField]
            int shaderPassIndex;
            [SerializeField]
            string passName;
            [SerializeField]
            string passLightMode;
            [SerializeField]
            string shaderKeywords;
            [SerializeField]
            ShaderParamInfo shaderParams;
        }

        [System.Serializable]
        public class ShaderParamInfo
        {
            [SerializeField]
            TextureParamInfo[] textures;
            [SerializeField]
            TextureParamInfo[] floats;
            [SerializeField]
            VectorParamInfo[] vectors;
            [SerializeField]
            MatrixParamInfo[] matricies;
            [SerializeField]
            BufferParamInfo[] buffers;
        }
        [System.Serializable]
        public class TextureParamInfo
        {
            [SerializeField]
            string name;
            [SerializeField]
            string textureName;
            [SerializeField]
            string originFormat;
            [SerializeField]
            int originWidth;
            [SerializeField]
            int originHeight;
            [SerializeField]
            int originMipCount;
            [SerializeField]
            SavedTextureInfo saved;
        }
        [System.Serializable]
        public class FloatParamInfo
        {
            [SerializeField]
            string name;
            [SerializeField]
            float val;
        }
        [System.Serializable]
        public class VectorParamInfo
        {
            [SerializeField]
            string name;
            [SerializeField]
            float[] val;
        }
        [System.Serializable]
        public class MatrixParamInfo
        {
            [SerializeField]
            string name;
            [SerializeField]
            float[] val;
        }
        [System.Serializable]
        public class BufferParamInfo
        {
            [SerializeField]
            string name;
        }
        [System.Serializable]
        public class SavedTextureInfo
        {
            [SerializeField]
            string path;
            [SerializeField]
            string type;
            [SerializeField]
            string width;
            [SerializeField]
            string height;
            [SerializeField]
            int mipCount;
            [SerializeField]
            int rawFormat;
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
