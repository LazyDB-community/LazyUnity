using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LazyCsharp;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Callback callback = new Callback();
        callback.success = delegate (Newtonsoft.Json.Linq.JToken s) {
            Debug.Log(s);
        };
        callback.fail = delegate (Newtonsoft.Json.Linq.JToken s) {
            Debug.Log(s);
        };

        Database db = new Database("eu.indivis.cloud", 42600, delegate (object s) {
            Debug.Log("Good!");
        });

        db.connect("arthur", "test", callback);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
