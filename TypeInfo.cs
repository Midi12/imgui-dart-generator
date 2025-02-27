﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imgui_dart_generator
{
    public sealed class TypeInfo
    {
        public static readonly Dictionary<string, string> WellKnownTypes = new Dictionary<string, string>()
        {
            { "bool", "byte" },
            { "unsigned char", "byte" },
            { "signed char", "sbyte" },
            { "char", "byte" },
            { "ImWchar", "ushort" },
            { "ImFileHandle", "IntPtr" },
            { "ImU8", "byte" },
            { "ImS8", "sbyte" },
            { "ImU16", "ushort" },
            { "ImS16", "short" },
            { "ImU32", "uint" },
            { "ImS32", "int" },
            { "ImU64", "ulong" },
            { "ImS64", "long" },
            { "unsigned short", "ushort" },
            { "unsigned int", "uint" },
            { "ImWchar16", "ushort" }, //char is not blittable
            { "ImVec4_Simple", "ImVec4" },
            { "ImColor_Simple", "ImColor" },
            { "ImTextureID", "IntPtr" },
            { "ImGuiID", "uint" },
            { "ImDrawIdx", "ushort" },
            { "ImDrawListSharedData", "IntPtr" },
            { "ImDrawListSharedData*", "IntPtr" },
            { "ImDrawCallback", "IntPtr" },
            { "size_t", "uint" },
            { "ImGuiContext*", "IntPtr" },
            { "ImPlotContext*", "IntPtr" },
            { "EditorContext*", "IntPtr" },
            { "ImGuiMemAllocFunc", "IntPtr" },
            { "ImGuiMemFreeFunc", "IntPtr" },
            { "ImFontBuilderIO", "IntPtr" },
            { "float[2]", "Vector2*" },
            { "float[3]", "Vector3*" },
            { "float[4]", "Vector4*" },
            { "int[2]", "int*" },
            { "int[3]", "int*" },
            { "int[4]", "int*" },
            { "float&", "float*" },
            { "ImVec2[2]", "Vector2*" },
            { "char* []", "byte**" },
            { "unsigned char[256]", "byte*"},
        };

        public static readonly List<string> WellKnownEnums = new List<string>()
        {
            "ImGuiMouseButton"
        };

        public static readonly Dictionary<string, string> WellKnownFieldReplacements = new Dictionary<string, string>()
        {
            { "bool", "bool" }, // Force bool to remain as bool in type-safe wrappers.
        };

        public static readonly HashSet<string> CustomDefinedTypes = new HashSet<string>()
        {
            "ImVector",
            "ImGuiStoragePair",
        };

        public static readonly Dictionary<string, string> WellKnownDefaultValues = new Dictionary<string, string>()
        {
            { "((void *)0)", "null" },
            { "((void*)0)", "null" },
            { "NULL", "null"},
            { "nullptr", "null"},
            { "ImVec2(0,0)", "new Vector2()" },
            { "ImVec2(0.0f,0.0f)", "new Vector2()" },
            { "ImVec2(-FLT_MIN,0)", "new Vector2(-float.MinValue, 0.0f)" },
            { "ImVec2(-1,0)", "new Vector2(-1, 0)" },
            { "ImVec2(1,0)", "new Vector2(1, 0)" },
            { "ImVec2(1,1)", "new Vector2(1, 1)" },
            { "ImVec2(0,1)", "new Vector2(0, 1)" },
            { "ImVec4(0,0,0,0)", "new Vector4()" },
            { "ImVec4(1,1,1,1)", "new Vector4(1, 1, 1, 1)" },
            { "ImVec4(0,0,0,-1)", "new Vector4(0, 0, 0, -1)" },
            { "ImPlotPoint(0,0)", "new ImPlotPoint { x = 0, y = 0 }" },
            { "ImPlotPoint(1,1)", "new ImPlotPoint { x = 1, y = 1 }" },
            { "ImDrawCornerFlags_All", "ImDrawCornerFlags.All" },
            { "ImPlotFlags_None", "ImPlotFlags.None"},
            { "ImPlotAxisFlags_None", "ImPlotAxisFlags.None"},
            { "ImPlotAxisFlags_NoGridLines", "ImPlotAxisFlags.NoGridLines"},
            { "ImGuiCond_Once", "ImGuiCond.Once"},
            { "ImPlotOrientation_Vertical", "ImPlotOrientation.Vertical"},
            { "PinShape_CircleFilled", "PinShape._CircleFilled"},
            { "ImGuiPopupFlags_None", "ImGuiPopupFlags.None"},
            { "ImGuiNavHighlightFlags_TypeDefault", "ImGuiNavHighlightFlags.TypeDefault"},
            { "ImGuiKeyModFlags_Ctrl", "ImGuiKeyModFlags.Ctrl"},
            { "ImPlotYAxis_1", "ImPlotYAxis._1"},
            { "FLT_MAX", "float.MaxValue" },
            { "(((ImU32)(255)<<24)|((ImU32)(255)<<16)|((ImU32)(255)<<8)|((ImU32)(255)<<0))", "0xFFFFFFFF" },
            { "sizeof(ImU8)", "sizeof(byte)"},
            { "sizeof(ImS8)", "sizeof(sbyte)"},
            { "sizeof(ImU16)", "sizeof(ushort)"},
            { "sizeof(ImS16)", "sizeof(short)"},
            { "sizeof(ImU32)", "sizeof(uint)"},
            { "sizeof(ImS32)", "sizeof(int)"},
            { "sizeof(ImU64)", "sizeof(ulong)"},
            { "sizeof(ImS64)", "sizeof(long)"}
        };

        public static readonly Dictionary<string, string> IdentifierReplacements = new Dictionary<string, string>()
        {
            { "in", "_in" },
            { "out", "_out" },
            { "ref", "_ref" },
        };

        public static readonly HashSet<string> LegalFixedTypes = new HashSet<string>()
        {
            "byte",
            "sbyte",
            "char",
            "ushort",
            "short",
            "uint",
            "int",
            "ulong",
            "long",
            "float",
            "double",
        };

        public static readonly HashSet<string> SkippedFunctions = new HashSet<string>()
        {
            "igInputText",
            "igInputTextMultiline",
            "igCalcTextSize",
            "igInputTextWithHint"
        };
    }
}
