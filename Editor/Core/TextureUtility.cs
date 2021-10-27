using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_2020_2_OR_NEWER
using UnityEngine.Experimental.Rendering;
#endif

namespace UTJ.FrameDebugSave
{
    public class TextureUtility
    {
        public static RenderTexture GetGameViewRT()
        {
            var renderTextures = Resources.FindObjectsOfTypeAll<RenderTexture>();
            foreach (var rt in renderTextures)
            {
                if (rt.name == "GameView RT")
                {
                    return rt;
                }
            }
            return null;
        }

        public static RenderTexture GetTargetRenderTexture(FrameInfoCrawler.FrameDebuggerEventData info)
        {
            RenderTexture target = null;
            var renderTextures = Resources.FindObjectsOfTypeAll<RenderTexture>();
            foreach (var rt in renderTextures)
            {
                if (rt.width == info.rtWidth && rt.height == info.rtHeight &&
                     rt.name == info.rtName )
                {
                    if (target != null) { Debug.LogWarning("Already find renderTarget. " + info.rtName); }
                    target = rt;
                }
            }
            return target;
        }

        public static FrameDebugDumpInfo.SavedTextureInfo 
            SaveRenderTexture(RenderTexture renderTexture, string file)
        {
            try
            {
                return RenderTextureSaveUtility.SaveRenderTexture(renderTexture, file);
            }catch(System.Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        public static FrameDebugDumpInfo.SavedTextureInfo SaveTexture(Texture2D tex, string file)
        {
            FrameDebugDumpInfo.SavedTextureInfo saveInfo = null;
            try
            {
                Texture2D writeTexture = null;
                if (tex.isReadable)
                {
                    writeTexture = tex;
                }
                else
                {
#if UNITY_2019_2_OR_NEWER
                    writeTexture = new Texture2D(tex.width, tex.height, tex.format, tex.mipmapCount, false);
#else
                    writeTexture = new Texture2D(tex.width, tex.height, tex.format, tex.mipmapCount > 0, false);
#endif
                    Graphics.CopyTexture(tex, writeTexture);
                }
                if (ShoudSaveRawData(tex))
                {
                    byte[] bytes = writeTexture.GetRawTextureData();
                    file += ".raw";
                    System.IO.File.WriteAllBytes(file, bytes);
                    saveInfo = new FrameDebugDumpInfo.SavedTextureInfo(file, tex, FrameDebugDumpInfo.SavedTextureInfo.TYPE_RAWDATA);
                }
                else
                {
                    byte[] bytes = writeTexture.EncodeToPNG();
                    file += ".png";
                    System.IO.File.WriteAllBytes(file, bytes);
                    saveInfo = new FrameDebugDumpInfo.SavedTextureInfo(file, tex, FrameDebugDumpInfo.SavedTextureInfo.TYPE_PNG);
                }
                if (tex != writeTexture)
                {
                    Object.DestroyImmediate(writeTexture);
                }
                return saveInfo;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            return null;
        }

        private static TextureFormat GetTextureFormat(RenderTexture tex)
        {
            switch (tex.format)
            {
                case RenderTextureFormat.ARGB2101010:
                case RenderTextureFormat.ARGB64:
                case RenderTextureFormat.ARGBFloat:
                    return TextureFormat.RGBAFloat;
                case RenderTextureFormat.ARGBHalf:
                case RenderTextureFormat.DefaultHDR:
                case RenderTextureFormat.RGB111110Float:
                    return TextureFormat.RGBAHalf;
            }
            return TextureFormat.RGBA32;
        }

        public static bool ShouldSaveAsDepth(RenderTexture tex)
        {
            switch (tex.format)
            {
                case RenderTextureFormat.Depth:
                    return true;
                case RenderTextureFormat.Shadowmap:
                    return true;
            }
            return false;
        }

 
        internal static bool ShoudSaveRawData(Texture2D tex)
        {
            switch (tex.format)
            {
                case TextureFormat.DXT1:
                case TextureFormat.DXT1Crunched:
                case TextureFormat.DXT5:
                case TextureFormat.DXT5Crunched:
                    return true;
#if UNITY_2019_1_OR_NEWER || UNITY_2019_OR_NEWER
                case TextureFormat.ASTC_4x4:
                case TextureFormat.ASTC_5x5:
                case TextureFormat.ASTC_6x6:
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ASTC_10x10:
                case TextureFormat.ASTC_12x12:
                    return true;
#endif
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                    return true;
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ETC2_RGBA8Crunched:
                    return true;
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC_RGB4Crunched:
                    return true;

                case TextureFormat.RFloat:
                case TextureFormat.RGFloat:
                case TextureFormat.RGB9e5Float:
                case TextureFormat.RGBAFloat:
                case TextureFormat.RHalf:
                case TextureFormat.RGHalf:
                case TextureFormat.RGBAHalf:
                    return true;
            }
            return false;
        }

        public static Texture LoadTexture(string basePath , FrameDebugDumpInfo.SavedTextureInfo info)
        {
            if( info == null || info.path == null ) {
                return null;
            }
            string path = System.IO.Path.Combine(basePath,info.path);
            if(!System.IO.File.Exists(path))
            {
                return null;
            }
            byte[] data = System.IO.File.ReadAllBytes(path);
            Texture2D tex = null;
#if UNITY_2020_2_OR_NEWER
            if (info.type == FrameDebugDumpInfo.SavedTextureInfo.TYPE_RENDERTEXURE_RAWDATA)
            {
                if (info.mipCount > 1)
                {
                    tex = new Texture2D(info.width, info.height, info.savedFormat, info.mipCount,
                        TextureCreationFlags.MipChain);
                }
                else
                {
                    tex = new Texture2D(info.width, info.height, info.savedFormat, info.mipCount,
                        TextureCreationFlags.None);
                }
            }
            else if (info.type != FrameDebugDumpInfo.SavedTextureInfo.TYPE_NO_TEXTURE )
            {
                tex = new Texture2D(info.width, info.height, 
                    (TextureFormat)info.textureFormat, info.mipCount, false);
            }
#elif UNITY_2019_2_OR_NEWER
            tex = new Texture2D(info.width, info.height, info.rawFormat, info.mipCount, false);
#else
            tex = new Texture2D(info.width, info.height, info.rawFormat, (info.mipCount > 0), false);
#endif
            switch (info.type)
            {
                case FrameDebugDumpInfo.SavedTextureInfo.TYPE_PNG:
                    ImageConversion.LoadImage(tex, data);
                    break;
                case FrameDebugDumpInfo.SavedTextureInfo.TYPE_EXR:
                    ImageConversion.LoadImage(tex, data);
                    break;
                case FrameDebugDumpInfo.SavedTextureInfo.TYPE_RAWDATA:
                case FrameDebugDumpInfo.SavedTextureInfo.TYPE_RENDERTEXURE_RAWDATA:
                    tex.LoadRawTextureData(data);
                    tex.Apply();
                    break;
            }

            return tex;
        }

        public static int GetMipMapCount(Texture tex)
        {
#if UNITY_2019_2_OR_NEWER
            return tex.mipmapCount;
#else
            Texture2D tex2d = tex as Texture2D;
            if(tex2d == null){return 0;}
            return tex2d.mipmapCount;
#endif
        }
    }
}