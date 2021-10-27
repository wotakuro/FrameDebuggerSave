using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace UTJ.FrameDebugSave
{
    public class RenderTextureSaveUtility 
    {


#if UNITY_2020_2_OR_NEWER
        private static Material depthMaterial;

        public static FrameDebugDumpInfo.SavedTextureInfo SaveRenderTexture(RenderTexture src, string file)
        {
            
            FrameDebugDumpInfo.SavedTextureInfo saveInfo = null;
            byte[] saveData = null;
            RenderTexture capture = null;
            bool createTmpTexture = ShouldCreateTmpTexture(src);
            if ( createTmpTexture)
            {
                capture = CreateTmpTexture(src);
            }
            else
            {
                capture = src;
            }
            var req = AsyncGPUReadback.Request(capture, 0,
                capture.graphicsFormat, null);
            req.WaitForCompletion();
            var data = req.GetData<byte>();

            if (Prefer32Bit(capture.graphicsFormat) || 
                Prefer16Bit(capture.graphicsFormat))
            {
                /* can save exr but can't load exr.... */
                saveData = data.ToArray();
                file += ".raw";
                saveInfo = new FrameDebugDumpInfo.SavedTextureInfo(file,src, capture);
            }
            else
            {
                var pngBin = ImageConversion.EncodeNativeArrayToPNG(data,
                    capture.graphicsFormat, (uint)capture.width, (uint)capture.height);
                saveData = pngBin.ToArray();
                file += ".png";
                saveInfo = new FrameDebugDumpInfo.SavedTextureInfo(file, capture, FrameDebugDumpInfo.SavedTextureInfo.TYPE_PNG);
            }

            if (createTmpTexture)
            {
                RenderTexture.active = null;
                capture.Release();
            }
            if (saveData != null)
            {
                System.IO.File.WriteAllBytes(file, saveData);
            }
            return saveInfo;
        }


        private static RenderTexture CreateTmpTexture(RenderTexture src)
        {
            RenderTexture dest = null;
            if (ShouldSaveAsDepth(src))
            {
                if (!depthMaterial) { depthMaterial = new Material(Shader.Find("Hidden/RenderDebugSave/RenderDepth")); }
                dest = new RenderTexture(src.width, src.height, src.depth,
                    GraphicsFormat.R16_SFloat, src.mipmapCount);

                CommandBuffer cmdBuffer = new CommandBuffer();
                cmdBuffer.SetRenderTarget(dest);
                cmdBuffer.ClearRenderTarget(true, true, Color.black);
                cmdBuffer.Blit(src, dest, depthMaterial);
                Graphics.ExecuteCommandBuffer(cmdBuffer);
                return dest;
            }else if (Prefer32Bit(src.graphicsFormat))
            {
                dest = new RenderTexture(src.width, src.height, src.depth,
                    GraphicsFormat.R32G32B32A32_SFloat,src.mipmapCount);
            }
            else if (Prefer16Bit(src.graphicsFormat))
            {
                dest = new RenderTexture(src.width, src.height, src.depth,
                    GraphicsFormat.R16G16B16A16_SFloat, src.mipmapCount);
            }
            else
            {
                dest = new RenderTexture(src.width, src.height, src.depth,
                    GraphicsFormat.R8G8B8A8_UNorm, src.mipmapCount);
            }
            Graphics.Blit(src, dest);
            return dest;
        }

        private static bool ShouldSaveAsDepth(RenderTexture tex)
        {
            switch (tex.format)
            {
                case RenderTextureFormat.Depth:
                case RenderTextureFormat.Shadowmap:
                    return true;
            }
            return false;
        }

        private static bool ShouldCreateTmpTexture(RenderTexture rt)
        {
            if (ShouldSaveAsDepth(rt)) { return true; }
            var format = rt.graphicsFormat;
            bool isSupportSetPixel = SystemInfo.IsFormatSupported(format, FormatUsage.SetPixels);
            if (!isSupportSetPixel) { return true; }
            bool isSupportReadPixel = SystemInfo.IsFormatSupported(format, FormatUsage.ReadPixels);
            if (!isSupportReadPixel) { return true; }
            return false;
        }

        private static bool Prefer16Bit(GraphicsFormat format)
        {
            switch (format)
            {
                case GraphicsFormat.A10R10G10B10_XRSRGBPack32:
                case GraphicsFormat.A10R10G10B10_XRUNormPack32:
                case GraphicsFormat.A2B10G10R10_SIntPack32:
                case GraphicsFormat.A2B10G10R10_UIntPack32:
                case GraphicsFormat.A2B10G10R10_UNormPack32:
                case GraphicsFormat.A2R10G10B10_SIntPack32:
                case GraphicsFormat.A2R10G10B10_UIntPack32:
                case GraphicsFormat.A2R10G10B10_UNormPack32:
                case GraphicsFormat.A2R10G10B10_XRSRGBPack32:
                case GraphicsFormat.A2R10G10B10_XRUNormPack32:
                case GraphicsFormat.B10G11R11_UFloatPack32:
                case GraphicsFormat.E5B9G9R9_UFloatPack32:
                case GraphicsFormat.R10G10B10_XRSRGBPack32:
                case GraphicsFormat.R10G10B10_XRUNormPack32:
                case GraphicsFormat.R16G16B16A16_SFloat:
                case GraphicsFormat.R16G16B16A16_SInt:
                case GraphicsFormat.R16G16B16A16_SNorm:
                case GraphicsFormat.R16G16B16A16_UInt:
                case GraphicsFormat.R16G16B16A16_UNorm:
                case GraphicsFormat.R16G16B16_SFloat:
                case GraphicsFormat.R16G16B16_SInt:
                case GraphicsFormat.R16G16B16_SNorm:
                case GraphicsFormat.R16G16B16_UInt:
                case GraphicsFormat.R16G16B16_UNorm:
                case GraphicsFormat.R16G16_SFloat:
                case GraphicsFormat.R16G16_SInt:
                case GraphicsFormat.R16G16_SNorm:
                case GraphicsFormat.R16G16_UInt:
                case GraphicsFormat.R16G16_UNorm:
                case GraphicsFormat.R16_SFloat:
                case GraphicsFormat.R16_SInt:
                case GraphicsFormat.R16_SNorm:
                case GraphicsFormat.R16_UInt:
                case GraphicsFormat.R16_UNorm:
                    return true;
                default:
                    return false;
            }
        }

        private static bool Prefer32Bit(GraphicsFormat format)
        {
            switch (format)
            {
                // 32 bit
                case GraphicsFormat.R32G32B32A32_SFloat:
                case GraphicsFormat.R32G32B32A32_SInt:
                case GraphicsFormat.R32G32B32A32_UInt:
                case GraphicsFormat.R32G32B32_SFloat:
                case GraphicsFormat.R32G32B32_SInt:
                case GraphicsFormat.R32G32B32_UInt:
                case GraphicsFormat.R32G32_SFloat:
                case GraphicsFormat.R32G32_SInt:
                case GraphicsFormat.R32G32_UInt:
                case GraphicsFormat.R32_SFloat:
                case GraphicsFormat.R32_SInt:
                case GraphicsFormat.R32_UInt:
                    return true;
                default:
                    return false;
            }

        }
#else

        public static SaveTextureInfo SaveRenderTexture(RenderTexture renderTexture, string file)
        {
            SaveTextureInfo saveInfo = null;
            try
            {
                Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, GetTextureFormat(renderTexture), false);
                RenderTexture.active = renderTexture;
                tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                tex.Apply();

                if (ShoudSaveRawData(tex))
                {
                    byte[] bytes = tex.GetRawTextureData();
                    file += ".raw";
                    System.IO.File.WriteAllBytes(file, bytes);
                    saveInfo = new SaveTextureInfo(file, renderTexture, SaveTextureInfo.TYPE_RAWDATA);
                }
                else
                {
                    byte[] bytes = tex.EncodeToPNG();
                    file += ".png";
                    System.IO.File.WriteAllBytes(file, bytes);
                    saveInfo = new SaveTextureInfo(file, renderTexture, SaveTextureInfo.TYPE_PNG);
                }
                Object.DestroyImmediate(tex);
                return saveInfo;
            }catch(System.Exception e){
                Debug.LogError(e);
            }
            return null;
        }

#endif

    }
}
