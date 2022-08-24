using Nancy.Json;
using Rainmeter;
using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
        static public implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }
        //public HttpClient client;
        public IntPtr buffer = IntPtr.Zero;

        public string tracker;
    }

    public class Plugin
    {
        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
            Rainmeter.API api = (Rainmeter.API)rm;
            Measure measure = (Measure)data;
            //measure.client = new HttpClient();
            measure.api = api;
            measure.tracker = api.ReadString("tracker", "");
            measure.server = api.ReadString("server", "homeassitant.local");
            measure.auth = api.ReadString("authKey", "");
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
            //measure.client = new HttpClient();
            measure.api = api;
            measure.server = api.ReadString("server", "homeassitant.local");
            measure.auth = api.ReadString("auth", "");
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            return -1.12;
        }

        //[DllExport]
        //public static IntPtr GetString(IntPtr data)
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
        /*[DllExport]
        public static IntPtr getValue(IntPtr data, int argc,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
        {
        }*/

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
                Task<HttpResponseMessage> response = client.PostAsync($"http://{measure.server}:8123/api/services/{device}/{service}", content);
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

