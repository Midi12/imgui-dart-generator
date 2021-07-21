using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imgui_dart_generator
{
    public sealed class DartCodeWriter : IDisposable
    {
        private readonly StreamWriter _sw;
        private int _currentIndentation;
        private int _indent;

        public DartCodeWriter(string outputPath, int indent = 2)
        {
            _sw = File.CreateText(outputPath);
            _indent = indent;
            _currentIndentation = 0;
        }

        public DartCodeWriter AddIndentation()
        {
            _currentIndentation++;

            return this;
        }

        public DartCodeWriter RemoveIndentation()
        {
            if (_currentIndentation > 0)
                _currentIndentation--;

            return this;
        }

        public DartCodeWriter PushBlock(string header)
        {
            WriteLine($"{header} {{");
            AddIndentation();

            return this;
        }

        public DartCodeWriter PopBlock()
        {
            RemoveIndentation();
            WriteLine("}");

            return this;
        }

        public DartCodeWriter WriteLine()
        {
            return WriteLine(string.Empty);
        }

        public DartCodeWriter WriteLine(string content)
        {
            _sw.Write(new String(Enumerable.Repeat(' ', _currentIndentation * _indent).ToArray()));
            _sw.WriteLine(content);

            return this;
        }

        public void Dispose()
        {
            _sw.Close();
            _sw.Dispose();
        }
    }
}
