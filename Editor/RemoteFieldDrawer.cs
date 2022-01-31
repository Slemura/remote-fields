using System;
using System.Reflection;
using com.rpdev.remote_fields;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace modules.remote_fields.Editor {
    
    [CustomPropertyDrawer(typeof(BoolRemoteField), true)]
    public class BoolRemoteFieldDrawer : RemoteFieldDrawer {}

    [CustomPropertyDrawer(typeof(StringRemoteField), true)]
    public class StringRemoteFieldDrawer : RemoteFieldDrawer {}

    [CustomPropertyDrawer(typeof(LongRemoteField), true)]
    public class LongRemoteFieldDrawer : RemoteFieldDrawer {}
    
    [CustomPropertyDrawer(typeof(FloatRemoteField), true)]
    public class FloatRemoteFieldDrawer : RemoteFieldDrawer {}

    [CustomPropertyDrawer(typeof(IntRemoteField), true)]
    public class IntRemoteFieldDrawer : RemoteFieldDrawer {}

    [CustomPropertyDrawer(typeof(JsonRemoteField), true)]
    public class JsonRemoteFieldDrawer : RemoteFieldDrawer {
        protected override string GetRemoteFieldType() {
            return $"[json]";
        }
    }

    public abstract class RemoteFieldDrawer : PropertyDrawer {
        
        private float _additional_height;

        private SerializedProperty _default_value;
        private SerializedProperty _key;
        private SerializedProperty _diff;
        private SerializedProperty _remote_value;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            float total_height = EditorGUI.GetPropertyHeight (property, label, true) + EditorGUIUtility.standardVerticalSpacing + _additional_height;
            return total_height;
        }

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            
            EditorGUI.BeginProperty(position, label, property);
            
            _key           ??= property.FindPropertyRelative("key");
            _default_value ??= property.FindPropertyRelative("_default_value");
            _diff          ??= property.FindPropertyRelative("_diff");
            _remote_value  ??=   property.FindPropertyRelative("_remote_value");
            
            Rect black_bg_rect      = new Rect(position.x, position.y, position.width, position.height - 3);
            Rect gray_bg_rect       = new Rect(position.x + 1, position.y + 1, position.width - 2, position.height - 5);
            Rect key_rect           = new Rect(position.x + 8, position.y, position.width, 30);
            Rect default_value_rect = new Rect(position.x + 8, key_rect.y + EditorGUI.GetPropertyHeight(_key) + 10, position.width - 30, EditorGUI.GetPropertyHeight(_default_value));
            
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.DrawRect(black_bg_rect, Color.black);
            EditorGUI.DrawRect(gray_bg_rect, new Color(0.2509f, 0.2509f, 0.2509f, 1));
            
            Color gui_color = GUI.color;
            GUI.color = _diff.boolValue ? Color.yellow : Color.green;
            EditorGUI.LabelField(key_rect, $"{_key.stringValue} {GetRemoteFieldType()}", EditorStyles.boldLabel);
            GUI.color = gui_color;
            
            EditorGUI.PropertyField(default_value_rect, _default_value);

            _additional_height =  key_rect.height + default_value_rect.height - 10;

            if (_diff.boolValue) {
                Rect       remote_value_rect = new Rect(position.x + 105, default_value_rect.y + EditorGUI.GetPropertyHeight(_default_value) + 5, position.width - 130, EditorGUI.GetPropertyHeight(_default_value));
                Rect       remote_label_rect = new Rect(position.x + 8, default_value_rect.y + EditorGUI.GetPropertyHeight(_default_value) + 5, position.width - 30, EditorGUI.GetPropertyHeight(_default_value));

                Type class_type = _remote_value.GetType();
                FieldInfo field      = class_type.GetField(_remote_value.propertyPath);
                
                gui_color = GUI.color;
                GUI.color = Color.yellow;
                EditorGUI.LabelField(remote_label_rect, $"Remote value = {field?.GetValue(_remote_value)}", EditorStyles.boldLabel);
                GUI.color = gui_color;
                
                GUIContent content           = new GUIContent {
                    text = ""
                };
                
                //EditorGUI.PropertyField(remote_value_rect, _remote_value, content);
                _additional_height = key_rect.height + default_value_rect.height + remote_value_rect.height - 5;
            }
            
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        protected virtual string GetRemoteFieldType() {
            return $"[{_default_value?.propertyType.HumanName().ToLower()}]";
        }
    }
}
