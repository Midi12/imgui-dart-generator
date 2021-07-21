using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace imgui_dart_generator
{
    class Program
    {
        private static string fileExt = ".g.dart";
        static void Main(string[] args)
        {
            string libraryName = "cimgui";

            string inputPath = args.Length > 0 ? Path.Combine(AppContext.BaseDirectory, args[0]) : Path.Combine(AppContext.BaseDirectory, "definitions", libraryName);
            
            string outputPath = args.Length > 1 ? Path.Combine(AppContext.BaseDirectory, args[1]) : Path.Combine(AppContext.BaseDirectory, "output", libraryName);
            
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            Console.WriteLine($"Loading definitions from {outputPath}.");

            var defs = new ImguiDefinitions(inputPath);


            Console.WriteLine("Generating enums ...");
            GenerateEnums(defs, outputPath);

            Console.WriteLine("Generating types ...");
            GenerateTypes(defs, outputPath);

            Console.WriteLine("Generating functions ...");
            GenerateFunctions(defs, outputPath);

            Console.WriteLine($"Generated files to {outputPath}.");
        }

        private static void GenerateFunctions(ImguiDefinitions defs, string outputPath)
        {
            using (DartCodeWriter writer = new DartCodeWriter(Path.Combine(outputPath, $"ImGui{fileExt}")))
            {
                writer.WriteLine("import 'dart:ffi';");

                writer.WriteLine();
                
                // declare dll
                writer.WriteLine("final _cimgui = DynamicLibrary.open('cimgui.dll');");
                writer.WriteLine();


                foreach (FunctionDefinition fd in defs.Functions)
                {
                    foreach (OverloadDefinition overload in fd.Overloads)
                    {
                        string exportedName = overload.ExportedName;
                        if (exportedName.Contains("~")) { continue; }
                        if (exportedName.Contains("ImVector_")) { continue; }
                        if (exportedName.Contains("ImChunkStream_")) { continue; }

                        if (overload.Parameters.Any(tr => tr.Type.Contains('('))) { continue; } // TODO: Parse function pointer parameters.

                        string ret = GetTypeString(overload.ReturnType, false);

                        bool hasVaList = false;
                        List<string> nativeParamParts = new List<string>();
                        List<string> dartParamParts = new List<string>();
                        List<string> paramParts = new List<string>();
                        for (int i = 0; i < overload.Parameters.Length; i++)
                        {
                            TypeReference p = overload.Parameters[i];

                            string paramType = GetTypeString(p.Type, p.IsFunctionPointer);

                            if (paramType == "va_list")
                            {
                                hasVaList = true;
                                break;
                            }

                            if (p.ArraySize != 0)
                            {
                                paramType = paramType + "*";
                            }

                            if (p.Name == "...") { continue; }

                            string identifier = CorrectIdentifier(p.Name);
                            paramParts.Add(identifier);
                            nativeParamParts.Add($"{MapFFIType(paramType)} {identifier}");
                            dartParamParts.Add($"{MapIntegralType(paramType)} {identifier}");
                        }

                        if (hasVaList) { continue; }

                        string parameters = string.Join(", ", paramParts);
                        string nativeParameters = string.Join(", ", nativeParamParts);
                        string dartParameters = string.Join(", ", dartParamParts);

                        bool isUdtVariant = exportedName.Contains("nonUDT");
                        string methodName = isUdtVariant
                            ? exportedName.Substring(0, exportedName.IndexOf("_nonUDT"))
                            : exportedName;



                        writer.WriteLine("///```c");
                        writer.WriteLine($"/// {ret} {methodName}(");
                        for (int i = 0; i < overload.Parameters.Length; i++)
                        {
                            TypeReference p = overload.Parameters[i];

                            writer.WriteLine($"///  {GetTypeString(p.Type, p.IsFunctionPointer)} {CorrectIdentifier(p.Name)} {(i == overload.Parameters.Length - 1 ? "" : ",")}");
                        }
                        writer.WriteLine("/// );");
                        writer.WriteLine("///```");
                        writer.WriteLine($"{MapIntegralType(ret)} {methodName}({dartParameters}) =>");
                        writer.AddIndentation().WriteLine($"_{methodName}({parameters});").RemoveIndentation();

                        writer.WriteLine();

                        writer.WriteLine($"late final _{methodName} = _cimgui.lookupFunction<");
                        writer.AddIndentation()
                            .WriteLine($"{MapFFIType(ret)} Function({nativeParameters}),")
                            .WriteLine($"{MapIntegralType(ret)} Function({dartParameters})>('{methodName}');")
                            .RemoveIndentation();

                        writer.WriteLine();
                    }
                }

                writer.WriteLine();
            }
        }

        private static string CorrectIdentifier(string identifier)
        {
            if (TypeInfo.IdentifierReplacements.TryGetValue(identifier, out string replacement))
            {
                return replacement;
            }
            else
            {
                return identifier;
            }
        }

        private static void GenerateTypes(ImguiDefinitions defs, string outputPath)
        {
            foreach (TypeDefinition td in defs.Types)
            {
                if (TypeInfo.CustomDefinedTypes.Contains(td.Name)) { continue; }

                using (DartCodeWriter writer = new DartCodeWriter(Path.Combine(outputPath, $"{td.Name}{fileExt}")))
                {
                    // imports
                    writer.WriteLine("import 'dart:ffi';");

                    writer.WriteLine();

                    // class
                    writer.PushBlock($"class {td.Name} extends Struct");
                    writer.WriteLine();

                    // fields
                    foreach (TypeReference field in td.Fields)
                    {
                        string typeStr = GetTypeString(field.Type, field.IsFunctionPointer);

                        if (field.ArraySize != 0)
                        {
                            if (TypeInfo.LegalFixedTypes.Contains(typeStr))
                            {
                                writer.WriteLine($"@Array({field.ArraySize})");
                                writer.WriteLine($"external Array<{MapFFIType(typeStr)}> {Uncapitalize(field.Name)};");
                            }
                            else
                            {
                                for (int i = 0; i < field.ArraySize; i++)
                                {
                                    writer.WriteLine($"external {MapIntegralType(typeStr)} {Uncapitalize(field.Name)}_{i};");
                                }
                            }
                        }
                        else
                        {
                            if (TypeInfo.LegalFixedTypes.Contains(typeStr))
                            {
                                writer.WriteLine($"@{MapFFIType(typeStr)}()");
                            }
                            
                            writer.WriteLine($"external {MapIntegralType(typeStr)} {Uncapitalize(field.Name)};");
                        }

                        writer.WriteLine();
                    }

                    writer.PopBlock();
                }
            }
        }

        private static void GenerateEnums(ImguiDefinitions defs, string outputPath)
        {
            foreach (EnumDefinition ed in defs.Enums)
            {
                using (DartCodeWriter writer = new DartCodeWriter(Path.Combine(outputPath, $"E{ed.FriendlyName}{fileExt}")))
                {
                    writer.PushBlock($"class E{ed.FriendlyName}");
                    foreach (EnumMember member in ed.Members)
                    {
                        string sanitizedName = ed.SanitizeNames(member.Name);
                        string sanitizedValue = ed.SanitizeNames(member.Value);
                        writer.WriteLine($"static const int {sanitizedName} = {sanitizedValue};");
                    }
                    writer.PopBlock();
                }
            }
        }

        private static string GetTypeString(string typeName, bool isFunctionPointer)
        {
            int pointerLevel = 0;
            if (typeName.EndsWith("**")) { pointerLevel = 2; }
            else if (typeName.EndsWith("*")) { pointerLevel = 1; }

            if (!TypeInfo.WellKnownTypes.TryGetValue(typeName, out string typeStr))
            {
                if (TypeInfo.WellKnownTypes.TryGetValue(typeName.Substring(0, typeName.Length - pointerLevel), out typeStr))
                {
                    typeStr = typeStr + new string('*', pointerLevel);
                }
                else if (!TypeInfo.WellKnownTypes.TryGetValue(typeName, out typeStr))
                {
                    typeStr = typeName;
                    if (isFunctionPointer) { typeStr = "IntPtr"; }
                }
            }

            return typeStr;
        }

        private static bool GetWrappedType(string nativeType, out string wrappedType)
        {
            if (nativeType.StartsWith("Im") && nativeType.EndsWith("*") && !nativeType.StartsWith("ImVector"))
            {
                int pointerLevel = nativeType.Length - nativeType.IndexOf('*');
                if (pointerLevel > 1)
                {
                    wrappedType = null;
                    return false; // TODO
                }
                string nonPtrType = nativeType.Substring(0, nativeType.Length - pointerLevel);

                if (TypeInfo.WellKnownTypes.ContainsKey(nonPtrType))
                {
                    wrappedType = null;
                    return false;
                }

                wrappedType = nonPtrType + "Ptr";

                return true;
            }
            else
            {
                wrappedType = null;
                return false;
            }
        }

        private static bool IsStringFieldName(string name)
        {
            return Regex.IsMatch(name, ".*Filename.*")
                || Regex.IsMatch(name, ".*Name");
        }

        private static int GetIndex(TypeReference[] parameters, string key)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (key == parameters[i].Name) { return i; }
            }

            throw new InvalidOperationException();
        }

        private static string Uncapitalize(string str)
        {
            return str.Substring(0, 1).ToLowerInvariant() + str.Substring(1, str.Length - 1);
        }

        private static string MapIntegralType(string type)
        {
            string mappedType = type;

            bool IsPointer(string typeStr) => typeStr.EndsWith("*");

            if (IsPointer(type))
            {
                int levels = type.Count(c => (c == '*'));
                mappedType = String.Join("", Enumerable.Repeat("Pointer<", levels)) + MapFFIType(type.Replace("*", "")) + String.Join("", Enumerable.Repeat(">", levels));
            }
            else
            {
                switch (type)
                {
                    case "byte":
                    case "sbyte":
                    case "char":
                    case "ushort":
                    case "short":
                    case "uint":
                    case "int":
                    case "ulong":
                    case "long":
                        mappedType = "int";
                        break;
                    case "float":
                    case "double":
                        mappedType = "double";
                        break;
                    default:
                        break;
                }
            }

            return mappedType;
        }

        private static string MapFFIType(string type)
        {
            string mappedType = type;

            bool IsPointer(string typeStr) => typeStr.EndsWith("*");

            if (IsPointer(type))
            {
                int levels = type.Count(c => (c == '*'));
                mappedType = String.Join("", Enumerable.Repeat("Pointer<", levels)) + MapFFIType(type.Replace("*", "")) + String.Join("", Enumerable.Repeat(">", levels));
            }
            else
            {
                switch (type)
                {
                    case "byte":
                        mappedType = "Uint8";
                        break;
                    case "sbyte":
                        mappedType = "Int8";
                        break;
                    case "char":
                        mappedType = "Uint8";
                        break;
                    case "ushort":
                        mappedType = "Uint16";
                        break;
                    case "short":
                        mappedType = "Int16";
                        break;
                    case "uint":
                        mappedType = "Uint32";
                        break;
                    case "int":
                        mappedType = "Int32";
                        break;
                    case "ulong":
                        mappedType = "Uint64";
                        break;
                    case "long":
                        mappedType = "Int64";
                        break;
                    case "float":
                        mappedType = "Float";
                        break;
                    case "double":
                        mappedType = "Double";
                        break;
                    default:
                        break;
                }
            }

            return mappedType;
        }
    }
}
