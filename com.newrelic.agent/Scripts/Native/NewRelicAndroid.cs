#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NewRelic.Utilities;
using UnityEngine;

namespace NewRelic.Native
{

    public class NewRelicAndroid : NewRelic
    {

        private AndroidJavaObject activityContext = null;
        private AndroidJavaObject pluginInstance = null;
        private AndroidJavaObject agentInstance = null;
        private AndroidJavaClass unityApiClass = null;

        public NewRelicAndroid(NewRelicAgent plugin) : base(plugin)
        {

            using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
                if (activityContext == null)
                {
                    UnityEngine.Debug.LogError("NewRelicAndroid: Could not load activity context.");
                }

                pluginInstance = new AndroidJavaClass("com.newrelic.agent.android.NewRelic");
                if (pluginInstance == null)
                {
                    UnityEngine.Debug.LogError("NewRelicAndroid: Could not instantiate NewRelic plugin class.");
                }

                unityApiClass = new AndroidJavaClass("com.newrelic.agent.android.unity.NewRelicUnity");
                if (unityApiClass == null)
                {
                    UnityEngine.Debug.LogError("NewRelicAndroid: unable to instantiate a NewRelicUnity class.");
                }
            }
        }

        protected bool isValid()
        {
            return (pluginInstance != null);
        }

        protected bool initialize(string applicationToken)
        {
            if (isValid())
            {
                agentInstance = pluginInstance.CallStatic<AndroidJavaObject>("withApplicationToken", applicationToken);
                if (agentInstance == null)
                {
                    UnityEngine.Debug.LogError("NewRelicAndroid: NewRelic plugin initialization failed: could not instantiate an agent instance.");
                }
            }
            return (agentInstance != null);
        }


        protected AndroidJavaObject dictionaryToHashMap(Dictionary<string, object> dictionary)
        {
            // Convert the C# Dictionary<string,object> to a Java Map<String,Object>
            AndroidJavaObject mapInstance = new AndroidJavaClass("java.util.HashMap");
            if (mapInstance == null)
            {
                UnityEngine.Debug.LogError("NewRelicAndroid: Could not instantiate HashMap class.");
            }

            // Call 'put' via the JNI instead of using helper classes to avoid:
            //  "JNI: Init'd AndroidJavaObject with null ptr!"
            IntPtr putMethod = AndroidJNIHelper.GetMethodID(mapInstance.GetRawClass(), "put",
                                                             "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
            object[] putCallArgs = new object[2];

            if (dictionary != null)
            {
                foreach (KeyValuePair<string, object> kvp in dictionary)
                {
                    var value = kvp.Value;  // kvp.Value is read-only

                    if (!(value is string || kvp.Value is double))
                    {
                        try
                        {
                            value = Double.Parse(value.ToString());
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogErrorFormat("Coercion from [{0}] to [{1}] failed: {2}",
                                kvp.Key.GetType(), Double.MaxValue.GetType(), e.Message);
                        }
                    }

                    if (value is string || value is double)
                    {
                        using (AndroidJavaObject k = new AndroidJavaObject("java.lang.String", (object)kvp.Key))
                        {
                            if (value is string)
                            {
                                using (AndroidJavaObject v = new AndroidJavaObject("java.lang.String", (object)value))
                                {
                                    putCallArgs[0] = k;
                                    putCallArgs[1] = v;
                                    AndroidJNI.CallObjectMethod(mapInstance.GetRawObject(),
                                        putMethod, AndroidJNIHelper.CreateJNIArgArray(putCallArgs));
                                }
                            }
                            else
                            {
                                using (AndroidJavaObject v = new AndroidJavaObject("java.lang.Double", (object)value))
                                {
                                    putCallArgs[0] = k;
                                    putCallArgs[1] = v;
                                    AndroidJNI.CallObjectMethod(mapInstance.GetRawObject(),
                                        putMethod, AndroidJNIHelper.CreateJNIArgArray(putCallArgs));
                                }
                            }
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("NewRelicAndroid: Unsupported type - value must be either string or double: " + kvp.Value);
                        return null;
                    }
                }
            }

            return mapInstance;
        }

        private void withBuilderMethods(AndroidJavaObject agentInstance)
        {


            using (AndroidJavaObject platform = new AndroidJavaClass("com.newrelic.agent.android.ApplicationFramework").GetStatic<AndroidJavaObject>("Unity"))
            {
                agentInstance.Call<AndroidJavaObject>("withApplicationFramework", platform, "1.3.0");
            }


            bool loggingEnabled = plugin.logLevel != NewRelicAgent.AgentLogLevel.NONE;

            agentInstance.Call<AndroidJavaObject>("withLoggingEnabled", loggingEnabled);
            if (loggingEnabled)
            {
                agentInstance.Call<AndroidJavaObject>("withLogLevel", (int)plugin.logLevel);
            }

            if (plugin.applicationVersion.Length > 0)
            {
                agentInstance.Call<AndroidJavaObject>("withApplicationVersion", plugin.applicationVersion);
            }

            if (plugin.applicationBuild.Length > 0)
            {

                agentInstance.Call<AndroidJavaObject>("withApplicationBuild", plugin.applicationBuild);
            }

            agentInstance.Call<AndroidJavaObject>("withCrashReportingEnabled", plugin.crashReporting);
            if (plugin.crashReporting)
            {
                if (crashCollectorAddress.Length > 0)
                {
                    agentInstance.Call<AndroidJavaObject>("usingCrashCollectorAddress", crashCollectorAddress);
                }
            }

            if (collectorAddress.Length > 0)
            {
                agentInstance.Call<AndroidJavaObject>("usingCollectorAddress", collectorAddress);
            }
        }


        override public void start(string applicationToken)
        {
            if (initialize(applicationToken))
            {
                // Call the NewRelic builder methods. Must be call *before** start() method
                withBuilderMethods(agentInstance);
                AndroidJavaClass featureFlags = new AndroidJavaClass("com.newrelic.agent.android.FeatureFlag");
                AndroidJavaObject nativeReportingFlag = featureFlags.GetStatic<AndroidJavaObject>("NativeReporting");
                agentInstance.CallStatic("enableFeature", nativeReportingFlag);

                AndroidJavaObject OfflineStorage = featureFlags.GetStatic<AndroidJavaObject>("OfflineStorage");
                if (plugin.offlineStorageEnabled)
                {
                    agentInstance.CallStatic("enableFeature", OfflineStorage);
                }
                else
                {
                    agentInstance.CallStatic("disableFeature", OfflineStorage);

                }

                // finally, start the agent
                agentInstance.Call("start", this.activityContext);
            }
            else
            {
                UnityEngine.Debug.LogError("NewRelicAndroid: NewRelic plugin initialization failed: no instance");
            }
        }

        override public void crashNow(string message)
        {
            throw new SystemException(message);
        }


        // Configuration

        override public void enableFeatures(int features)
        {
            if (NewRelicAgent.FeatureFlag.HttpResponseBodyCapture.Equals((features & (int)NewRelicAgent.FeatureFlag.HttpResponseBodyCapture)))
            {
                agentInstance.Call("withHttpResponseBodyCaptureEnabled", true);
            }

            if (NewRelicAgent.FeatureFlag.CrashReporting.Equals((features & (int)NewRelicAgent.FeatureFlag.CrashReporting)))
            {
                agentInstance.Call("withCrashReportingEnabled", true);
            }

            if (NewRelicAgent.FeatureFlag.AnalyticsEvents.Equals((features & (int)NewRelicAgent.FeatureFlag.AnalyticsEvents)))
            {
                agentInstance.Call("withAnalyticsEvents", true);
            }

        }

        override public void disableFeatures(int features)
        {
            if (NewRelicAgent.FeatureFlag.HttpResponseBodyCapture.Equals((features & (int)NewRelicAgent.FeatureFlag.HttpResponseBodyCapture)))
            {
                agentInstance.Call("withHttpResponseBodyCaptureEnabled", false);
            }

            if (NewRelicAgent.FeatureFlag.CrashReporting.Equals((features & (int)NewRelicAgent.FeatureFlag.CrashReporting)))
            {
                agentInstance.Call("withCrashReportingEnabled", false);
            }

            if (NewRelicAgent.FeatureFlag.AnalyticsEvents.Equals((features & (int)NewRelicAgent.FeatureFlag.AnalyticsEvents)))
            {
                agentInstance.Call("withAnalyticsEvents", false);
            }
        }

        override public void enableCrashReporting(bool enabled)
        {
            pluginInstance.Call<AndroidJavaObject>("withCrashReportingEnabled", enabled);
        }

        override public void setApplicationVersion(string version)
        {
            agentInstance.Call<AndroidJavaObject>("withApplicationVersion", version);
        }

        override public void setApplicationBuild(string buildNumber)
        {
            agentInstance.Call<AndroidJavaObject>("withApplicationBuild", buildNumber);
        }

        override public string currentSessionId()
        {
            return pluginInstance.CallStatic<string>("currentSessionId");
        }


        // Custom Instrumentation

        override public string startInteractionWithName(string name)
        {
            string interactionTraceId = pluginInstance.CallStatic<string>("startInteraction", name);
            return interactionTraceId;
        }

        override public void stopCurrentInteraction(string interactionIdentifier)
        {
            pluginInstance.CallStatic("endInteraction", interactionIdentifier);
        }

        override public void startTracingMethod(string methodName, string className, Timer timer, NewRelicAgent.NRTraceType category)
        {
            using (AndroidJavaClass traceMachineClass = new AndroidJavaClass("com.newrelic.agent.android.tracing.TraceMachine"))
            {
                traceMachineClass.CallStatic("enterMethod", className + "." + methodName);
            }
        }

        override public void endTracingMethodWithTimer(Timer timer)
        {
            using (AndroidJavaClass traceMachineClass = new AndroidJavaClass("com.newrelic.agent.android.tracing.TraceMachine"))
            {
                traceMachineClass.CallStatic("exitMethod");
            }
        }


        // Metrics

        private Dictionary<string, AndroidJavaObject> metricUnitCache = new Dictionary<string, AndroidJavaObject>();

        private AndroidJavaObject metricUnitToEnum(NewRelicAgent.MetricUnit mu)
        {
            AndroidJavaObject metricUnit = null;
            string unit = mu.ToString();

            if (unit != null)
            {
                if (metricUnitCache.ContainsKey(unit))
                {
                    metricUnit = metricUnitCache[unit];
                }
                else
                {
                    try
                    {
                        metricUnit = new AndroidJavaClass("com.newrelic.agent.android.metric.MetricUnit").GetStatic<AndroidJavaObject>(unit);
                    }
                    catch (AndroidJavaException)
                    {
                        UnityEngine.Debug.LogError("NewRelicAgent.metricUnitToEnum: invalid MetricUnit passed [" + unit + "]");
                    }
                    metricUnitCache.Add(unit, metricUnit);
                }
            }
            return metricUnit;
        }

        override public void recordMetricWithName(string name, string category)
        {
            pluginInstance.CallStatic("recordMetric", name, category);
        }

        override public void recordMetricWithName(string name, string category, double value)
        {
            pluginInstance.CallStatic("recordMetric", name, category, value);
        }

        override public void recordMetricWithName(string name, string category, double value, NewRelicAgent.MetricUnit valueUnits)
        {
            AndroidJavaObject metricUnit = metricUnitToEnum(valueUnits);
            pluginInstance.CallStatic("recordMetric", name, category, 1, value, value, metricUnit, metricUnit);
        }

        override public void recordMetricWithName(string name,
                                         string category,
                                         double value,
                                         NewRelicAgent.MetricUnit valueUnits,
                                         NewRelicAgent.MetricUnit countUnits)
        {
            pluginInstance.CallStatic("recordMetric", name, category, 1, value, value,
                metricUnitToEnum(valueUnits), metricUnitToEnum(countUnits));
        }

        private long dateTimeToMillisSinceEpoch(DateTime dateTime)
        {
            TimeSpan span = (dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            return (long)span.TotalMilliseconds;
        }


        // Networking




        override public void noticeNetworkFailure(string url,
                                         string httpMethod,
                                         Timer timer,
                                         NewRelicAgent.NetworkFailureCode failureCode,
                                         string message)
        {
            // Invoke the Android agent noticeNetworkFailure(url, httpMethod, startTime, endTime, exception)
            pluginInstance.CallStatic("noticeNetworkFailure", url, httpMethod, dateTimeToMillisSinceEpoch(timer.start), dateTimeToMillisSinceEpoch(timer.end), (int)failureCode, message);
        }


        // Insights Events


        override public void setMaxEventPoolSize(uint size)
        {
            pluginInstance.CallStatic("setMaxEventPoolSize", size);
        }

        override public void setMaxEventBufferTime(uint seconds)
        {
            pluginInstance.CallStatic("setMaxEventBufferTime", seconds);
        }

        override public bool setAttribute(string name, string value)
        {
            return pluginInstance.CallStatic<Boolean>("setAttribute", name, value);
        }

        override public bool setAttribute(string name, double value)
        {
            return pluginInstance.CallStatic<Boolean>("setAttribute", name, Convert.ToSingle(value));
        }

        override public bool incrementAttribute(string name)
        {
            return pluginInstance.CallStatic<Boolean>("incrementAttribute", name);
        }

        override public bool incrementAttribute(string name, double amount)
        {
            return pluginInstance.CallStatic<Boolean>("incrementAttribute", name, Convert.ToSingle(amount));
        }

        override public bool removeAttribute(string name)
        {
            return pluginInstance.CallStatic<Boolean>("removeAttribute", name);
        }

        override public bool removeAllAttributes()
        {
            return pluginInstance.CallStatic<Boolean>("removeAllAttributes");
        }



        private static void throwUnityException(string cause, string message, string stackTrace)
        {


            using (AndroidJavaObject unityException = new AndroidJavaObject("com.newrelic.agent.android.unity.UnityException", cause, message))
            {

                Regex stackFrameRegex = new Regex("(\\S+)\\s*\\(.*?\\)\\s*(?:(?:\\[.*\\]\\s*in\\s|\\(at\\s*\\s*)(.*):(\\d+))?",
                                                   RegexOptions.IgnoreCase | RegexOptions.Multiline);

                // break stack trace string down into stack frames
                foreach (Match frame in stackFrameRegex.Matches(stackTrace))
                {

                    string className = String.Empty;
                    string methodName = String.Empty;
                    string fileName = String.Empty;
                    string lineNumber = "0";

                    if (frame.Groups.Count > 1)
                    {
                        methodName = frame.Groups[1].Value;

                        if (!String.IsNullOrEmpty(methodName))
                        {
                            string[] splits = methodName.Split(".".ToCharArray());
                            if (splits != null && splits.Length > 1)
                            {
                                className = splits[0];
                                methodName = splits[1];
                            }
                        }

                        if (frame.Groups.Count > 2)
                        {
                            string filenameValue = frame.Groups[2].Value;
                            if (!(String.IsNullOrEmpty(filenameValue) ||
                                filenameValue.ToLower().Equals("<filename unknown>")))
                            {
                                fileName = filenameValue;
                            }

                            if (frame.Groups.Count > 3)
                            {
                                string lineNumberValue = frame.Groups[3].Value;
                                if (!String.IsNullOrEmpty(lineNumberValue))
                                {
                                    lineNumber = lineNumberValue;
                                }
                            }
                        }
                    }

                    try
                    {
                        unityException.Call("appendStackFrame", className, methodName, fileName, Convert.ToInt32(lineNumber));
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError("NewRelicAndroid: appendStackFrame[" + e.Message + "]");
                    }
                }

                using (AndroidJavaClass newRelic = new AndroidJavaClass("com.newrelic.agent.android.NewRelic"))
                {
                    try
                    {
                        newRelic.CallStatic<Boolean>("recordHandledException", unityException);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError("NewRelicAndroid: handleUnityCrash[" + e.Message + "]");
                    }
                }
            }
        }

        override public void recordHandleException(Exception exception)
        {
            logMessageHandler(exception.Message, exception.StackTrace, LogType.Exception);
        }

        public static void logMessageHandler(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                Regex logMessageRegex = new Regex(@"^(?<class>\S+):\s*(?<message>.*)");

                string exceptionClass = String.Empty;
                string exceptionMsg = String.Empty;
                Match match = logMessageRegex.Match(logString);

                if (match.Success)
                {
                    if (match.Groups.Count > 1)
                    {
                        exceptionClass = match.Groups["class"].Value;
                        exceptionMsg = match.Groups["message"].Value;
                    }
                }

                if (String.IsNullOrEmpty(stackTrace))
                {
                    stackTrace = new System.Diagnostics.StackTrace(1, true).ToString();
                }



                throwUnityException(exceptionClass, exceptionMsg, stackTrace);
            }
            else
            {
                AndroidJavaClass pluginInstance = new AndroidJavaClass("com.newrelic.agent.android.NewRelic");
                if (pluginInstance != null)
                {
                    Dictionary<string, object> attributes = new Dictionary<string, object>();
                    attributes.Add("logType", type.ToString());
                    attributes.Add("log", logString);
                    if (stackTrace.Length > 0)
                    {
                        attributes.Add("stacktrace", stackTrace);
                    }
                    AndroidJavaObject mapInstance = new AndroidJavaObject("java.util.HashMap");
                    IntPtr putMethod = AndroidJNIHelper.GetMethodID(mapInstance.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");

                    object[] args = new object[2];
                    if (attributes != null)
                    {
                        foreach (KeyValuePair<string, object> kvp in attributes)
                        {
                            using (AndroidJavaObject key = new AndroidJavaObject("java.lang.String", kvp.Key))
                            {
                                args[0] = key;
                                if (kvp.Value is string)
                                {
                                    args[1] = new AndroidJavaObject("java.lang.String", kvp.Value);
                                }
                                AndroidJNI.CallObjectMethod(mapInstance.GetRawObject(), putMethod, AndroidJNIHelper.CreateJNIArgArray(args));
                            }
                        }
                    }

                    pluginInstance.CallStatic<Boolean>("recordCustomEvent", "Mobile Unity Logs", mapInstance);
                }


            }
        }

        public static void unhandledExceptionHandler(object sender, System.UnhandledExceptionEventArgs args)
        {


            if (args != null)
            {
                if (args.ExceptionObject != null)
                {
                    if (args.ExceptionObject.GetType() == typeof(System.Exception))
                    {
                        System.Exception e = (System.Exception)args.ExceptionObject as System.Exception;
                        throwUnityException(e.GetType().ToString(), e.Message, e.StackTrace);
                    }
                }
            }
        }

        public override bool recordBreadcrumb(string name, Dictionary<string, object> attributes)
        {

            AndroidJavaObject javaMap = CreateJavaMapFromDictionary(attributes);
            return pluginInstance.CallStatic<Boolean>("recordBreadcrumb", name, javaMap);

        }

        protected AndroidJavaObject CreateJavaMapFromDictionary(Dictionary<string, object> dict)
        {
            AndroidJavaObject mapInstance = new AndroidJavaObject("java.util.HashMap");
            IntPtr putMethod = AndroidJNIHelper.GetMethodID(mapInstance.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");

            object[] args = new object[2];
            if (dict != null)
            {
                foreach (KeyValuePair<string, object> kvp in dict)
                {
                    using (AndroidJavaObject key = new AndroidJavaObject("java.lang.String", kvp.Key))
                    {
                        args[0] = key;
                        if (kvp.Value is string)
                        {
                            args[1] = new AndroidJavaObject("java.lang.String", kvp.Value);
                        }
                        else if (kvp.Value is bool)
                        {
                            args[1] = new AndroidJavaObject("java.lang.Boolean", kvp.Value);
                        }
                        else
                        {
                            try
                            {
                                double dbl = Double.Parse((kvp.Value).ToString());
                                args[1] = new AndroidJavaObject("java.lang.Double", dbl);
                            }
                            catch (Exception e)
                            {
                                UnityEngine.Debug.LogErrorFormat("NewRelicUnity: Coercion from [{0}] to [{1}] failed: {2}",
                                    kvp.Key.GetType(),
                                    Double.MaxValue.GetType(),
                                    e.Message);
                            }
                        }
                        AndroidJNI.CallObjectMethod(mapInstance.GetRawObject(), putMethod, AndroidJNIHelper.CreateJNIArgArray(args));
                    }
                }
            }
            return mapInstance;
        }

        public override void noticeHttpTransaction(string url, string httpMethod, int statusCode, long startTime, long endTime, long bytesSent, long bytesReceived, string responseBody, Dictionary<string, object> dtHeaders)
        {

            AndroidJavaObject traceAttributes = CreateJavaMapFromDictionary(dtHeaders);

            pluginInstance.CallStatic("noticeHttpTransaction", url, httpMethod, statusCode, startTime, endTime,
                                     (long)bytesSent, (long)bytesReceived, responseBody, null, null, traceAttributes);
        }

        public override bool recordCustomEvent(string eventName, Dictionary<string, object> attributes)
        {
            AndroidJavaObject javaMap = CreateJavaMapFromDictionary(attributes);
            return pluginInstance.CallStatic<Boolean>("recordCustomEvent", eventName, javaMap);
        }

        public override Dictionary<string, object> noticeDistributedTrace()
        {
            Dictionary<string, object> dtHeaders = new Dictionary<string, object>();
            AndroidJavaObject traceContext = pluginInstance.CallStatic<AndroidJavaObject>("noticeDistributedTrace", (object)null);
            AndroidJavaObject tracePayload = traceContext.Call<AndroidJavaObject>("getTracePayload");
            string headerName = tracePayload.Call<string>("getHeaderName");
            string headerValue = tracePayload.Call<string>("getHeaderValue");
            string spanId = tracePayload.Call<string>("getSpanId");
            string traceId = traceContext.Call<string>("getTraceId");
            string parentId = traceContext.Call<string>("getParentId");
            string vendor = traceContext.Call<string>("getVendor");
            string accountId = traceContext.Call<string>("getAccountId");
            string applicationId = traceContext.Call<string>("getApplicationId");

            dtHeaders.Add(tracePayload.Call<string>("getHeaderName"), tracePayload.Call<string>("getHeaderValue"));
            dtHeaders.Add(NRConstants.TRACE_PARENT, "00-" + traceContext.Call<string>("getTraceId") + "-" + traceContext.Call<string>("getParentId") + "-00");
            dtHeaders.Add(NRConstants.TRACE_STATE,
                traceContext.Call<string>("getVendor") +
                "=0-2-" +
                traceContext.Call<string>("getAccountId") +
                "-" +
                traceContext.Call<string>("getApplicationId") +
                "-" +
                traceContext.Call<string>("getParentId") +
                "----" +
                DateTimeOffset.Now.ToUnixTimeMilliseconds());

            dtHeaders.Add(NRConstants.TRACE_ID, traceId);
            dtHeaders.Add(NRConstants.ID, spanId);
            dtHeaders.Add(NRConstants.GUID, spanId);

            return dtHeaders;
        }

        public override bool setUserId(string userId)
        {

            bool userIdIsAdded = pluginInstance.CallStatic<bool>("setUserId", userId);
            return userIdIsAdded;

        }

        public override void setMaxOfflineStorageSize(uint megabytes)
        {
            pluginInstance.CallStatic("setMaxOfflineStorageSize", megabytes);
        }
    }
}

#endif