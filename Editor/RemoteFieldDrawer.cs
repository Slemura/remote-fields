using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.rpdev.remote_fields.Runtime;
using UnityEditor;
using UnityEngine;

namespace com.rpdev.remote_fields.Editor {

    [CustomPropertyDrawer(typeof(BoolRemoteField), true)]
    public class BoolRemoteFieldDrawer : RemoteFieldDrawer {
        protected override string GetRemoteValue() {
            return remote_value.boolValue.ToString();
        }
    }

    [CustomPropertyDrawer(typeof(StringRemoteField), true)]
    public class StringRemoteFieldDrawer : RemoteFieldDrawer {
        protected override string GetRemoteValue() {
            return remote_value.stringValue;
        }
    }

    [CustomPropertyDrawer(typeof(LongRemoteField), true)]
    public class LongRemoteFieldDrawer : RemoteFieldDrawer {
        protected override string GetRemoteValue() {
            return remote_value.longValue.ToString();
        }
    }
    
    [CustomPropertyDrawer(typeof(FloatRemoteField), true)]
    public class FloatRemoteFieldDrawer : RemoteFieldDrawer {
        protected override string GetRemoteValue() {
            return remote_value.floatValue.ToString();
        }
    }

    [CustomPropertyDrawer(typeof(IntRemoteField), true)]
    public class IntRemoteFieldDrawer : RemoteFieldDrawer {
        protected override string GetRemoteValue() {
            return remote_value.intValue.ToString();
        }
    }

    [CustomPropertyDrawer(typeof(JsonRemoteField), true)]
    public class JsonRemoteFieldDrawer : RemoteFieldDrawer {
        protected override string GetRemoteValue() {
            return remote_value.stringValue;
        }

        protected override string GetRemoteFieldType() {
            return $"[json]";
        }
    }

    public abstract class RemoteFieldDrawer : PropertyDrawer {
        
        private float _additional_height;

        private SerializedProperty _default_value;
        private SerializedProperty _key;
        private SerializedProperty _diff;
        
        protected SerializedProperty remote_value;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            float total_height = EditorGUI.GetPropertyHeight (property, label, true) + EditorGUIUtility.standardVerticalSpacing + _additional_height;
            return total_height;
        }

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            
            EditorGUI.BeginProperty(position, label, property);
            
            _key           ??= property.FindPropertyRelative("key");
            _default_value ??= property.FindPropertyRelative("_default_value");
            _diff          ??= property.FindPropertyRelative("_diff");
            remote_value   ??= property.FindPropertyRelative("_remote_value");

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            DrawBackground(position);
            
            Rect key_rect = DrawFieldLabel(position);
            Rect default_value_rect = DrawDefaultValue(new Rect(position.x, key_rect.position.y, position.width, position.height), property);
            
            _additional_height =  key_rect.height + default_value_rect.height - 10;

            if (_diff.boolValue) {
                Rect remote_value_rect = DrawRemoteValue(default_value_rect, property);
                _additional_height = key_rect.height + default_value_rect.height + remote_value_rect.height - 5;
            }
            
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        protected virtual void DrawBackground(Rect position) {
            Rect black_bg_rect = new Rect(position.x, position.y, position.width, position.height - 3);
            Rect gray_bg_rect  = new Rect(position.x + 1, position.y + 1, position.width - 2, position.height - 5);
            
            EditorGUI.DrawRect(black_bg_rect, Color.black);
            EditorGUI.DrawRect(gray_bg_rect, new Color(0.2509f, 0.2509f, 0.2509f, 1));
        }
        
        protected virtual Rect DrawFieldLabel(Rect position) {
            Rect  key_rect  = new Rect(position.x + 8, position.y, position.width, 30);
            Color gui_color = GUI.color;
            GUI.color = _diff.boolValue ? Color.yellow : Color.green;
            EditorGUI.LabelField(key_rect, $"{_key.stringValue} {GetRemoteFieldType()} {(_diff.boolValue ? "Not Equal" : "")}", EditorStyles.boldLabel);
            GUI.color = gui_color;
            return key_rect;
        }

        protected virtual Rect DrawDefaultValue(Rect position, SerializedProperty property) {
            Rect default_value_rect = new Rect(position.x + 8, position.y + EditorGUI.GetPropertyHeight(_key) + 10, position.width - 30, EditorGUI.GetPropertyHeight(_default_value));
            EditorGUI.PropertyField(default_value_rect, _default_value);
            return default_value_rect;
        }

        protected virtual Rect DrawRemoteValue(Rect position, SerializedProperty property) {
            int update_button_width = 140;
            Rect remote_value_rect = new Rect(position.x + position.width - update_button_width,
                                              position.y + EditorGUI.GetPropertyHeight(_default_value) + 3,
                                                update_button_width, EditorGUI.GetPropertyHeight(_default_value));

            Rect remote_label_rect = new Rect(position.x,
                                              position.y + EditorGUI.GetPropertyHeight(_default_value) + 5,
                                              position.width         - update_button_width - 5, 
                                              EditorGUI.GetPropertyHeight(_default_value));

            Color gui_color = GUI.color;
            GUI.color = Color.yellow;
            EditorGUI.LabelField(remote_label_rect, $"Remote value = {GetRemoteValue()}", EditorStyles.boldLabel);
            GUI.color = gui_color;
            
            bool pressed = EditorGUI.LinkButton(remote_value_rect, "^ Update default value ^");
            
            if (pressed) {
                MergeRemoteToDefault(property);
            }
            
            return remote_label_rect;
        }

        protected abstract string GetRemoteValue();

        protected virtual string GetRemoteFieldType() {
            return $"[{_default_value.propertyType.ToString().ToLower()}]";
        }

    #region Reflection utility
        private void MergeRemoteToDefault(SerializedProperty property) {
            object     parent = GetParent(property);
            object     value  = GetValue(parent, property.name);
            MethodInfo method = value.GetType().GetMethod("MergeRemoteToDefault");
            method?.Invoke(value, Array.Empty<object>());
        }
        
        private object GetParent(SerializedProperty prop) {
            string   path     = prop.propertyPath.Replace(".Array.data[", "[");
            object   obj      = prop.serializedObject.targetObject;
            string[] elements = path.Split('.');
            
            foreach(string element in elements.Take(elements.Length -1)) {
                if(element.Contains("[")) {
                    
                    string element_name = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                    int    index        = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal)).Replace("[","").Replace("]",""));
                    obj = GetValue(obj, element_name, index);
                } else {
                    obj = GetValue(obj, element);
                }
            }
            return obj;
        }

        private object GetValue(object source, string name) {
            if(source == null) return null;
            Type type = source.GetType();
            FieldInfo f    = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if(f == null) {
                PropertyInfo p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                return p == null ? null : p.GetValue(source, null);
            }
            return f.GetValue(source);
        }

        private object GetValue(object source, string name, int index) {
            if (GetValue(source, name) is IEnumerable enumerable) {
                IEnumerator enm = enumerable.GetEnumerator();
                while(index-- >= 0)
                    enm.MoveNext();
                return enm.Current;
            }

            return null;
        }
    #endregion
    }
}
