

using System.Reflection;
using Unity.Collections;

namespace com.rpdev.remote_fields.Runtime {
    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Firebase.RemoteConfig;
    using Newtonsoft.Json;
    using UnityEngine;
    
    [Serializable]
    public abstract class RemoteField<TValue> {
    
        [SerializeField]
        protected string key;
    
        [SerializeField]
        private TValue _default_value;

    #if UNITY_EDITOR
        [SerializeField]
        [ReadOnly]
        private TValue _remote_value;
        [SerializeField]
        private bool   _diff;
    #endif
    
        protected RemoteField (string key) {
            this.key = key;
            _remote_value = _default_value;
        }

        public TValue Value {
            get {
                FirebaseRemoteConfig remote_config = FirebaseRemoteConfig.DefaultInstance;
                
                Log($"key {key} remote config contains key {remote_config.Keys.Contains(key)}");
                
                if (remote_config.Keys.Contains(key))
                    return GetRemoteValue(remote_config);

                Debug.LogWarning($"Firebase Remote Config: Key \"{key}\" not found.");

                return _default_value;
            }
        }

    #if UNITY_EDITOR
        public void FetchInfo() {
            _remote_value = Value;
            _diff         = !EqualityComparer<TValue>.Default.Equals(_remote_value, _default_value);
        }

        public void MergeRemoteToDefault() {
            _default_value = _remote_value;
            _diff          = false;
        }
    #endif

        protected abstract TValue GetRemoteValue (FirebaseRemoteConfig remote_config);

        private void Log(string log) {
            Debug.Log($"[Remote field] : {log}");
        }
    }


    [Serializable]
    public class LongRemoteField : RemoteField <long> {
        public LongRemoteField (string key) : base(key) {}

        protected override long GetRemoteValue (FirebaseRemoteConfig remote_config) {
            return remote_config.GetValue(key).LongValue;
        }
    }

    [Serializable]
    public class IntRemoteField : RemoteField <int> {
        public IntRemoteField (string key) : base(key) {}

        protected override int GetRemoteValue (FirebaseRemoteConfig remote_config) {
            return  (int)remote_config.GetValue(key).LongValue;
        }
    }

    [Serializable]
    public class FloatRemoteField : RemoteField<float> {
    
        public FloatRemoteField (string key) : base(key) {}

        protected override float GetRemoteValue (FirebaseRemoteConfig remote_config) {
            return (float)remote_config.GetValue(key).LongValue;
        }
    }

    [Serializable]
    public class BoolRemoteField : RemoteField <bool> {
        public BoolRemoteField (string key) : base(key) {}

        protected override bool GetRemoteValue (FirebaseRemoteConfig remote_config) {
            return remote_config.GetValue(key).BooleanValue;
        }
    }

    [Serializable]
    public class StringRemoteField : RemoteField <string> {
        public StringRemoteField (string key) : base(key) {}

        protected override string GetRemoteValue (FirebaseRemoteConfig remote_config) {
            return remote_config.GetValue(key).StringValue;
        }
    }

    [Serializable]
    public class JsonRemoteField : RemoteField <string> {
    
        public JsonRemoteField (string key) : base(key) {}

        protected T GetParsedValue<T>() {
            FirebaseRemoteConfig remote_config = FirebaseRemoteConfig.DefaultInstance;

            if (remote_config.Keys.Contains(key))
                return default;

            try {
                T parsed_data = JsonConvert.DeserializeObject<T>(GetRemoteValue(remote_config));
                return parsed_data;
                
            } catch (Exception e) {
                Debug.LogError($"Can't parse json in {nameof(T)} data {GetRemoteValue(remote_config)}");
                return default;
            }
        }
        
        protected override string GetRemoteValue (FirebaseRemoteConfig remote_config) {
            return remote_config.GetValue(key).StringValue;
        }
    }

    
}
