using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using UnityEditor;
using UnityEngine;

namespace qjs
{
    class CodeGenerator
    {
        private int inset = 0;
        private StringWriter stringWriter = new StringWriter();

        public CodeGenerator(Type type)
        {
            WriteLine(String.Format("public static class _Gen{0} {{", type.Name));
            Enter();
            WriteLine("public static bool doNotModify = true;");
            WriteLine("public static void Register() {");
            Enter();
            WriteLine("if (doNotModify) return;");
            WriteLine("object o = null;");
        }

        public void WriteLine(String line)
        {
            for (int i = 0; i < inset; ++i)
            {
                stringWriter.Write("    ");
            }
            stringWriter.WriteLine(line);
        }

        public void Enter()
        {
            ++inset;
        }

        public void Exit()
        {
            --inset;
        }

        private int paramCount = 0;
        public string NewParam()
        {
            paramCount++;
            return "p" + paramCount.ToString("X");
        }

        private bool complete = false;

        public void End()
        {
            if (!complete)
            {
                complete = true;
                Exit();
                WriteLine("}");
                Exit();
                WriteLine("}");
            }
        }

        public override string ToString()
        {
            return stringWriter.ToString();
        }
    }

    struct Asset
    {
        public string path;
        public string relativePath;
        public TextAsset text;
    }

    public class InputEditorWindow : EditorWindow
    {
        public string text;
        public delegate void OnConfirm();
        public OnConfirm onConfirm;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Prefix path of scripts.");

            text = EditorGUILayout.TextField(text);

            Rect rect = EditorGUILayout.GetControlRect(false, 24);
            if (GUI.Button(new Rect(rect.x, rect.y, rect.width / 2, rect.height), "Confirm"))
            {
                if (onConfirm != null) onConfirm();
                Close();
            }

            if (GUI.Button(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height), "Cancel"))
                Close();
        }
    }

    [ExecuteInEditMode]
    public class TempBehaviour : MonoBehaviour
    {
        public string targetType;
        float timeCounter = 0;

        private void OnEnable()
        {
            timeCounter = 0;
            EditorApplication.update += CheckType;
        }

        void CheckType()
        {
            timeCounter += Time.deltaTime;
            if (timeCounter > 0.5)
            {
                timeCounter = 0;
                Type type;
                if ((type = GetTypeByName(targetType)) != null)
                {
                    gameObject.AddComponent(type);
                    DestroyImmediate(this);
                    EditorApplication.update -= CheckType;
                }
            }
        }

        private static Type GetTypeByName(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.FullName == name)
                    {
                        return type;
                    }
                }
            }

            return null;
        }
    }

    [CustomEditor(typeof(Configure), true)]
    public class ConfigureEditor : Editor
    {
        Vector2 scrollPosition = Vector2.zero;
        Vector2 scrollPosition2 = Vector2.zero;
        SerializedProperty filesProperty, asarsProperty;

        [MenuItem("GameObject/Create QJS Configure", false, 11)]
        static void CreateConfigure()
        { 
            string path = EditorUtility.SaveFilePanelInProject("New a configure", "MyConfigure", "cs", "");
            if (path == null || path.Length == 0) return;
            string name = Path.GetFileNameWithoutExtension(path);
            File.WriteAllLines(path, new string[] {
                "using System;",
                "using System.Collections.Generic;",
                "",
                "public class "+name+" : qjs.Configure",
                "{",
                "    //protected override Action _typeRegister",
                "    public override void OnRegisterClass(Action<Type, HashSet<string>> RegisterClass)",
                "    {",
                "        ",
                "    }",
                "}"
            });
            AssetDatabase.Refresh();

            GameObject gameObject = new GameObject();
            gameObject.name = name;
            TempBehaviour temp = gameObject.AddComponent<TempBehaviour>();
            temp.targetType = name;

        }

        [MenuItem("Assets/Create/QJS Script", priority = 81)]
        static void CreateJsFile()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Assets/qjs/Editor/QJS_Temp.js", path + "/NewScript.js");
        }

        private void OnEnable()
        {
            
            filesProperty = serializedObject.FindProperty("files");
            asarsProperty = serializedObject.FindProperty("asars");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            const float LineSize = 20;

            Rect rect;

            rect = EditorGUILayout.GetControlRect(false, LineSize + 8);
            GUI.Label(new Rect(rect.x, rect.y + 4, LineSize, rect.height - 8), "+");
            var resource = EditorGUI.ObjectField(new Rect(rect.x + LineSize, rect.y, rect.width - LineSize, rect.height), null, typeof(TextAsset), true);
            if (resource != null)
            {
                string path = AssetDatabase.GetAssetPath(resource);
                int index = filesProperty.arraySize;
                filesProperty.InsertArrayElementAtIndex(0);
                var pro = filesProperty.GetArrayElementAtIndex(0);

                pro.FindPropertyRelative("key").stringValue = path.Substring("Assets".Length + 1);
                pro.FindPropertyRelative("value").objectReferenceValue = resource;
            }

            EditorGUILayout.LabelField("Drag file to above for adding script.", new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10
            });

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUI.skin.scrollView, GUILayout.MinHeight(120), GUILayout.MaxHeight(120));


            if (filesProperty.arraySize == 0)
            {
                EditorGUILayout.LabelField("No file is configured, add a file or folder.", new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter
                });
            } else
            {
                List<int> willRemove = null;
                for (int i = 0, t = filesProperty.arraySize; i < t; ++i)
                {
                    var elementProperty = filesProperty.GetArrayElementAtIndex(i);
                    rect = EditorGUILayout.GetControlRect(false, LineSize);
                    float width = rect.width - LineSize;
                    var keyProperty = elementProperty.FindPropertyRelative("key");
                    var valueProperty = elementProperty.FindPropertyRelative("value");
                    keyProperty.stringValue = EditorGUI.TextField(new Rect(rect.x, rect.y, width / 2 - 1, LineSize), keyProperty.stringValue);
                    valueProperty.objectReferenceValue = EditorGUI.ObjectField(new Rect(rect.x + width / 2, rect.y, width / 2, LineSize), valueProperty.objectReferenceValue, typeof(TextAsset), false);
                    if (GUI.Button(new Rect(rect.x + width, rect.y, LineSize, LineSize), "-"))
                    {
                        if (willRemove == null)
                        {
                            willRemove = new List<int>();
                        }
                        willRemove.Add(i);
                    }
                    rect = EditorGUILayout.GetControlRect(false, 1);
                    EditorGUI.DrawRect(rect, Color.gray);
                }
                if (willRemove != null)
                {
                    willRemove.Reverse();
                    willRemove.ForEach((idx) => {
                        filesProperty.DeleteArrayElementAtIndex(idx);
                    });
                }

            }

            EditorGUILayout.EndScrollView();
            rect = EditorGUILayout.GetControlRect(false, LineSize);
            
            if (GUI.Button(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height), "+.."))
            {
                string path = EditorUtility.OpenFolderPanel("Select a folder", "Assets", "");

                if (Application.dataPath == path || path.StartsWith(Application.dataPath + "/"))
                {
                    string prefix = path.Replace(Application.dataPath + "/", "");
                    InputEditorWindow inputEditorWindow = EditorWindow.CreateInstance<InputEditorWindow>();
                    inputEditorWindow.titleContent = new GUIContent("Prefix path");
                    inputEditorWindow.text = prefix;
                    inputEditorWindow.onConfirm = () => {
                        var list = new List<Asset>();
                        findResource(path, ref list);
                        Debug.Log("what ? " + list.Count);
                        for (int i = 0, t = list.Count; i < t; ++i)
                        {
                            var asset = list[i];
                            string key = asset.path.Substring(Application.dataPath.Length + 1);
                            if (key.StartsWith(prefix))
                            {
                                string preText = inputEditorWindow.text;
                                if (!preText.EndsWith("/") && preText.Length > 0)
                                    preText = preText + "/";
                                key = preText + key.Substring(prefix.Length + 1);
                                filesProperty.InsertArrayElementAtIndex(0);
                                var pro = filesProperty.GetArrayElementAtIndex(0);
                                pro.FindPropertyRelative("key").stringValue = key;
                                pro.FindPropertyRelative("value").objectReferenceValue = asset.text;
                            }
                        }
                    };
                    inputEditorWindow.ShowModal();
                } else
                {
                    EditorUtility.DisplayDialog("Wrong path", "Please select a folder in project", "Ok");
                }
            }

            if (serializedObject.targetObjects.Length == 1)
            {
                rect = EditorGUILayout.GetControlRect(true, 1);
                EditorGUI.DrawRect(rect, Color.gray);

                rect = EditorGUILayout.GetControlRect(false, LineSize + 8);
                GUI.Label(new Rect(rect.x, rect.y + 4, LineSize, rect.height - 8), "+");
                AsarAsset asar = EditorGUI.ObjectField(new Rect(rect.x + LineSize, rect.y, rect.width - LineSize, rect.height), null, typeof(AsarAsset), false) as AsarAsset;
                if (asar != null)
                {
                    bool has = false;
                    for (int i = 0, t = asarsProperty.arraySize; i < t; ++i)
                    {
                        if (asarsProperty.GetArrayElementAtIndex(i).objectReferenceValue == asar)
                        {
                            has = true;
                            break;
                        }
                    }
                    if (!has)
                    {
                        asarsProperty.InsertArrayElementAtIndex(0);
                        var pro = asarsProperty.GetArrayElementAtIndex(0);
                        pro.objectReferenceValue = asar;
                    }
                }
                EditorGUILayout.LabelField("Drag file to above for adding asar.", new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10
                });

                scrollPosition2 = EditorGUILayout.BeginScrollView(scrollPosition2, GUILayout.MaxHeight(120));

                List<int> willRemove = new List<int>();
                for (int i = 0, t = asarsProperty.arraySize; i < t; ++i)
                {
                    var pro = asarsProperty.GetArrayElementAtIndex(i);
                    rect = EditorGUILayout.GetControlRect();
                    GUI.enabled = false;
                    EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width - LineSize, rect.height), pro.objectReferenceValue, typeof(AsarAsset), false);
                    GUI.enabled = true;
                    if (GUI.Button(new Rect(rect.xMax - LineSize, rect.y, LineSize, rect.height), "-"))
                    {
                        willRemove.Add(i);
                    } else if (pro.objectReferenceValue == null)
                    {
                        willRemove.Add(i);
                    }
                    willRemove.Reverse();
                    willRemove.ForEach((idx) => {
                        asarsProperty.DeleteArrayElementAtIndex(idx);
                    });
                }

                EditorGUILayout.EndScrollView();

                rect = EditorGUILayout.GetControlRect(true, 26);
                if (GUI.Button(rect, "Process register"))
                {
                    string path = assetPath(serializedObject.targetObject);
                    if (path != null)
                    {
                        Type configureType = typeof(Configure);

                        CodeGenerator generator = new CodeGenerator(serializedObject.targetObject.GetType());
                        Action<Type, HashSet<string>> registerHandler = (Type type, HashSet<string> banlist) => {
                            if (!type.IsGenericType)
                            {
                                generator.WriteLine("{");
                                generator.Enter();

                                generator.WriteLine(string.Format("{0} v = ({0})o;", type.FullName));
                                foreach (var constructor in type.GetConstructors())
                                {
                                    var obsolates = constructor.GetCustomAttributes(typeof(ObsoleteAttribute), true);
                                    if (obsolates.Length > 0)
                                        continue;
                                    if (constructor.IsPublic)
                                    {
                                        var argv = constructor.GetParameters();
                                        List<string> vs = new List<string>();
                                        vs.Add(string.Format("v = new {0}(", type.FullName));
                                        for (int i = 0, t = argv.Length; i < t; ++i)
                                        {
                                            var p = argv[i];
                                            if (i == 0)
                                            {
                                                vs.Add(string.Format("({0})o", p.ParameterType.FullName));
                                            } else
                                            {
                                                vs.Add(string.Format(",({0})o", p.ParameterType.FullName));
                                            }
                                        }
                                        vs.Add(");");
                                        generator.WriteLine(string.Join("", vs));
                                    }
                                }

                                foreach (var property in type.GetProperties())
                                {
                                    if (banlist != null && banlist.Contains(property.Name)) continue;
                                    var obsolates = property.GetCustomAttributes(typeof(ObsoleteAttribute), true);
                                    if (obsolates.Length > 0)
                                        continue;
                                    if (property.CanRead)
                                    {
                                        var method = property.GetGetMethod();
                                        if (isMethodAviaible(method))
                                        {
                                            if (method.IsStatic)
                                            {
                                                generator.WriteLine(string.Format("var {0} = {2}.{1};", generator.NewParam(), property.Name, type.FullName));
                                            } else
                                            {
                                                generator.WriteLine(string.Format("var {0} = v.{1};", generator.NewParam(), property.Name));
                                            }
                                        }
                                    }
                                    if (property.CanWrite)
                                    {
                                        var method = property.GetSetMethod();
                                        if (isMethodAviaible(method))
                                        {
                                            if (method.IsStatic)
                                            {
                                                generator.WriteLine(string.Format("{2}.{0} = ({1})o;", property.Name, property.PropertyType.FullName, type.FullName));
                                            } else
                                            {
                                                generator.WriteLine(string.Format("v.{0} = ({1})o;", property.Name, property.PropertyType.FullName));
                                            }
                                        }
                                    }
                                }

                                foreach (var method in type.GetMethods())
                                {
                                    if (banlist != null && banlist.Contains(method.Name)) continue;
                                    if (!isMethodAviaible(method)) continue;
                                    if (method.IsPublic && !method.IsConstructor && !method.IsGenericMethod)
                                    {
                                        if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
                                            continue;
                                        if (method.Name.StartsWith("op_"))
                                        {

                                        } else
                                        {
                                            if (method.IsStatic)
                                            {
                                                var argv = method.GetParameters();
                                                List<string> vs = new List<string>();
                                                vs.Add(string.Format("{0}.{1}(", type.FullName, method.Name));
                                                for (int i = 0, t = argv.Length; i < t; ++i)
                                                {
                                                    var p = argv[i];
                                                    if (i != 0)
                                                    {
                                                        vs.Add(",");
                                                    }
                                                    string typeName = getTypeName(p.ParameterType);
                                                    if (p.ParameterType.IsByRef)
                                                    {
                                                        string pname = generator.NewParam();
                                                        generator.WriteLine(string.Format("{0} {1} = ({0})o;", typeName, pname));
                                                        if (p.IsOut)
                                                        {
                                                            vs.Add(string.Format("out {0}", pname));
                                                        }
                                                        else
                                                        {
                                                            vs.Add(string.Format("ref {0}", pname));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        vs.Add(string.Format("({0})o", typeName));
                                                    }
                                                }
                                                vs.Add(");");
                                                generator.WriteLine(string.Join("", vs));
                                            } else
                                            {
                                                var argv = method.GetParameters();
                                                List<string> vs = new List<string>();
                                                vs.Add(string.Format("v.{0}(", method.Name));
                                                for (int i = 0, t = argv.Length; i < t; ++i)
                                                {
                                                    var p = argv[i];
                                                    if (i != 0)
                                                    {
                                                        vs.Add(",");
                                                    }
                                                    string typeName = getTypeName(p.ParameterType);
                                                    if (p.ParameterType.IsByRef)
                                                    {
                                                        string pname = generator.NewParam();
                                                        generator.WriteLine(string.Format("{0} {1} = ({0})o;", typeName, pname));
                                                        if (p.IsOut)
                                                        {
                                                            vs.Add(string.Format("out {0}", pname));
                                                        }
                                                        else
                                                        {
                                                            vs.Add(string.Format("ref {0}", pname));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        vs.Add(string.Format("({0})o", typeName));
                                                    }
                                                }
                                                vs.Add(");");
                                                generator.WriteLine(string.Join("", vs));
                                            }
                                        }
                                    }
                                }

                                generator.Exit();
                                generator.WriteLine("}");
                            }
                        };
                        (serializedObject.targetObject as Configure).OnRegisterClass(registerHandler);
                        generator.End();

                        File.WriteAllText(path.Replace(".cs", "_Gen.cs"), generator.ToString());
                        List<string> lines = new List<string>(File.ReadAllLines(path));

                        int classBeginLine = -1, registerLine = -1; 
                        for (int i = 0, t = lines.Count; i < t; ++i)
                        {
                            string line = lines[i];
                            if (isClassLine(line, serializedObject.targetObject.GetType()))
                            {
                                if (line.Contains("{")) classBeginLine = i;
                                else classBeginLine = i + 1;
                            } else if (line.IndexOf("override Action _typeRegister") >= 0)
                            {
                                registerLine = i;
                            }
                        }

                        string newLine = string.Format("    protected override Action _typeRegister => _Gen{0}.Register;", serializedObject.targetObject.GetType().Name);
                        if (registerLine >= 0)
                        {
                            lines[registerLine] = newLine;
                        } else
                        {
                            lines.Insert(classBeginLine + 1, newLine);
                        }
                        File.WriteAllLines(path, lines);

                        AssetDatabase.Refresh();
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        bool isMethodAviaible(MethodBase method)
        {
            if (method.IsPublic && !method.IsGenericMethod)
            {
                var obsolates = method.GetCustomAttributes(typeof(ObsoleteAttribute), true);
                if (obsolates.Length > 0)
                    return false;
                var flags = method.GetMethodImplementationFlags();
                if (flags.HasFlag(MethodImplAttributes.InternalCall))
                    return false;
                if (!method.IsSecurityTransparent) return false;

                return true;
            }
            return false;
        }

        bool isClassLine(string line, Type type)
        {
            int classIndex = line.IndexOf(" class ");
            if (classIndex >= 0)
            {
                int nameIndex = line.IndexOf(" " + type.Name + " ", classIndex);
                return nameIndex >= 0;
            }
            return false;
        }

        string assetPath(UnityEngine.Object target)
        {
            string[] paths = AssetDatabase.GetAllAssetPaths();
            foreach (string path in paths)
            {
                if (Path.GetExtension(path) == ".cs" && Path.GetFileNameWithoutExtension(path) == target.GetType().Name)
                {
                    return path;
                }
            }
            return null;
        }

        string getTypeName(Type type)
        {
            while (type.IsByRef) type = type.GetElementType();
            if (type.IsGenericType)
            {
                StringWriter stringWriter = new StringWriter();
                string ns = type.Namespace;
                string name = type.Name.Substring(0, type.Name.IndexOf('`'));
                if (ns == null)
                {
                    stringWriter.Write(name + "<");
                } else
                {
                    stringWriter.Write(ns + "." + name + "<");
                }
                var types = type.GetGenericArguments();
                for (int i = 0, t = types.Length; i < t; ++i)
                {
                    if (i == 0)
                    {
                        stringWriter.Write(getTypeName(types[i]));
                    } else
                    {
                        stringWriter.Write("," + getTypeName(types[i]));
                    }
                }
                stringWriter.Write(">");
                return stringWriter.ToString();
            } else
            {
                string ns = type.Namespace;
                if (ns == null)
                {
                    return type.Name;
                }
                else
                {
                    return ns + "." + type.Name;
                }
            }
        }

        void findResource(string path, ref List<Asset> list)
        {
            var paths = Directory.EnumerateFiles(path);
            foreach (var subpath in paths)
            {
                var attr = File.GetAttributes(subpath);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    findResource(subpath, ref list);
                } else
                {
                    if (subpath.EndsWith(".js"))
                    {
                        string replacePath = subpath.Replace(Application.dataPath, "Assets");
                        TextAsset text = AssetDatabase.LoadAssetAtPath<TextAsset>(replacePath);
                        if (text != null)
                        {
                            list.Add(new Asset()
                            {
                                path = subpath,
                                relativePath = replacePath,
                                text = text
                            });
                        }
                    }
                }
            }
        }
    }
}