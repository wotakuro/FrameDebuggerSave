using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace UTJ.FrameDebugSave
{
    public class JsonStringGenerator
    {
        private StringBuilder stringBuilder;
        private List<int> objNum;
        private int currentIdx;
        private bool isFirstLine = true;

        public class ObjectScope :System.IDisposable
        {
            private JsonStringGenerator jsonStringGenerator;
            public ObjectScope(JsonStringGenerator generator)
            {
                this.jsonStringGenerator = generator;
                generator.StartObject();
            }
            public void Dispose()
            {
                this.jsonStringGenerator.EndObject();
            }
        }
        public class ObjectScopeWithName : System.IDisposable
        {
            private JsonStringGenerator jsonStringGenerator;
            public ObjectScopeWithName(JsonStringGenerator generator,string name)
            {
                this.jsonStringGenerator = generator;
                generator.StartObjectWithName(name);
            }
            public void Dispose()
            {
                this.jsonStringGenerator.EndObject();
            }
        }

        public class ObjectArrayValueScope : System.IDisposable
        {
            private JsonStringGenerator jsonStringGenerator;

            public ObjectArrayValueScope(JsonStringGenerator generator,string name)
            {
                this.jsonStringGenerator = generator;
                generator.StartObjectArray(name);
            }
            public void Dispose()
            {
                this.jsonStringGenerator.EndArray();
            }
        }

        public JsonStringGenerator()
        {
            stringBuilder = new StringBuilder(1024 * 1024);
            this.objNum = new List<int>(32);
            currentIdx = -1;
        }
        public JsonStringGenerator(StringBuilder sb)
        {
            stringBuilder = sb;
        }

        private JsonStringGenerator StartObjectWithName(string name)
        {
            ExecuteObjectNum();
            NextLine();
            ExecuteWhiteSpace();
            this.stringBuilder.Append('"');
            this.stringBuilder.Append(name);
            this.stringBuilder.Append("\":{");
            IncCurrentIdx();
            return this;
        }
        private JsonStringGenerator StartObject()
        {
            ExecuteObjectNum();
            NextLine();
            ExecuteWhiteSpace();
            this.stringBuilder.Append("{");
            IncCurrentIdx();
            return this;
        }
        private JsonStringGenerator EndObject()
        {
            DecCurrentIdx();
            NextLine();
            ExecuteWhiteSpace();
            this.stringBuilder.Append("}");
            return this;
        }
        private JsonStringGenerator StartObjectArray(string name)
        {
            ExecuteObjectNum();
            NextLine();
            ExecuteWhiteSpace();

            this.stringBuilder.Append('"');
            this.stringBuilder.Append(name);
            this.stringBuilder.Append("\":[");
            IncCurrentIdx();
            return this;
        }
        private JsonStringGenerator EndArray()
        {
            DecCurrentIdx();
            NextLine();
            ExecuteWhiteSpace();

            this.stringBuilder.Append("]");
            return this;
        }
        public JsonStringGenerator AddObjectValue(string name, string val)
        {
            ExecuteObjectNum();
            NextLine();
            ExecuteWhiteSpace();
            this.stringBuilder.Append('"');
            this.stringBuilder.Append(name);
            this.stringBuilder.Append("\":\"");
            this.stringBuilder.Append(val);
            this.stringBuilder.Append('"');
            return this;
        }
        public JsonStringGenerator AddObjectValue(string name, float val)
        {
            ExecuteObjectNum();
            NextLine();
            ExecuteWhiteSpace();
            this.stringBuilder.Append('"');
            this.stringBuilder.Append(name);
            this.stringBuilder.Append("\":");
            this.AppendFloatValueToStringBuilder(val);
            return this;
        }
        public JsonStringGenerator AddArrayValue(float val)
        {
            ExecuteObjectNum();
            this.AppendFloatValueToStringBuilder(val);
            return this;
        }
        public JsonStringGenerator AddArrayValue(string val)
        {
            ExecuteObjectNum();
            this.stringBuilder.Append('"');
            this.stringBuilder.Append(val);
            this.stringBuilder.Append('"');
            return this;
        }
        #region USE_UNITY_TYPE
        public JsonStringGenerator AddObjectVector(string name, ref Vector4 val)
        {
            ExecuteObjectNum();
            NextLine();
            ExecuteWhiteSpace();
            this.stringBuilder.Append('"');
            this.stringBuilder.Append(name);
            this.stringBuilder.Append("\":[");
            this.AppendFloatValueToStringBuilder(val.x).Append(',');
            this.AppendFloatValueToStringBuilder(val.y).Append(',');
            this.AppendFloatValueToStringBuilder(val.z).Append(',');
            this.AppendFloatValueToStringBuilder(val.w).Append(']');
            return this;
        }

        public JsonStringGenerator AddObjectMatrix(string name,ref Matrix4x4 matrix)
        {
            ExecuteObjectNum();
            NextLine();
            ExecuteWhiteSpace();
            this.stringBuilder.Append('"');
            this.stringBuilder.Append(name);
            this.stringBuilder.Append("\":[");
            for (int i = 0; i < 4; ++i)
            {
                NextLine();
                ExecuteWhiteSpace(1);
                for (int j = 0; j < 4; ++j)
                {
                    this.AppendFloatValueToStringBuilder(matrix[i * 4 + j]);
                    if( i != 3 || j != 3)
                    {
                        this.stringBuilder.Append(',');
                    }
                }
            }
            this.stringBuilder.Append(']');
            return this;
        }
        # endregion USE_UNITY_TYPE

        private StringBuilder AppendFloatValueToStringBuilder(float val)
        {
            if (float.IsNaN(val))
            {
                this.stringBuilder.Append("\"Nan\"");
            }
            else if (float.IsNegativeInfinity(val))
            {
                this.stringBuilder.Append("\"-Infinity\"");
            }
            else if (float.IsPositiveInfinity(val))
            {
                this.stringBuilder.Append("\"Infinity\"");
            }
            else
            {
                this.stringBuilder.Append(val);
            }
            return stringBuilder;
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }

        private void IncCurrentIdx()
        {
            ++currentIdx;
            if (objNum.Count <= currentIdx)
            {
                objNum.Add(0);
            }
        }

        private void DecCurrentIdx()
        {
            objNum[currentIdx] = 0;
            --currentIdx;

        }


        private void ExecuteWhiteSpace(int append = 0)
        {
            for (int i = 0; i <= currentIdx + append; ++i)
            {
                this.stringBuilder.Append("  ");
            }
        }
        private void NextLine() {
            if (isFirstLine)
            {
                isFirstLine = false;
                return;
            }
            this.stringBuilder.Append("\n");
        }

        private void ExecuteObjectNum()
        {
            if(currentIdx <0) { return; }
            if ( objNum[currentIdx] > 0)
            {
                this.stringBuilder.Append(",");
            }
            ++objNum[currentIdx];
        }

    }
}