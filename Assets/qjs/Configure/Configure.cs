using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace qjs
{
    class FilesIndex : RequireLoader, Configure.ResourceIndex
    {
        Dictionary<string, string> files;
        Dictionary<TextAsset, string> index;
        List<AsarAsset> asars;
        HashSet<string> tree;

        public FilesIndex(List<FileKeyValue> list, List<AsarAsset> asars)
        {
            files = new Dictionary<string, string>();
            index = new Dictionary<TextAsset, string>();
            tree = new HashSet<string>();
            if (list != null)
            {
                foreach (var file in list)
                {
                    files[file.key] = file.value.text;
                    index[file.value] = file.key;
                    tree.Add(file.key);
                }
            }
            if (asars != null)
            {
                this.asars = asars;
                foreach (var asar in asars)
                {
                    foreach (var key in asar.Files.Keys)
                        tree.Add(key);
                }
            } else
            {
                this.asars = new List<AsarAsset>();
            }
        }

        public string FindIndex(TextAsset text)
        {
            string res;
            if (index.TryGetValue(text, out res))
            {
                return res;
            }
            return null;
        }

        public bool Test(string filename)
        {
            return tree.Contains(filename);
        }

        public string Load(string filename)
        {
            string text;
            if (files.TryGetValue(filename, out text))
            {
                return text;
            }
            foreach (var asar in asars)
            {
                FileInfo file;
                if (asar.Files.TryGetValue(filename, out file))
                {
                    return asar.ReadString(file);
                }
            }
            return null;
        }
    }

    [Serializable]
    public class FileKeyValue
    {

        public string key;
        public TextAsset value;
    }

    [DefaultExecutionOrder(-50)]
    [ExecuteInEditMode]
    public abstract class Configure : MonoBehaviour
    {

        public interface ResourceIndex
        {
            string FindIndex(TextAsset text);
        }

        private static ResourceIndex index;
        public static ResourceIndex Index { get => index; }

        [SerializeField]
        private List<FileKeyValue> files;

        [SerializeField]
        private List<AsarAsset> asars;

        protected virtual Action _typeRegister { get; }

        protected void OnEnable()
        {
            if (_typeRegister != null)
            {
                _typeRegister();
            }
            
            OnRegisterClass((type, _) => QuickJS.Instance.RegisterClass(type));
            FilesIndex filesIndex = new FilesIndex(files, asars);
            QuickJS.Instance.Loader = filesIndex;
            index = filesIndex;
        }

        public abstract void OnRegisterClass(Action<Type, HashSet<string>> RegisterClass);

        public void addAsar(AsarAsset asar)
        {
            if (!asars.Contains(asar))
            {
                asars.Insert(0, asar);
            }
        }
    }

}
