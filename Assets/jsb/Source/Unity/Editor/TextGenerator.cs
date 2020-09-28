using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public class TextGenerator
    {
        public class IndentBlock : IDisposable
        {
            private TextGenerator _generator;

            public IndentBlock(TextGenerator generator)
            {
                _generator = generator;
                _generator.AddTabLevel();
            }

            public void Dispose()
            {
                _generator.DecTabLevel();
            }
        }

        public class CodeBlock : IDisposable
        {
            private TextGenerator _generator;

            public CodeBlock(TextGenerator generator)
            {
                _generator = generator;
                _generator.AppendLine("{");
                _generator.AddTabLevel();
            }

            public void Dispose()
            {
                _generator.DecTabLevel();
                _generator.AppendLine("}");
            }
        }

        public bool enabled = true;
        public readonly string newline;
        private string tab;
        private StringBuilder sb = new StringBuilder();
        private int tabLevel;

        public int size { get { return sb.Length; } }

        public string tabString
        {
            get
            {
                var s = "";
                for (var i = 0; i < tabLevel; i++)
                {
                    s += tab;
                }
                return s;
            }
        }

        public TextGenerator(string newline, string tab)
        {
            this.newline = newline;
            this.tab = tab;
            this.tabLevel = 0;
        }

        public override string ToString()
        {
            return sb.ToString();
        }

        public CodeBlock CodeBlockScope()
        {
            return new CodeBlock(this);
        }

        public IndentBlock IndentBlockScope()
        {
            return new IndentBlock(this);
        }

        public void BeginBlock()
        {
            if (!enabled)
            {
                return;
            }
            AppendLine("{");
            tabLevel++;
        }

        public void EndBlock()
        {
            if (!enabled)
            {
                return;
            }
            tabLevel--;
            AppendLine("}");
        }

        public void AddTabLevel()
        {
            if (!enabled)
            {
                return;
            }
            tabLevel++;
        }

        public void DecTabLevel()
        {
            if (!enabled)
            {
                return;
            }
            tabLevel--;
        }

        public void AppendTab()
        {
            if (!enabled)
            {
                return;
            }
            for (var i = 0; i < tabLevel; i++)
            {
                sb.Append(tab);
            }
        }

        public void AppendLines(params string[] lines)
        {
            if (!enabled)
            {
                return;
            }

            foreach (var line in lines)
            {
                AppendLine(line);
            }
        }

        public void AppendLine()
        {
            if (!enabled)
            {
                return;
            }
            sb.Append(newline);
        }

        public void AppendLine(string text)
        {
            if (!enabled)
            {
                return;
            }
            AppendTab();
            sb.Append(text);
            sb.Append(newline);
        }

        public void AppendLine(string text, object arg1)
        {
            if (!enabled)
            {
                return;
            }
            AppendTab();
            sb.AppendFormat(text, arg1);
            sb.Append(newline);
        }

        public void AppendLine(string text, object arg1, object arg2)
        {
            if (!enabled)
            {
                return;
            }
            AppendTab();
            sb.AppendFormat(text, arg1, arg2);
            sb.Append(newline);
        }

        public void AppendLine(string text, object arg1, object arg2, object arg3)
        {
            if (!enabled)
            {
                return;
            }

            AppendTab();
            sb.AppendFormat(text, arg1, arg2, arg3);
            sb.Append(newline);
        }

        public void AppendLine(string text, params object[] args)
        {
            if (!enabled)
            {
                return;
            }

            AppendTab();
            sb.AppendFormat(text, args);
            sb.Append(newline);
        }

        public void AppendLineL(string text)
        {
            if (!enabled)
            {
                return;
            }

            sb.Append(text);
            sb.Append(newline);
        }

        public void AppendLineL(string text, object arg1)
        {
            if (!enabled)
            {
                return;
            }

            sb.AppendFormat(text, arg1);
            sb.Append(newline);
        }

        public void AppendLineL(string text, object arg1, object arg2)
        {
            if (!enabled)
            {
                return;
            }

            sb.AppendFormat(text, arg1, arg2);
            sb.Append(newline);
        }

        public void AppendLineL(string text, object arg1, object arg2, object arg3)
        {
            if (!enabled)
            {
                return;
            }

            sb.AppendFormat(text, arg1, arg2, arg3);
            sb.Append(newline);
        }

        public void AppendLineL(string text, params object[] args)
        {
            if (!enabled)
            {
                return;
            }

            sb.AppendFormat(text, args);
            sb.Append(newline);
        }

        public void Append(string text)
        {
            if (!enabled)
            {
                return;
            }

            AppendTab();
            sb.Append(text);
        }

        public void Append(string text, object arg1)
        {
            if (!enabled)
            {
                return;
            }

            AppendTab();
            sb.AppendFormat(text, arg1);
        }

        public void Append(string text, object arg1, object arg2)
        {
            if (!enabled)
            {
                return;
            }

            AppendTab();
            sb.AppendFormat(text, arg1, arg2);
        }

        public void Append(string text, object arg1, object arg2, object arg3)
        {
            if (!enabled)
            {
                return;
            }

            AppendTab();
            sb.AppendFormat(text, arg1, arg2, arg3);
        }

        public void Append(string text, params object[] args)
        {
            if (!enabled)
            {
                return;
            }

            AppendTab();
            sb.AppendFormat(text, args);
        }

        public void AppendL(string text)
        {
            if (!enabled)
            {
                return;
            }

            sb.Append(text);
        }

        public void AppendL(string text, object arg1)
        {
            if (!enabled)
            {
                return;
            }

            sb.AppendFormat(text, arg1);
        }

        public void AppendL(string text, object arg1, object arg2)
        {
            if (!enabled)
            {
                return;
            }

            sb.AppendFormat(text, arg1, arg2);
        }

        public void AppendL(string text, object arg1, object arg2, object arg3)
        {
            if (!enabled)
            {
                return;
            }

            sb.AppendFormat(text, arg1, arg2, arg3);
        }

        public void AppendL(string text, params object[] args)
        {
            if (!enabled)
            {
                return;
            }

            sb.AppendFormat(text, args);
        }

        public void Clear()
        {
            tabLevel = 0;
            sb.Clear();
        }
    }
}
