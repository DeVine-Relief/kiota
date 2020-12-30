﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.OpenApi.Models;

namespace kiota.core
{
 
    public abstract class LanguageWriter
    {
        private TextWriter writer;
        const int indentSize = 4;
        private string indentString = "                                                                                             ";
        private int currentIndent = 0;

        /// <summary>
        /// This method must be called before you can use the writer method
        /// </summary>
        /// <param name="writer"></param>
        /// <remarks>Passing this to the constructor is problematic because for writing to files, an instance of this
        /// class is needed to get the file suffix to be able to create the filestream to create the writer.
        /// By making this a separate step, we can instantiate the LanguageWriter, then get the suffix, then create the writer.</remarks>
        public void SetTextWriter(TextWriter writer)
        {
            this.writer = writer;
        }
        public abstract IPathSegmenter PathSegmenter { get; }

        private Stack<int> factorStack = new Stack<int>();
        public void IncreaseIndent(int factor = 1)
        {
            factorStack.Push(factor);
            currentIndent += indentSize * factor;
        }

        public void DecreaseIndent()
        {
            var popped = factorStack.TryPop(out var factor);
            currentIndent -= indentSize * (popped ? factor : 1);
        }

        public string GetIndent()
        {
            return indentString.Substring(0, currentIndent);
        }
        public string NewLine { get => Environment.NewLine;}
        /// <summary>
        /// Adds an empty line
        /// </summary>
        protected void WriteLine() => WriteLine(string.Empty, false);
        protected void WriteLine(string line, bool includeIndent = true)
        {
            writer.WriteLine(includeIndent ? GetIndent() + line : line);
        }

        protected void Write(string text, bool includeIndent = true)
        {
            writer.Write(includeIndent ? GetIndent() + text : text);
        }
        /// <summary>
        /// Dispatch call to Write the code element to the proper derivative write method
        /// </summary>
        /// <param name="code"></param>
        public void Write(CodeElement code)
        {
            switch (code)
            {
                case CodeClass.Declaration c: WriteCodeClassDeclaration(c); break;
                case CodeClass.End c: WriteCodeClassEnd(c); break;
                case CodeNamespace.BlockDeclaration c: WriteNamespaceDeclaration(c); break;
                case CodeNamespace.BlockEnd c: WriteNamespaceEnd(c); break;
                case CodeProperty c: WriteProperty(c); break;
                case CodeIndexer c: WriteIndexer(c); break;
                case CodeMethod c: WriteMethod(c); break;
                case CodeType c: WriteType(c); break;
                case CodeNamespace: break;
                case CodeClass: break;
                default:
                    throw new ArgumentException($"Dispatcher missing for type {code.GetType()}");
            }

        }

        public abstract string GetParameterSignature(CodeParameter parameter);
        public abstract string GetTypeString(CodeType code);
        public abstract string TranslateType(string typeName, OpenApiSchema schema);
        public abstract void WriteProperty(CodeProperty code);
        public abstract void WriteIndexer(CodeIndexer code);
        public abstract void WriteMethod(CodeMethod code);
        public abstract void WriteType(CodeType code);
        public abstract void WriteNamespaceEnd(CodeNamespace.BlockEnd code);
        public abstract void WriteNamespaceDeclaration(CodeNamespace.BlockDeclaration code);
        public abstract void WriteCodeClassDeclaration(CodeClass.Declaration code);
        public abstract void WriteCodeClassEnd(CodeClass.End code);
    }
}
