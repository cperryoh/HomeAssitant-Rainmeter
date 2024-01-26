using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Rainmeter;
using Newtonsoft.Json;
using System.Dynamic;

// Overview: This is a blank canvas on which to build your plugin.

// Note: GetString, ExecuteBang and an unnamed function for use as a section variable
// have been commented out. If you need GetString, ExecuteBang, and/or section variables 
// and you have read what they are used for from the SDK docs, uncomment the function(s)
// and/or add a function name to use for the section variable function(s). 
// Otherwise leave them commented out (or get rid of them)!

namespace HomeAssitantPlugin
{
    public class Measure
    {
        public string auth;
        public string server;
        public API api;
        public string id;
        public string data="nan";
        static public implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }
        //public HttpClient client;
        public IntPtr buffer = IntPtr.Zero;
        public bool ssl;
        public string isInt;
        public string fullUrl;
        public string path;
    }

    public class Plugin
    {
        class Entity
        {
            public string state { get; set; }
        }
        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
            Rainmeter.API api = (Rainmeter.API)rm;
            Measure measure = (Measure)data;
            init(measure,api);
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            Measure measure = (Measure)data;
            if (measure.buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(measure.buffer);
            }
            GCHandle.FromIntPtr(data).Free();
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)data;
            Rainmeter.API api = (Rainmeter.API)rm;
            init(measure, api);
        }
        public static void init(Measure measure,API api) {
            measure.api = api;
            measure.path = api.ReadString("path", "state");
            measure.id = api.ReadString("entityId", "");
            measure.isInt = api.ReadString("isInt", "false").ToLower();
            measure.server = api.ReadString("server", "homeassitant.local");
            measure.ssl = api.ReadString("ssl", "false").ToLower() == "true";
            measure.auth = api.ReadString("authKey", "");
            int port = api.ReadInt("port", -1);
            string portStr = (port == -1) ? "" : ":" + port;
            if (measure.ssl)
                measure.fullUrl = "https://" + measure.server + portStr;
            else
                measure.fullUrl = "http://" + measure.server + portStr;
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)data;
            try
            {
                if (measure.id.Equals(""))
                    return 0.0;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {measure.auth}");
                Task<HttpResponseMessage> response = client.GetAsync($"{measure.fullUrl}/api/states/{measure.id}");
                response.Wait();
                Task<string> stringTask = response.Result.Content.ReadAsStringAsync();
                stringTask.Wait();
                string json = stringTask.Result;
                measure.api.Log(API.LogType.Debug, json);
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                string jsonOut = getValue(measure.path, values);
                measure.data= jsonOut;
                if (measure.isInt.Equals("true"))
                    return Double.Parse(jsonOut);
                //return Int32.Parse(entity.state);
                return 0.0;
            }
            catch (Exception e)
            {
                measure.api.Log(API.LogType.Error, e.Message);
                return 0.0;
            }
        }
        public static bool isInt(string str)
        {
            try
            {
                int temp = Int32.Parse(str);
                return true;
            }catch(Exception e)
            {
                return false;
            }
        }
        public static string getValue(string path, Dictionary<string, object> json) 
        {
            string[] keys = path.Split('.');
            dynamic jsonObject = new ExpandoObject();
            jsonObject = json[keys[0]];
            JValue jsonValue;
            for(int i = 1; i< keys.Length; i++)
            {
                bool isNum = isInt(keys[i]);
                if (isNum)
                {
                    int index = Int32.Parse(keys[i]);
                    JArray objects =(JArray)jsonObject;
                    jsonObject = objects[index];
                }
                else
                {
                    jsonObject = jsonObject[keys[i]];
                }
            }
            jsonValue = (JValue)jsonObject;
            return jsonValue.Value.ToString();
        }
        [DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = (Measure)data;
            try
            {
                if (measure.buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(measure.buffer);
                    measure.buffer = IntPtr.Zero;
                }

                measure.buffer = Marshal.StringToHGlobalUni(measure.data);
                return measure.buffer;
            }catch(Exception e)
            {
                measure.api.Log(API.LogType.Error, e.Message);
                return Marshal.StringToHGlobalUni("could not get data");
            }
        }

        //[DllExport]
        //public static void ExecuteBang(IntPtr data, [MarshalAs(UnmanagedType.LPWStr)]String args)
        //{
        //    Measure measure = (Measure)data;
        //}

        //[DllExport]
        //public static IntPtr (IntPtr data, int argc,
        //    [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
        //{
        //    Measure measure = (Measure)data;
        //    if (measure.buffer != IntPtr.Zero)
        //    {
        //        Marshal.FreeHGlobal(measure.buffer);
        //        measure.buffer = IntPtr.Zero;
        //    }
        //
        //    measure.buffer = Marshal.StringToHGlobalUni("");
        //
        //    return measure.buffer;
        //}
        [DllExport]
        public static void ExecuteBang(IntPtr data, [MarshalAs(UnmanagedType.LPWStr)] string args)
        {
            Measure measure = (Measure)data;
            try
            {
                measure.api.Log(API.LogType.Debug, $"Args: {args}");
                string[] argValues = args.Split('!');
                measure.api.Log(API.LogType.Debug, argValues.Length.ToString());
                string print = "";
                argValues[2] = argValues[2].Replace("'", "\"");
                foreach (string arg in argValues)
                {
                    print += $" [{arg}] ";
                }
                measure.api.Log(API.LogType.Debug, print);

                string device = argValues[0];
                string service = argValues[1];
                string serviceData = argValues[2];
                HttpClient client = new HttpClient();
                if (measure.auth.Length == 0)
                    return;
                // !CommandMeasure was used on this measure...any arguments will be in args
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {measure.auth}");
                var content = new StringContent(serviceData);
                Task<HttpResponseMessage> response = client.PostAsync($"{measure.fullUrl}/api/services/{device}/{service}", content);
                response.Wait();
                Task<String> getText = response.Result.Content.ReadAsStringAsync();
                getText.Wait();
                string responseText = getText.Result;
                measure.api.Log(API.LogType.Debug, responseText);
            }
            catch (Exception e)
            {
                measure.api.Log(API.LogType.Error, e.Message);
            }
        }
    }
}

