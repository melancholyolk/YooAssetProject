using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

namespace YooAsset.Editor
{
    [Serializable]
    public class TypeInfo
    {
        [SerializeField]
        private string _type;
        [SerializeField]
        private int _key;

        public string Type
        {
            get => _type;
        }

        public int Key
        {
            get => _key;
            set => _key = value;
        }

        public TypeInfo(string type, int key)
        {
            this._type = type;
            this._key = key;
        }
    }
    
    [Serializable]
    public class AssetCollectorType
    {
        public List<TypeInfo> typeInfos = new List<TypeInfo>();
        
        public TypeInfo this[int key]
        {
            get
            {
                var item = typeInfos.FirstOrDefault(ti => ti.Key == key);
                if (item.Equals(default(TypeInfo)))
                    throw new KeyNotFoundException("未找到指定的key。");
                return item;
            }
            set
            {
                var index = typeInfos.FindIndex(ti => ti.Key == key);
                if (index != -1)
                    typeInfos[index] = value;
                else
                    typeInfos.Add(value);
            }
        }
        
        public TypeInfo this[long key]
        {
            get
            {
                var item = typeInfos.FirstOrDefault(ti => ti.Key == key);
                if (item.Equals(default(TypeInfo)))
                    throw new KeyNotFoundException("未找到指定的key。");
                return item;
            }
            set
            {
                var index = typeInfos.FindIndex(ti => ti.Key == key);
                if (index != -1)
                    typeInfos[index] = value;
                else
                    typeInfos.Add(value);
            }
        }
        
        public TypeInfo this[string type]
        {
            get
            {
                var item = typeInfos.FirstOrDefault(ti => ti.Type == type);
                if (item.Equals(default(TypeInfo)))
                    throw new KeyNotFoundException("未找到指定的key。");
                return item;
            }
            set
            {
                var index = typeInfos.FindIndex(ti => ti.Type == type);
                if (index != -1)
                    typeInfos[index] = value;
                else
                    typeInfos.Add(value);
            }
        }

        public bool ContainType(string type)
        {
            foreach (var typeInfo in typeInfos)
            {
                if (typeInfo.Type == type)
                    return true;
            }
            return false;
        }

        public bool ContainKey(int Key)
        {
            foreach (var typeInfo in typeInfos)
            {
                if (typeInfo.Key == Key)
                    return true;
            }
            return false;
        }

        public void RemoveByKey(int key)
        {
            for (int i = typeInfos.Count - 1; i >= 0; i--)
            {
                if(typeInfos[i].Key == key)
                    typeInfos.RemoveAt(i);
            }
        }
        
        public void RemoveByType(string type)
        {
            for (int i = typeInfos.Count - 1; i >= 0; i--)
            {
                if(typeInfos[i].Type == type)
                    typeInfos.RemoveAt(i);
            }
        }

        public bool Add(string type,int Key)
        {
            if (ContainType(type) || ContainKey(Key))
                return false;
            TypeInfo typeInfo = new TypeInfo(type,Key);
            typeInfos.Add(typeInfo);
            return true;
        }
    }
}

