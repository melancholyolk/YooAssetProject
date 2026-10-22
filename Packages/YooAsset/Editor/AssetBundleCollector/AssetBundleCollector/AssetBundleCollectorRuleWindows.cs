using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;

namespace YooAsset.Editor
{

    internal class AssetBundleCollectorRuleWindows : EditorWindow
    {
        private static Action RefreshCallback;
        public static void Open(AssetBundleCollectorGroup group, Action refreshCallback = null)
        {
            _group = group;
            _rule = group.AssetBundleCollectorRule;
            _collectorType = group.AssetCollectorType;
            
            RefreshCallback = refreshCallback;
            _collectorItems = new();

            var window = GetWindow<AssetBundleCollectorRuleWindows>();
            window.minSize = new Vector2(800, 600);
        }

        public static void RefreshGroup(AssetBundleCollectorGroup group)
        {
            if(_root == null || _objectField1 == null || _groupList == null || _refreshButton == null)
                return;
            _group = group;
            _rule = group.AssetBundleCollectorRule;
            _collectorType = group.AssetCollectorType;

            _collectorItems = new();
            UpdateInfo();
        }
        
        private static AssetBundleCollectorGroup _group;
        
        private static AssetBundleCollectorRule _rule;

        private static AssetCollectorType _collectorType;
        
        private const string AssetUXMLFolder = PathDefine.YOOASSET_PATH + "/Editor/AssetBundleCollector/AssetBundleCollector/uxml/{0}";

        private VisualTreeAsset _visualElement;
        
        private VisualElement _listItemTemplate;
        
        private static VisualElement _root;

        private static List<CollectorItem> _collectorItems;

        private static List<string> _extractorTypeNames;

        private static UnityEngine.Object _collectObject;
        
        private static ObjectField _objectField1;
        
        private static ListView _groupList;
        
        private static Button _refreshButton;
        
        private void CreateGUI()
        {
            _root = this.rootVisualElement;

            _extractorTypeNames = AssetExtractorUtils.GetAssetExtractorMaps().Keys.ToList();
            
            _visualElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(string.Format(AssetUXMLFolder,"AssetBundleCollectorSetting.uxml"));
            
            _visualElement.CloneTree(_root);

            _collectorItems = new();
            
            _refreshButton = _root.Q<Button>("refresh");
                        
            _objectField1 = _root.Q<ObjectField>("Path");
            
            _groupList = _root.Q<ListView>("groupList");
            _groupList.makeItem = MakeItem;
            _groupList.bindItem = BindItem; 
            _groupList.destroyItem = DestroyItem;
            _groupList.itemsRemoved += OnItemRemoved;
            
            UpdateInfo();
        }

        private static void UpdateInfo()
        {
            _collectObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_rule.path);
            if (_collectObject != null)
                _collectObject.name = _rule.path;

            _objectField1.SetValueWithoutNotify(_collectObject);
            _objectField1.RegisterValueChangedCallback(evt =>
            {
                _rule.path = AssetDatabase.GetAssetPath(evt.newValue);
                _objectField1.value.name = _rule.path;
            });
            
            _groupList.itemsSource = _rule.collectorRules;
            _groupList.Rebuild();
            
            _refreshButton.clicked += ()=>
            {
                _group.RebuildCollector();
                RefreshCallback?.Invoke();
            };
        }

        private static void OnItemRemoved(IEnumerable<int> enumerable)
        {
            var index = enumerable.ToList().FirstOrDefault();
            var element = _collectorItems.ElementAt(index);
            element.Unbind();
        }
        
        private static VisualElement MakeItem()
        { 
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(string.Format(AssetUXMLFolder,"BundleCollectorSetting.uxml"));
            return asset.Instantiate();
        }
        
        private static void BindItem(VisualElement visualElement, int index)
        {
            var collectorSetting = _rule.collectorRules[index];

            if (collectorSetting == null)
            {
                collectorSetting = new AssetBundleCollectorRule.AutoCollectBundleSetting();
                _rule.collectorRules[index] = collectorSetting;
            }
            
            var collectorItem = new CollectorItem(_collectorType);
            collectorItem.Bind(visualElement, collectorSetting, ApplyProperties, _extractorTypeNames.Count > 0);
            
            _collectorItems.Add(collectorItem);
            
        }

        private static void ApplyProperties() { }
        
        private static void DestroyItem(VisualElement visualElement)
        {
            var index = -1;
            for (var i = 0; i < _collectorItems.Count; i++)
            {
                if (_collectorItems[i].element == visualElement)
                {
                    index = i;
                    break;
                }
            }
            
            if(index<0) return;
            
            _collectorItems[index].Unbind();
            _collectorItems.RemoveAt(index);
        }

        class CollectorItem
        {
            private static AssetCollectorType _collectorType;

            private static Dictionary<string, ECollectorType> _collectTypeMap;
            private static List<string> _collectTypeNames;
            
            private static List<string> _addressRuleNames;
            private static List<string> _packRuleNames;
            private static List<string> _filterRuleNames;
            private static List<string> _collectorTypeNames;
            private static List<string> GetCollectTypeNames()
            {
                if (_collectTypeNames == null)
                {
                    _collectTypeMap = new Dictionary<string, ECollectorType>
                    {
                        { nameof(ECollectorType.MainAssetCollector), ECollectorType.MainAssetCollector },
                        { nameof(ECollectorType.StaticAssetCollector), ECollectorType.StaticAssetCollector },
                        { nameof(ECollectorType.DependAssetCollector), ECollectorType.DependAssetCollector },
                    };
                    _collectTypeNames = _collectTypeMap.Keys.ToList();
                }

                return _collectTypeNames;
            }
            
            private static List<string> GetAddressRuleNames()
            {
                return _addressRuleNames??=AssetBundleCollectorSettingData.GetAddressRuleNames().Select(rule=>rule.ClassName).ToList();
            }
            
            private static List<string> GetPackRuleNames()
            {
                return _packRuleNames??=AssetBundleCollectorSettingData.GetPackRuleNames().Select(rule=>rule.ClassName).ToList();
            }

            private static List<string> GetFilterRuleNames()
            {
                return _filterRuleNames??=AssetBundleCollectorSettingData.GetFilterRuleNames().Select(rule=>rule.ClassName).ToList();
            }

            private static List<string> GetCollectorType()
            {
                if (_collectorTypeNames == null)
                {
                    _collectorTypeNames = new List<string>();
                }
                _collectorTypeNames.Clear();
                foreach (var typeInfo in _collectorType.typeInfos)
                {
                    _collectorTypeNames.Add(typeInfo.Type);
                }
                return _collectorTypeNames;
            }
            

            private TextField _collectorRegular;
            private DropdownField _collectType;
            private DropdownField _addressRules;
            private DropdownField _packRules;
            private DropdownField _filterRules;
            private TextField _packRegularTags;
            private TextField _addressRegular;
            private TextField _collectRegularTags;
            private TextField _assetTags;
            
            private VisualElement _extractorGroup;
            private ListView _extractorList;
            private Toggle _enableExtractor;
            
            
            private Action _onChangeCallback;

            public CollectorItem(AssetCollectorType collectorType)
            {
                _collectorType = collectorType;
            }

            public AssetBundleCollectorRule.AutoCollectBundleSetting setting { get; private set; }
            public VisualElement element { get; private set; }
            public void Unbind()
            {
                _collectorRegular.UnregisterValueChangedCallback(OnCollectorRegularChange);
                _assetTags.UnregisterValueChangedCallback(OnAssetTagsChange);
                _packRegularTags.UnregisterValueChangedCallback(OnPackRegularChange);
                _collectRegularTags.UnregisterValueChangedCallback(OnCollectRegularChange);
                _collectType.UnregisterValueChangedCallback(OnCollectTypeChanged);
                _packRules.UnregisterValueChangedCallback(OnPackRuleChanged);
                _filterRules.UnregisterValueChangedCallback(OnFilterRuleChanged);
                
                _extractorList.itemsAdded -= OnExtractorItemAdd;
                _extractorList.itemsRemoved -= OnExtractorItemRemove;
                _extractorList.makeItem = null;
                _extractorList.bindItem = null;
                _extractorList.itemsSource = null;
                
                _collectorRegular = null;
                _assetTags = null;
                _collectType = null;
                _packRules = null;
                _filterRules = null;
            }
            
            
            public void Bind(VisualElement element, AssetBundleCollectorRule.AutoCollectBundleSetting setting, Action onRemoveItemCallback, bool hasExtractor)
            {
                this.setting = setting;
                this.element = element;
                
                _collectorRegular = element.Q<TextField>("collectFolder");
                _collectorRegular.value = setting.collectorRegular;
                _collectorRegular.RegisterValueChangedCallback(OnCollectorRegularChange);

                _extractorGroup = element.Q<VisualElement>("extractorGroup");
                
                _extractorList = element.Q<ListView>("extractorList");
                _extractorList.itemsAdded += OnExtractorItemAdd;
                _extractorList.itemsRemoved += OnExtractorItemRemove;
                _extractorList.makeItem = OnExtractorItemMake;
                _extractorList.bindItem = OnExtractorItemBind;
                _extractorList.unbindItem = OnExtractorItemUnbind;
                _extractorList.itemsSource = setting.extractorSettings;
                _extractorList.RefreshItems();
                
                _enableExtractor = element.Q<Toggle>("enableExtractor");
                _enableExtractor.RegisterValueChangedCallback(OnExtractorEnableChange);
                _enableExtractor.value = hasExtractor && setting.enableExtractor;
                
                _enableExtractor.style.display = hasExtractor ? DisplayStyle.Flex : DisplayStyle.None;
                
                _assetTags = element.Q<TextField>("assetTags");
                _assetTags.value = setting.assetTags;
                _assetTags.RegisterValueChangedCallback(OnAssetTagsChange);
                
                _addressRegular = element.Q<TextField>("addressRegular");
                _addressRegular.value = setting.addressRegular;
                _addressRegular.RegisterValueChangedCallback(OnAddreeRegularChanged);
                
                _packRegularTags = element.Q<TextField>("packRegular");
                _packRegularTags.value = setting.packRegular;
                _packRegularTags.RegisterValueChangedCallback(OnPackRegularChange);
                
                _collectRegularTags = element.Q<TextField>("collectRegular");
                _collectRegularTags.value = setting.collectRegular;
                _collectRegularTags.RegisterValueChangedCallback(OnCollectRegularChange);

                InitDropDown(ref _collectType, element, "collectType", GetCollectTypeNames(), setting.collectType.ToString(), OnCollectTypeChanged);
                InitDropDown(ref _addressRules, element, "addressRuleName", GetAddressRuleNames(), setting.addressRuleName, OnAddreeRuleChanged);
                InitDropDown(ref _packRules, element, "packRuleName", GetPackRuleNames(), setting.packRuleName, OnPackRuleChanged);
                InitDropDown(ref _filterRules, element, "filterRuleName", GetFilterRuleNames(), setting.filterRuleName, OnFilterRuleChanged);
                InitDropDown(ref _filterRules, element, "type", GetCollectorType(), setting.type, OnCollectorTypeChanged);
                
                _onChangeCallback = onRemoveItemCallback;
                
            }

            private void OnExtractorItemAdd(IEnumerable<int> obj)
            {
                foreach (var index in obj)
                {
                    setting.extractorSettings[index] = new AssetBundleCollectorRule.ExtractorSetting();
                }
                OnChange();
            }

            private void OnExtractorItemRemove(IEnumerable<int> obj)
            {
                OnChange();
            }
            
            private VisualElement OnExtractorItemMake()
            {
                var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(string.Format(AssetUXMLFolder,"CollectorExtractorSetting.uxml"));
                return asset.Instantiate();
            }

            private void OnExtractorItemBind(VisualElement visualElement, int index)
            {
                var control = new ExtractorItemControl();
                control.Bind(visualElement, _extractorTypeNames, setting.extractorSettings[index]);
                visualElement.userData = control;

            }
            private void OnExtractorItemUnbind(VisualElement visualElement, int i)
            {
                if (visualElement.userData is ExtractorItemControl control)
                {
                    control.Unbind();
                }
                visualElement.userData = null;
            }
            
            
            private void OnExtractorEnableChange(ChangeEvent<bool> evt)
            {
                _extractorGroup.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                
                if (setting.enableExtractor == evt.newValue)
                {
                    return;
                }
                
                setting.enableExtractor = evt.newValue;
                OnChange();
            }

            private void OnChange()
            {
                _onChangeCallback?.Invoke();
            }

            private void InitDropDown(ref DropdownField field, VisualElement element, string qKey, List<string> choices, string value, EventCallback<ChangeEvent<string>> onValueChange)
            {
                field = element.Q<DropdownField>(qKey);
                var index = choices.IndexOf(value);
                if (index < 0)
                {
                    index = 0;
                }

                field.choices = choices;
                field.index = index;
                field.RegisterValueChangedCallback(onValueChange);
            }



            private void OnAssetTagsChange(ChangeEvent<string> evt)
            {
                setting.assetTags = evt.newValue;
                OnChange();
            }
            
            private void OnPackRegularChange(ChangeEvent<string> evt)
            {
                setting.packRegular = evt.newValue;
                OnChange();
            }
            
            private void OnCollectRegularChange(ChangeEvent<string> evt)
            {
                setting.collectRegular = evt.newValue;
                OnChange();
            }

            private void OnCollectorRegularChange(ChangeEvent<string> evt)
            {
                setting.collectorRegular = evt.newValue;
                OnChange();
            }

            private void OnFilterRuleChanged(ChangeEvent<string> evt)
            {
                setting.filterRuleName = evt.newValue;
                OnChange();
            }
            
            private void OnAddreeRuleChanged(ChangeEvent<string> evt)
            {
                setting.addressRuleName = evt.newValue;
                OnChange();
            }
            
            private void OnAddreeRegularChanged(ChangeEvent<string> evt)
            {
                setting.addressRegular = evt.newValue;
                OnChange();
            }
            
            private void OnCollectorTypeChanged(ChangeEvent<string> evt)
            {
                setting.type = evt.newValue;
                OnChange();
            }

            private void OnPackRuleChanged(ChangeEvent<string> evt)
            {
                setting.packRuleName = evt.newValue;
                OnChange();
            }

            private void OnCollectTypeChanged(ChangeEvent<string> evt)
            {
                var names = GetCollectTypeNames();
                if (names.Contains(evt.newValue))
                {
                    setting.collectType = _collectTypeMap[evt.newValue];
                }
            }


            class ExtractorItemControl
            {
                private AssetBundleCollectorRule.ExtractorSetting _setting;
                private DropdownField _extractorType;
                private TextField _extractorPath;
                private TextField _assetNameTemplate;
                private TextField _filePathPickRule;
                
                public void Bind(VisualElement element, List<string> choices, AssetBundleCollectorRule.ExtractorSetting setting)
                {
                    _setting = setting;
                    
                    _extractorType = element.Q<DropdownField>("extractorType");
                    _extractorType.choices = choices;
                    _extractorType.RegisterValueChangedCallback(OnTypeChanged);
                    var selectedIndex = choices.IndexOf(setting.extractorName);
                    _extractorType.index = selectedIndex;
                    _extractorType.SetValueWithoutNotify(selectedIndex >=0 ? setting.extractorName:"Undefined");
                    
                    _extractorPath = element.Q<TextField>("extractorPath");
                    _extractorPath.RegisterValueChangedCallback(OnPathChanged);
                    _extractorPath.SetValueWithoutNotify(setting.extractorFolderPath);
                    
                    _assetNameTemplate = element.Q<TextField>("assetNameTemplate");
                    _assetNameTemplate.RegisterValueChangedCallback(OnAssetNameTemplateChanged);
                    _assetNameTemplate.SetValueWithoutNotify(setting.assetNameTemplate);
                    
                    _filePathPickRule = element.Q<TextField>("filePathPickRule");
                    _filePathPickRule.RegisterValueChangedCallback(OnFilePathPickRuleChanged);
                    _filePathPickRule.SetValueWithoutNotify(setting.filePathPickRule);
                    
                }

                public void Unbind()
                {
                    _extractorType.UnregisterValueChangedCallback(OnTypeChanged);
                    _extractorPath.UnregisterValueChangedCallback(OnPathChanged);
                    _assetNameTemplate.UnregisterValueChangedCallback(OnAssetNameTemplateChanged);
                    _filePathPickRule.UnregisterValueChangedCallback(OnFilePathPickRuleChanged);
                    
                    _extractorType = null;
                    _extractorPath = null;
                    _setting = null;
                }
                
                private void OnFilePathPickRuleChanged(ChangeEvent<string> evt)
                {
                    _setting.filePathPickRule = evt.newValue;
                }

                private void OnAssetNameTemplateChanged(ChangeEvent<string> evt)
                {
                    _setting.assetNameTemplate = evt.newValue;
                }

                private void OnTypeChanged(ChangeEvent<string> evt)
                {
                    _setting.extractorName = evt.newValue;
                }

                private void OnPathChanged(ChangeEvent<string> evt)
                {
                    _setting.extractorFolderPath = evt.newValue;
                }

            }
        }
    }
}