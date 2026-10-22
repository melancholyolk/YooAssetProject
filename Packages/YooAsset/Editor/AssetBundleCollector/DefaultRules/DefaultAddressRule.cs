using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    [DisplayName("定位地址: 禁用")]
    public class AddressDisable : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            return string.Empty;
        }
    }

    [DisplayName("定位地址: 文件名")]
    public class AddressByFileName : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            return Path.GetFileNameWithoutExtension(data.AssetPath);
        }
    }

    [DisplayName("定位地址: 分组名_文件名")]
    public class AddressByGroupAndFileName : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
            return $"{data.GroupName}_{fileName}";
        }
    }

    [DisplayName("定位地址: 文件夹名_文件名")]
    public class AddressByFolderAndFileName : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
            FileInfo fileInfo = new FileInfo(data.AssetPath);
            return $"{fileInfo.Directory.Name}_{fileName}";
        }
    }
    
    [DisplayName("定位地址: 路径")]
    public class AddressByPath : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            return Path.ChangeExtension(data.AssetPath.Replace("Assets/", string.Empty), null);
        }
    }
    
    [DisplayName("定位地址: 路径")]
    public class AddressByShaderName : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            var shaderPath = data.AssetPath;

            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
            if (shader != null)
            {
                return shader.name;
            }
            
            return Path.GetFileNameWithoutExtension(shaderPath);
        }
    }

    public class AddressWithData { }
    
    [DisplayName("定位地址：正则")]
    public class AddressByRegular : AddressWithData, IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            Match match = Regex.Match(data.AssetPath, data.Regular);
            var result = match.Success ? match.Groups[1].Value : null;
            return result;
        }
    }
    
    [DisplayName("定位地址：正则")]
    public class AddressByRegularWithGroupName : AddressWithData,IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            Match match = Regex.Match(data.AssetPath, data.Regular);
            var result = match.Success ? match.Groups[1].Value : null;
            return $"{data.GroupName}/{result}";
        }
    }
        
    [DisplayName("定位地址：Data + Name")]
    public class AddressByFileNameAndData : AddressWithData,IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            var name = Path.GetFileNameWithoutExtension(data.AssetPath);
            return $"{data.Regular}/{name}";
        }
    }
}