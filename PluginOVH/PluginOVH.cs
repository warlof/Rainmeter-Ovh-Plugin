// Uncomment these only if you want to export GetString() or ExecuteBang().
//#define DLLEXPORT_GETSTRING
//#define DLLEXPORT_EXECUTEBANG

using System;
using System.IO;
using System.Runtime.InteropServices;
using Rainmeter;

using Ovh.RestLib;
using RestSharp;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using PluginOVH.Models;
using PluginOVH.Models.Dedicated;
using PluginOVH.Models.Vps;
using System.Diagnostics;
using PluginOVH.Measures;

// Overview: This is a blank canvas on which to build your plugin.

// Note: Measure.GetString, Plugin.GetString, Measure.ExecuteBang, and
// Plugin.ExecuteBang have been commented out. If you need GetString
// and/or ExecuteBang and you have read what they are used for from the
// SDK docs, uncomment the function(s). Otherwise leave them commented out
// (or get rid of them)!

namespace PluginOVH
{
    
    public static class Plugin
    {
        static IntPtr StringBuffer = IntPtr.Zero;
        static Service service;

        private enum Service
        {
            Dedicated,
            VPS
        }

        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            API api = new API(rm);
            service = Service.Dedicated;

            #region Service check
            string parameter = api.ReadString("Service", "");

            if (Enum.IsDefined(typeof(Service), parameter))
                service = (Service)Enum.Parse(typeof(Service), parameter, true);
            else
                API.Log(API.LogType.Warning, String.Format("'{0}' is mandatory and no value has been set or the value is not valid. The default value '{1}' will be use for the measure '{2}'.",
                    "Service", Service.Dedicated, api.GetMeasureName()));
            #endregion

            switch (service)
            {
                case Service.Dedicated:
                    data = GCHandle.ToIntPtr(GCHandle.Alloc(new DedicatedMeasure()));
                    break;
                default:
                    data = GCHandle.ToIntPtr(GCHandle.Alloc(new VpsMeasure()));
                    break;
            }
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            GCHandle.FromIntPtr(data).Free();

            if (StringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            switch (service)
            {
                case Service.Dedicated:
                    DedicatedMeasure dedicatedMeasure = (DedicatedMeasure)GCHandle.FromIntPtr(data).Target;
                    dedicatedMeasure.Reload(new API(rm), ref maxValue);
                    break;
                default:
                    VpsMeasure vpsMeasure = (VpsMeasure)GCHandle.FromIntPtr(data).Target;
                    vpsMeasure.Reload(new API(rm), ref maxValue);
                    break;
            }
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            switch (service)
            {
                case Service.Dedicated:
                    DedicatedMeasure dedicatedMeasure = (DedicatedMeasure)GCHandle.FromIntPtr(data).Target;
                    return dedicatedMeasure.Update();
                default:
                    VpsMeasure vpsMeasure = (VpsMeasure)GCHandle.FromIntPtr(data).Target;
                    return vpsMeasure.Update();
            }
        }
        
        [DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            string stringValue;

            switch (service)
            {
                case Service.Dedicated:
                    DedicatedMeasure dedicatedMeasure = (DedicatedMeasure)GCHandle.FromIntPtr(data).Target;
                    if (StringBuffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(StringBuffer);
                        StringBuffer = IntPtr.Zero;
                    }

                    stringValue = dedicatedMeasure.GetString();
                    if (stringValue != null)
                    {
                        StringBuffer = Marshal.StringToHGlobalUni(stringValue);
                    }

                    return StringBuffer;
                default:
                    VpsMeasure vpsMeasure = (VpsMeasure)GCHandle.FromIntPtr(data).Target;
                    if (StringBuffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(StringBuffer);
                        StringBuffer = IntPtr.Zero;
                    }

                    stringValue = vpsMeasure.GetString();
                    if (stringValue != null)
                    {
                        StringBuffer = Marshal.StringToHGlobalUni(stringValue);
                    }

                    return StringBuffer;
            }
        }

#if DLLEXPORT_EXECUTEBANG
        [DllExport]
        public static void ExecuteBang(IntPtr data, IntPtr args)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.ExecuteBang(Marshal.PtrToStringUni(args));
        }
#endif
    }
}
