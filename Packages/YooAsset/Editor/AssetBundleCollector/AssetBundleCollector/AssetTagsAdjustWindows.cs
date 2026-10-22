using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    public class AssetTagsAdjustWindows : EditorWindow
    {
        [Header("UI设置")] [SerializeField] private UIDocument uiDocument;
        [SerializeField] private List<string> tags = _package.Tags;

        [Header("样式设置")] [SerializeField] private Color tagBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color deleteButtonColor = new Color(0.8f, 0.3f, 0.3f, 1f);

        private static AssetBundleCollectorPackage _package;
        private const string AssetUXMLFolder = PathDefine.YOOASSET_PATH + "/Editor/AssetBundleCollector/AssetBundleCollector/uxml/AssetTagSetting.uxml";
                
        private VisualTreeAsset _visualElement;
        private TextField _newTagField;
        private Button _addTagButton;
        private Button _clearAllButton;
        private VisualElement _tagsContent;
        private Label _tagCountLabel;

        public System.Action<List<string>> OnTagsChanged;
        
        public static void Open(AssetBundleCollectorPackage package)
        {
            _package = package;
            
            var window = GetWindow<AssetTagsAdjustWindows>();
            window.minSize = new Vector2(600, 400);
        }

        private void CreateGUI()
        {
            VisualElement root = this.rootVisualElement;
            
            _visualElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(string.Format(AssetUXMLFolder,"AssetBundleCollectorSetting.uxml"));
            _visualElement.CloneTree(root);
            
            _newTagField = root.Q<TextField>("TagField");
            _addTagButton = root.Q<Button>("AddButton");
            _clearAllButton = root.Q<Button>("ClearButton");
            _tagsContent = root.Q<VisualElement>("TagsContent");
            _tagCountLabel = root.Q<Label>("CountLable");

            SetupEvents();
            RefreshTagsList();
        }

        void SetupEvents()
        {
            _addTagButton.clicked += AddTag;

            _clearAllButton.clicked += ClearAllTags;

            _newTagField.RegisterCallback<KeyDownEvent>(OnInputFieldKeyDown);

            _newTagField.RegisterValueChangedCallback(OnInputFieldChanged);
        }

        void OnInputFieldKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                AddTag();
            }
        }

        void OnInputFieldChanged(ChangeEvent<string> evt)
        {
            _addTagButton.SetEnabled(!string.IsNullOrEmpty(evt.newValue.Trim()));
        }

        void AddTag()
        {
            string newTag = _newTagField.value?.Trim();

            if (string.IsNullOrEmpty(newTag))
            {
                Debug.LogWarning("标签不能为空");
                return;
            }

            if (tags.Contains(newTag))
            {
                Debug.LogWarning($"标签 '{newTag}' 已存在");
                _newTagField.value = "";
                return;
            }

            tags.Add(newTag);
            _newTagField.value = "";

            RefreshTagsList();
            OnTagsChanged?.Invoke(tags);

            Debug.Log($"添加标签: {newTag}");
        }

        void RemoveTag(string tag)
        {
            if (tags.Remove(tag))
            {
                RefreshTagsList();
                OnTagsChanged?.Invoke(tags);
                Debug.Log($"删除标签: {tag}");
            }
        }

        void ClearAllTags()
        {
            tags.Clear();
            RefreshTagsList();
            OnTagsChanged?.Invoke(tags);
            Debug.Log("清除所有标签");
        }

        void RefreshTagsList()
        {
            _tagsContent.Clear();

            foreach (string tag in tags)
            {
                CreateTagElement(tag);
            }

            _tagCountLabel.text = $"Tags: {tags.Count}";
            _addTagButton.SetEnabled(!string.IsNullOrEmpty(_newTagField.value?.Trim()));
        }

        VisualElement CreateTagElement(string tag)
        {
            var tagElement = new VisualElement();
            tagElement.AddToClassList("tag-item");
            tagElement.style.flexDirection = FlexDirection.Row;
            tagElement.style.alignItems = Align.Center;
            tagElement.style.backgroundColor = tagBackgroundColor;
            tagElement.style.marginBottom = 5;
            tagElement.style.paddingLeft = 10;
            tagElement.style.paddingRight = 10;
            tagElement.style.paddingTop = 5;
            tagElement.style.paddingBottom = 5;
            tagElement.style.borderTopLeftRadius = 15;
            tagElement.style.borderBottomLeftRadius = 15;
            tagElement.style.borderTopRightRadius = 15;
            tagElement.style.borderBottomRightRadius = 15;

            var tagLabel = new Label(tag);
            tagLabel.style.flexGrow = 1;
            tagLabel.style.color = Color.white;
            tagElement.Add(tagLabel);

            var deleteButton = new Button(() => RemoveTag(tag));
            deleteButton.text = "✕";
            deleteButton.style.backgroundColor = deleteButtonColor;
            deleteButton.style.color = Color.white;
            deleteButton.style.borderLeftWidth = 0;
            deleteButton.style.borderRightWidth = 0;
            deleteButton.style.borderTopWidth = 0;
            deleteButton.style.borderBottomWidth = 0;
            deleteButton.style.borderTopLeftRadius = 3;
            deleteButton.style.borderBottomLeftRadius = 3;
            deleteButton.style.borderTopRightRadius = 3;
            deleteButton.style.borderBottomRightRadius = 3;
            deleteButton.style.marginLeft = 5;
            deleteButton.style.width = 25;
            deleteButton.style.height = 25;

            deleteButton.RegisterCallback<MouseEnterEvent>(evt =>
            {
                deleteButton.style.backgroundColor = new Color(1f, 0.4f, 0.4f, 1f);
            });

            deleteButton.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                deleteButton.style.backgroundColor = deleteButtonColor;
            });

            tagElement.Add(deleteButton);
            _tagsContent.Add(tagElement);

            return tagElement;
        }

        public List<string> GetTags()
        {
            return new List<string>(tags);
        }

        public void SetTags(List<string> newTags)
        {
            tags.Clear();
            if (newTags != null)
            {
                tags.AddRange(newTags);
            }

            RefreshTagsList();
        }

        public void AddTagProgrammatically(string tag)
        {
            if (!string.IsNullOrEmpty(tag) && !tags.Contains(tag))
            {
                tags.Add(tag);
                RefreshTagsList();
                OnTagsChanged?.Invoke(tags);
            }
        }
    }
}
