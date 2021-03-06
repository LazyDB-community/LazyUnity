using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using WebSocketSharp;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Threading;

public class LazyCsharp : MonoBehaviour
{
    public class Callback
    {
        public Action<Newtonsoft.Json.Linq.JToken> success;
        public Action<Newtonsoft.Json.Linq.JToken> fail;
    }

    public class Database
    {
        public string addr;
        public int port;
        public int id = 0;
        public Dictionary<int, Callback> callbacks = new Dictionary<int, Callback> { };
        public WebSocket ws;
        public List<string> messageQueue = new List<string> { };

        public class ReceiveObject
        {
            public bool s { get; set; }
            public int id { get; set; }
            public object r { get; set; }
        }

        public static class Interval
        {
            public static System.Timers.Timer Set(System.Action action, int interval)
            {
                var timer = new System.Timers.Timer(interval);
                timer.Elapsed += (s, e) => {
                    timer.Enabled = false;
                    action();
                    timer.Enabled = true;
                };
                timer.Enabled = true;
                return timer;
            }

            public static void Stop(System.Timers.Timer timer)
            {
                timer.Stop();
                timer.Dispose();
            }
        }

        public Database(string addr, int port, [Optional] Action<object> onconnect)
        {
            this.addr = addr;
            this.port = port;
            this.ws = new WebSocket($"ws://{addr}:{port}");
            this.ws.OnOpen += (sender, e) =>
            {
                onconnect(e);
            };

            this.ws.OnMessage += (sender, e) =>
            {
                var messages = e.Data.Split("|");
                for (int i = 0; i < messages.Length; i++)
                {
                    var msg = messages[i];
                    Newtonsoft.Json.Linq.JObject receivedMessage = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(msg);
                    if (receivedMessage.Value<bool>("s"))
                    {
                        this.callbacks[receivedMessage.Value<int>("id")].success.Invoke(receivedMessage.GetValue("r"));
                    }
                    else
                    {
                        this.callbacks[receivedMessage.Value<int>("id")].fail.Invoke(receivedMessage.GetValue("r"));
                    }
                }
            };

            Interval.Set(sendQueue, 100);

            this.ws.Connect();
        }

        public void sendQueue()
        {
            if (this.messageQueue.Count > 0 && this.ws.ReadyState.ToString() == "Open")
            {
                this.ws.Send(string.Join("|", this.messageQueue));
            }

            this.messageQueue = new List<string> { };
        }

        [DataContract]
        public class Send
        {
            [DataMember(Name = "c")]
            public string c { get; set; }

            [DataMember(Name = "id")]
            public int id { get; set; }

            [DataMember(Name = "a")]
            public object a { get; set; }
        }

        public void send(string name, object args, Callback callback)
        {
            int id = ++this.id;
            Send send = new Send
            {
                c = name,
                id = id,
                a = args
            };

            Debug.Log(send);
            string message = JsonConvert.SerializeObject(send);
            Debug.Log(message);
            Debug.Log(callback.success);
            if (!message.Contains("|"))
            {
                this.messageQueue.Add(message);
                this.callbacks.Add(id, callback);
            }
        }

        public void connect(string email, string password, Callback callback)
        {
            Dictionary<string, string> args = new Dictionary<string, string> { };
            args.Add("email", email);
            args.Add("password", password);

            this.send("connect", args, callback);
        }

        public void register(string email, string password, string username, string full_name, Callback callback)
        {
            Dictionary<string, string> args = new Dictionary<string, string> { };
            args.Add("email", email);
            args.Add("password", password);
            args.Add("username", username);
            args.Add("full_name", full_name);

            this.send("register", args, callback);
        }

        public void create(string keyPath, Newtonsoft.Json.Linq.JObject value, Callback callback)
        {
            var key = keyPath.Split("/");

            Newtonsoft.Json.Linq.JObject args = new Newtonsoft.Json.Linq.JObject { };
            args.Add("keyPath", Newtonsoft.Json.Linq.JToken.FromObject(key));
            args.Add("value", value);
            args.Add("w", true);

            this.send("create", args, callback);
        }

        public void get(string keyPath, Callback callback)
        {
            var key = keyPath.Split("/");

            Newtonsoft.Json.Linq.JObject args = new Newtonsoft.Json.Linq.JObject { };
            args.Add("keyPath", Newtonsoft.Json.Linq.JToken.FromObject(key));

            this.send("get", args, callback);
        }

        public void delete(string keyPath, Callback callback)
        {
            var key = keyPath.Split("/");

            Newtonsoft.Json.Linq.JObject args = new Newtonsoft.Json.Linq.JObject { };
            args.Add("keyPath", Newtonsoft.Json.Linq.JToken.FromObject(key));

            this.send("delete", args, callback);
        }

        public void keys(string keyPath, string filter, Callback callback)
        {
            var key = keyPath.Split("/");

            Newtonsoft.Json.Linq.JObject args = new Newtonsoft.Json.Linq.JObject { };
            args.Add("keyPath", Newtonsoft.Json.Linq.JToken.FromObject(key));
            args.Add("filter", filter);

            this.send("keys", args, callback);
        }

        public void update(string keyPath, string value, Callback callback)
        {
            var key = keyPath.Split("/");

            Newtonsoft.Json.Linq.JObject args = new Newtonsoft.Json.Linq.JObject { };
            args.Add("keyPath", Newtonsoft.Json.Linq.JToken.FromObject(key));
            args.Add("value", value);
            args.Add("w", true);

            this.send("update", args, callback);
        }

        public void exists(string keyPath, Callback callback)
        {
            var key = keyPath.Split("/");

            Newtonsoft.Json.Linq.JObject args = new Newtonsoft.Json.Linq.JObject { };
            args.Add("keyPath", Newtonsoft.Json.Linq.JToken.FromObject(key));

            this.send("exist", args, callback);
        }

        public void size(string keyPath, Callback callback)
        {
            var key = keyPath.Split("/");

            Newtonsoft.Json.Linq.JObject args = new Newtonsoft.Json.Linq.JObject { };
            args.Add("keyPath", Newtonsoft.Json.Linq.JToken.FromObject(key));

            this.send("size", args, callback);
        }
    }
}