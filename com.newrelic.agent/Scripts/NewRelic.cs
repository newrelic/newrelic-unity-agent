using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NewRelic
{

    public class NativeDictionaryWrapper : System.IDisposable
    {
#if UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern void NR_dictionaryDispose(System.IntPtr dictionary);

        [DllImport("__Internal")]
        private static extern System.IntPtr NR_dictionaryCreate();

        [DllImport("__Internal")]
        private static extern void NR_dictionaryInsertString(System.IntPtr dictionary, String key, String value);

        [DllImport("__Internal")]
        private static extern void NR_dictionaryInsertInt64(System.IntPtr dictionary, String key, Int64 value);

        [DllImport("__Internal")]
        private static extern void NR_dictionaryInsertUInt64(System.IntPtr dictionary, String key, UInt64 value);

        [DllImport("__Internal")]
        private static extern void NR_dictionaryInsertDouble(System.IntPtr dictionary, String key, double value);

        [DllImport("__Internal")]
        private static extern void NR_dictionaryInsertFloat(System.IntPtr dictionary, String key, float value);

#endif

        #region IDisposable implementation


        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
#if UNITY_IPHONE
                NR_dictionaryDispose(handle);

                handle = System.IntPtr.Zero;
#endif
                disposed = true;
            }
        }

        ~NativeDictionaryWrapper()
        {
            Dispose();
        }


        #endregion

#if UNITY_IPHONE
        public System.IntPtr handle { get; private set; }
#endif

        public NativeDictionaryWrapper()
        {
#if UNITY_IPHONE
            handle = NR_dictionaryCreate();
#endif
        }

        public bool insert(string key, object value)
        {
            if (key == null || value == null)
            {
                //todo: error message
                return false;
            }
            if (value is string)
            {
#if UNITY_IPHONE
                NR_dictionaryInsertString(handle, key, (string)value);
#endif
            }
            else if (value is SByte || value is Byte || value is Int16 || value is Int32 || value is Int64 || value is long)
            {
#if UNITY_IPHONE
                Int64 value64 = Convert.ToInt64(value);
                NR_dictionaryInsertInt64(handle, key, value64);
#endif
            }
            else if (value is UInt16 || value is UInt32 || value is UInt64)
            {
#if UNITY_IPHONE
                UInt64 value64 = Convert.ToUInt64(value);
                NR_dictionaryInsertUInt64(handle, key, value64);
#endif
            }
            else if (value is float || value is Single || value is Double || value is Decimal)
            {
#if UNITY_IPHONE
                NR_dictionaryInsertDouble(handle, key, Convert.ToDouble(value));
#endif
            }
            else
            {
                UnityEngine.Debug.LogError("ERROR: New Relic dictionary bridge: object \'" + value.GetType().Name + "\' invalid type for dictionary value. Values must be string or numeric.");
                return false;
            }
            return true;
        }
    }

    public class Timer : System.IDisposable
    {
#if UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern void NR_disposeTimer(System.IntPtr timer);

        [DllImport("__Internal")]
        private static extern System.IntPtr NR_createTimer();

        [DllImport("__Internal")]
        private static extern void NR_stopTimer(System.IntPtr timer);
#endif

        #region IDisposable implementation

        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
#if UNITY_IPHONE
                //NR_disposeTimer(handle);
                handle = System.IntPtr.Zero;
#endif
                disposed = true;
            }
        }

        ~Timer()
        {
            Dispose();
        }

        #endregion

#if UNITY_IPHONE
        public System.IntPtr handle { get; private set; }
#elif UNITY_ANDROID
    public System.DateTime start { get; private set; }
    public System.DateTime end { get; private set; }
#endif

        bool _running = false;

        public Timer()
        {
            Start();
        }

        void Start()
        {
            if (_running)
            {
                return;
            }

            _running = true;

#if UNITY_IPHONE
            handle = NR_createTimer();
#elif UNITY_ANDROID
        start = System.DateTime.UtcNow;
#endif
        }

        public void Stop()
        {
            if (!_running)
            {
                return;
            }

            _running = false;

#if UNITY_IPHONE
            NR_stopTimer(handle);
#elif UNITY_ANDROID
        end = System.DateTime.UtcNow;
#endif
        }

    }

    public abstract class NewRelic
    {
        protected NewRelicAgent plugin = null;
        protected string collectorAddress = "";
        protected string crashCollectorAddress = "";
        protected string platformVersion = "#PLATFORM_VERSION#";

        public NewRelic(NewRelicAgent plugin)
        {
            this.plugin = plugin;
        }


        abstract public void start(string applicationToken);

        abstract public void crashNow(string message);

        public void crashNow()
        {
            crashNow("");
        }


        // Configuration

        abstract public void enableFeatures(int features);

        abstract public void disableFeatures(int features);

        abstract public void enableCrashReporting(bool enabled);

        abstract public void setApplicationVersion(string version);

        abstract public void setApplicationBuild(string buildNumber);

        abstract public string currentSessionId();

        // Custom Instrumentation

        abstract public string startInteractionWithName(string name);

        abstract public void stopCurrentInteraction(string interactionIdentifier);

        abstract public void startTracingMethod(string methodname, string className, Timer timer, NewRelicAgent.NRTraceType category);

        abstract public void endTracingMethodWithTimer(Timer timer);

        // Metrics

        abstract public void recordMetricWithName(string name, string category);

        abstract public void recordMetricWithName(string name, string category, double value);

        abstract public void recordMetricWithName(string name, string category, double value, NewRelicAgent.MetricUnit valueUnits);

        abstract public void recordMetricWithName(string name,
                                         string category,
                                         double value,
                                         NewRelicAgent.MetricUnit valueUnits,
                                         NewRelicAgent.MetricUnit countUnits);

        // Networking

        abstract public void noticeHttpTransaction(string url,
                string httpMethod,
                int statusCode,
                long startTime,
                long endTime,
                long bytesSent,
                long bytesReceived,
                string responseBody,
                Dictionary<string, object> dtHeaders);

        abstract public void noticeNetworkFailure(string url,
                                         string httpMethod,
                                         Timer timer,
                                         NewRelicAgent.NetworkFailureCode failureCode,
                                         string message);


        // Insights Events

        abstract public void setMaxEventPoolSize(uint size);

        abstract public void setMaxEventBufferTime(uint seconds);

        abstract public bool setAttribute(string name, string value);

        abstract public bool setAttribute(string name, double value);

        abstract public bool incrementAttribute(string name);

        abstract public bool incrementAttribute(string name, double amount);

        abstract public bool removeAttribute(string name);

        abstract public bool removeAllAttributes();

        abstract public void recordHandleException(Exception exception);

        abstract public bool recordBreadcrumb(string name, Dictionary<string, object> attributes);

        abstract public bool recordCustomEvent(string eventName, Dictionary<string, object> attributes);

        abstract public Dictionary<string, object> noticeDistributedTrace();

        abstract public bool setUserId(string userId);


    }
}