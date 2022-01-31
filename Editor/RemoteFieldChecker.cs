using System;
using System.Linq;
using System.Reflection;
using com.rpdev.remote_fields.Runtime;

namespace com.rpdev.remote_fields.Editor {
    public static class RemoteFieldChecker {
        
        public static void CheckRemoteFieldsInType(Type type, object instance) {
            object[] remote_fields = type.GetFields()
                                         .Where(field => field.FieldType == typeof(RemoteField<>))
                                         .Select(field => field.GetValue(instance))
                                         .ToArray();

            foreach (object remote_field in remote_fields) {
                MethodInfo method = remote_field.GetType().GetMethod("FetchInfo");
                method?.Invoke(remote_field, Array.Empty<object>());
            }
        }
    }
}