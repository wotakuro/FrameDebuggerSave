using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System.IO;

namespace UTJ.FrameDebugSave
{
    public class FileLoadUtility
    {
        public static byte[] buffer;
        public static NativeArray<byte> ReadFile(string path)
        {
            FileInfo info = new FileInfo(path);
            if (!info.Exists)
            {
                return new NativeArray<byte>();
            }
            if(buffer == null)
            {
                buffer = new byte[512];
            }
            NativeArray<byte> val = new NativeArray<byte>((int)info.Length,Allocator.Persistent);


            using (Stream fs = File.OpenRead(path))
            {
                int read = 1;
                int idx = 0;
                while (read > 0)
                {
                    read = fs.Read(buffer, 0, buffer.Length);
                    for(int i = 0; i< read; ++i)
                    {
                        val[idx + i] = buffer[i];
                    }
                    idx += read;
                }
            }
            return val;
        }
    }
}