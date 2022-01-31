using System;
using UnityEngine;

namespace com.rpdev.remote_fields.Runtime {
    
    [CreateAssetMenu(fileName = "RemoteFieldTest", menuName = "Data/RemoteFieldTest")]
    public class RemoteFieldsSample : ScriptableObject {
        
        [SerializeField]
        private Color _color;
/*
        [SerializeField]
        private StringRemoteField _colum_remote = new StringRemoteField("column_remote");

        [SerializeField]
        private StringRemoteField _row_remote = new StringRemoteField("row_remote");
        
        [SerializeField]
        private StringRemoteField _title_remote = new StringRemoteField("title_remote");
        
        [SerializeField]
        private BoolRemoteField _is_tutorial = new BoolRemoteField("is_tutorial_remote");
        
        [SerializeField]
        private FloatRemoteField _inter_timeout = new FloatRemoteField("inter_timeout_remote");
        
        [SerializeField]
        private JsonRemoteField _json_remote = new JsonRemoteField("json_remote");
        
        [SerializeField]
        private IntRemoteField _iteration = new IntRemoteField("iteration");
        */

        [SerializeField]
        private IntRemoteField _iteratiofn = new IntRemoteField("iteration");

        private struct TestStruct {
            public int    index;
            public string name;
        }
    }
}