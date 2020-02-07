using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTJ.FrameDebugSave
{

    public class TextureLoader
    {
        private string currentDir;
        private Dictionary<string, Texture2D> cachedTexture = new Dictionary<string, Texture2D>();

        public void SetDirectory(string dir)
        {
            cachedTexture.Clear();
            this.currentDir = dir;
        }

        public Texture2D LoadTexture(FrameDebugDumpInfo.SavedTextureInfo textureInfo)
        {
            Texture2D texture = null;
            if( textureInfo == null || textureInfo.path == null)
            {
                return null;
            }
            if( cachedTexture.TryGetValue(textureInfo.path,out texture))
            {
                return texture;
            }
            texture = FrameDebugDumpInfo.LoadTexture(this.currentDir, textureInfo);
            cachedTexture.Add(textureInfo.path, texture);

            return texture;
        }

    }
}