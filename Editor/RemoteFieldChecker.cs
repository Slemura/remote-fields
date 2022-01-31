using System;
using System.Linq;
using System.Reflection;
using com.rpdev.remote_fields.Runtime;
using UnityEngine;

namespace com.rpdev.remote_fields.Editor {
    public static class RemoteFieldChecker {
        
        public static void CheckRemoteFieldsInType(Type type, object instance) {
            object[] remote_fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                                         .Select(field => field.GetValue(instance))
                                         .ToArray();
            
            Debug.Log($"[Remote field checker] : total remote fields in {instance} {remote_fields.Length}");
            
            foreach (object remote_field in remote_fields) {
                MethodInfo method = remote_field.GetType().GetMethod("FetchInfo");
                method?.Invoke(remote_field, Array.Empty<object>());
            }
        }
    }
}