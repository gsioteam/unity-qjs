using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace qjs {
    public struct FileInfo
    {
        public string link;
        public long size;
        public long offset;
    }

    public class AsarAsset : ScriptableObject
    {
        public byte[] bytes;

        private Dictionary<string, FileInfo> files;
        private uint contentOffset;

        public Dictionary<string, FileInfo> Files
        {
            get
            {
                if (files == null && bytes != null)
                {
                    files = new Dictionary<string, FileInfo>();
                    BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
                    uint s = reader.ReadUInt32();
                    if (s == 4)
                    {
                        uint length = reader.ReadUInt32();
                        s = reader.ReadUInt32();
                        if (s + 4 == length)
                        {
                            int strlen = reader.ReadInt32();
                            if (strlen <= length)
                            {
                                char[] chs = reader.ReadChars(strlen);
                                string str = new string(chs);
                                JSONNode json = JSON.Parse(str);
                                contentOffset = length + 8;
                                if (json.IsObject)
                                {
                                    processFile("", json.AsObject);
                                }
                            }
                            else
                            {
                                Debug.LogError("Wrong binary");
                            }
                        }
                        else
                        {
                            Debug.LogError("Wrong binary");
                        }
                    }
                    else
                    {
                        Debug.LogError("Wrong binary");
                    }
                }
                return files;
            }
        }

        private void processFile(string path, JSONObject obj)
        {
            if (obj.HasKey("files"))
            {
                var files = obj["files"].AsObject;
                var en = files.GetEnumerator();
                while (en.MoveNext())
                {
                    string key = en.Current.Key;
                    var subnode = en.Current.Value;
                    processFile(path + key + "/", subnode.AsObject);
                }
            }
            else
            {
                if (path[path.Length - 1] == '/')
                {
                    path = path.Substring(0, path.Length - 1);
                }
                FileInfo fileInfo = new FileInfo();
                if (obj.HasKey("link"))
                {
                    fileInfo.link = obj["link"];
                } else
                {
                    fileInfo.offset = obj["offset"];
                    fileInfo.size = obj["size"];
                }
                files[path] = fileInfo;
            }
        }

        public string ReadString(FileInfo file)
        {
            if (file.link != null)
            {
                file = Files[file.link];
            }
            return Encoding.UTF8.GetString(bytes, (int)(file.offset + contentOffset), (int)file.size);
        }
    }
}
