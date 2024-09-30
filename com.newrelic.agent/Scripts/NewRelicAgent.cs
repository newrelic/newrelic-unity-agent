using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.SceneManagement;
using System.Collections;
#if UNITY_IPHONE || UNITY_ANDROID
using NewRelic.Native;
#endif
using NewRelic.Networking;

namespace NewRelic
{

    public class NewRelicAgent : MonoBehaviour
    {
        /// <summary>
        /// The application token provided by NewRelic
        /// Set through the Unity object interface
        /// </summary>
        public string iOSApplicationToken = "<NewRelicAppToken>";
        public string androidApplicationToken = "<NewRelicAppToken>";

        /// <summary>
        /// The application version. Default to value set by Unity
        /// To override the default value set this field through the Unity object interface.
        /// </summary>
        public string applicationVersion = "";

        /// <summary>
        /// The application build. default to value set by Unity
        /// To override the default value set this field through the Unity object interface.
        /// </summary>
        public string applicationBuild = "";

        /// <summary>
        /// The detail of logging provided by New Relic for internal operations.
        /// defaults to <c>ERROR</c>
        /// </summary>
        public AgentLogLevel logLevel = AgentLogLevel.ERROR;

        /// <summary>
        /// Enable or disable SSL for New Relic data posts
        /// defaults to <c>true</c>
        /// </summary>
        public Boolean usingSSL = true;

        /// <summary>
        /// Enable or disable New Relic crash reporting
        /// defaults to <c>true</c>
        /// </summary>
        public Boolean crashReporting = true;

        /// <summary>
        /// enable or disable http response body capture.
        /// New Relic will capture a portion of http response bodies
        /// to allow for http error debugging.
        /// defaults to <c>true</c>
        /// </summary>
        public Boolean httpResponseBodyCapture = true;

        /// <summary>
        /// enable or disable Insights events, allows for custom events to be recorded as well
        /// defaults to <c>true</c> (only settable in Android)
        /// </summary>
        public Boolean analyticsEvents = true;

        /// <summary> Enable or disable offline data storage when no internet connection is available.
        ///  <c>true</c>
        /// </summary>
        public Boolean offlineStorageEnabled = true;

        /// <summary> Enable or disable Background Events Reporting When app is in background.
        ///  <c>true</c>
        /// </summary>
        public Boolean backgroundReportingEnabled = false;



#if UNITY_IPHONE
        /// <summary>
        /// enable disable interaction tracing.
        /// </summary>
        public Boolean interactionTracing = true;

        /// <summary>
        /// enable or disable swift interaction tracing
        /// Beware: enabling this feature may cause your swift application to crash.
        /// please read
        /// https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile/getting-started/enabling-interaction-tracing-swift
        /// before enabling this feature.
        ///
        /// disabled by default
        /// </summary>
        public Boolean swiftInteractionTracing = false;

        ///<summary>
        /// enable or disable network instrumentation for NSURLSessions
        /// enabled by default
        /// </summary>
        public Boolean URLSessionInstrumentation = true;

        public Boolean experimentalNetworkingInstrumentation = false;

        /// <summary> Enable or disable to use our new, more stable, event system for iOS agent.
        ///  <c>true</c>
        /// </summary>
        public Boolean newEventSystemEnabled = true;
#endif // UNITY_IPHONE

        protected NewRelic agentInstance = null;
        protected static NewRelicAgent instance = null;
        protected String interactionId = null;
        private NewRelicScenesToViewReporter scenesToViewReporter = null;
        private Thread _mainThread;



        /// <summary>
        /// <c>FeatureFlag</c> is used internally by the agents
        /// </summary>
        public enum FeatureFlag : int
        {
            InteractionTracing = 1 << 1,    // iOS only
            SwiftInteractionTracing = 1 << 2,   // iOS only: disabled by default
            CrashReporting = 1 << 3,    //
            URLSessionInstrumentation = 1 << 4, // iOS only
            HttpResponseBodyCapture = 1 << 5,   //
            AnalyticsEvents = 1 << 6,   //
            ExperimentalNetworkingInstrumentation = 1 << 13,    // iOS only: disabled by default
            OfflineStorage = 1 << 21,  // iOS only: enabled by default
            NewEventSystem = 1 << 20, // iOS only:enabled by default
            BackgroundReporting = 1 << 22 //disabled by default
        }


        /// <summary>
        ///	Network failure codes are used to map networking errors when using
        /// <c>noticeNetworkFailure(...)</c>
        /// used the value that best describes your network error
        /// </summary>
        public enum NetworkFailureCode : int
        {
            Unknown = -1,
            Cancelled = -999,
            BadURL = -1000,
            TimedOut = -1001,
            UnsupportedURL = -1002,
            CannotFindHost = -1003,
            CannotConnectToHost = -1004,
            DataLengthExceedsMaximum = -1103,
            NetworkConnectionLost = -1005,
            DNSLookupFailed = -1006,
            HTTPTooManyRedirects = -1007,
            ResourceUnavailable = -1008,
            NotConnectedToInternet = -1009,
            RedirectToNonExistentLocation = -1010,
            BadServerResponse = -1011,
            UserCancelledAuthentication = -1012,
            UserAuthenticationRequired = -1013,
            ZeroByteResource = -1014,
            CannotDecodeRawData = -1015,
            CannotDecodeContentData = -1016,
            CannotParseResponse = -1017,
            InternationalRoamingOff = -1018,
            CallIsActive = -1019,
            DataNotAllowed = -1020,
            RequestBodyStreamExhausted = -1021,
            FileDoesNotExist = -1100,
            FileIsDirectory = -1101,
            NoPermissionsToReadFile = -1102,
            SecureConnectionFailed = -1200,
            ServerCertificateHasBadDate = -1201,
            ServerCertificateUntrusted = -1202,
            ServerCertificateHasUnknownRoot = -1203,
            ServerCertificateNotYetValid = -1204,
            ClientCertificateRejected = -1205,
            ClientCertificateRequired = -1206,
            CannotLoadFromNetwork = -2000,
            CannotCreateFile = -3000,
            CannotOpenFile = -3001,
            CannotCloseFile = -3002,
            CannotWriteToFile = -3003,
            CannotRemoveFile = -3004,
            CannotMoveFile = -3005,
            DownloadDecodingFailedMidStream = -3006,
            DownloadDecodingFailedToComplete = -3007
        }

        /// <summary>
        /// used internally
        /// </summary>
        ///
        public enum AgentLogLevel : int
        {
            NONE = 0,
            ERROR = 1,
            WARNING = 2,
            INFO = 3,
            VERBOSE = 4,
            DEBUG = 5
        }

        public enum NRTraceType : int
        {
            None = 0,
            ViewLoading,
            Layout,
            Database,
            Images,
            Json,
            Network
        }

        public enum MetricUnit : int
        {
            PERCENT,
            BYTES,
            SECONDS,
            BYTES_PER_SECOND,
            OPERATIONS
        }

        private void InitializePlugin()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);

            try
            {
                if (agentInstance == null)
                {
#if UNITY_IPHONE
                    UnityEngine.Debug.Log("Initializing New Relic iOS agent.");
                    agentInstance = new NewRelicIos(this);
#elif UNITY_ANDROID
                        UnityEngine.Debug.Log("Initializing New Relic Android agent.");
                        agentInstance = new NewRelicAndroid(this);
#endif   // UNITY_ANDROID                  
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        private String startDefaultInteraction(String name)
        {
            if (name == null)
            {
                name = "defaultInteraction";
            }

            // create a default interaction for both platforms
            if (interactionId != null)
            {
                StopCurrentInteraction(interactionId);
            }

            interactionId = StartInteractionWithName(name);

            return interactionId;
        }

        private void runDefaultTrace()
        {
            Timer timer = new Timer();
            StartTracingMethod("runDefaultTrace", "NewRelicAgent", timer, NRTraceType.Network);
            Thread.Sleep(300);      // interactions need at least 300ms to avoid getting filtered out
            timer.Stop();
            EndTracingMethodWithTimer(timer);
        }

        private void stopDefaultInteraction()
        {
            // stop the default interaction
            if (interactionId != null)
            {
                StopCurrentInteraction(interactionId);
                interactionId = null;
            }
        }


        void Awake()
        {
#if UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_EDITOR_OSX
            UnityEngine.Debug.Log("NewRelic agent disabled when running in the editor");
#else
		        InitializePlugin ();
#endif // UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_EDITOR_OSX
        }

        void Start()
        {

#if UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_EDITOR_OSX
            UnityEngine.Debug.Log("NewRelic agent disabled when running in the editor");
#else
#if UNITY_IPHONE
		        // iOS calls StartAgent from NewRelicPostBuild script
               StartAgent ("iOS");
        
#elif UNITY_ANDROID
		        StartAgent ("Android");
          //       Application.logMessageReceivedThreaded += NewRelicAndroid.logMessageHandler;
		        //UnityEngine.Debug.Log ("logMessageReceived handler installed");

		        //System.AppDomain.CurrentDomain.UnhandledException += NewRelicAndroid.unhandledExceptionHandler;
		        //UnityEngine.Debug.Log ("UnhandledExceptionEventHandler installed");
#endif // UNITY_ANDROID
#endif // UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_EDITOR_OSX
            scenesToViewReporter = new NewRelicScenesToViewReporter();
            scenesToViewReporter.StartViewFromScene(SceneManager.GetActiveScene());

            SetAttribute("graphicsDeviceName", SystemInfo.graphicsDeviceName);
            SetAttribute("graphicsMemorySize", SystemInfo.graphicsMemorySize.ToString());
            SetAttribute("graphicsMultiThreaded", SystemInfo.graphicsMultiThreaded.ToString());
            SetAttribute("graphicsDeviceType", SystemInfo.graphicsDeviceType.ToString());
            SetAttribute("graphicsDeviceVersion", SystemInfo.graphicsDeviceVersion);
        }

        public void NewRelicLogCallback(string condition, string stackTrace, LogType type)
        {
            if (IsMainThread())
            {
                return;
            }
#if (UNITY_ANDROID && !UNITY_EDITOR)
       NewRelicAndroid.logMessageHandler(condition, stackTrace, type);

#endif

#if (UNITY_IOS)
            NewRelicIos.logMessageHandler(condition, stackTrace, type);
#endif

        }

        private bool IsMainThread()
        {
            if (_mainThread == null) return false;
            return _mainThread.Equals(Thread.CurrentThread);
        }


        void OnEnable()
        {


#if (UNITY_ANDROID && !UNITY_EDITOR)
		        Application.logMessageReceived += NewRelicAndroid.logMessageHandler;
		        UnityEngine.Debug.Log ("logMessageReceived handler installed");

		        System.AppDomain.CurrentDomain.UnhandledException += NewRelicAndroid.unhandledExceptionHandler;
		        UnityEngine.Debug.Log ("UnhandledExceptionEventHandler installed");
#endif // UNITY_ANDROID

#if (UNITY_IOS)
            Application.logMessageReceived += NewRelicIos.logMessageHandler;
#endif
        }

        void OnDisable()
        {
            // stop any running default interaction
            stopDefaultInteraction();

#if (UNITY_ANDROID && !UNITY_EDITOR)
		        Application.logMessageReceived -= NewRelicAndroid.logMessageHandler;
		        UnityEngine.Debug.Log ("LogMessageHandler removed");

		        System.AppDomain.CurrentDomain.UnhandledException -= NewRelicAndroid.unhandledExceptionHandler;
		        UnityEngine.Debug.Log ("UnhandledExceptionEventHandler removed");
#endif // UNITY_ANDROID

#if (UNITY_IOS)
            Application.logMessageReceived -= NewRelicIos.logMessageHandler;
#endif
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                stopDefaultInteraction();
            }
        }

        static internal bool validatePluginImpl()
        {
            bool isValid = false;
#if UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_EDITOR_OSX
            UnityEngine.Debug.Log("NewRelic agent disabled when running in the editor");
#else
		        isValid = instance.agentInstance != null;
#endif // UNITY_EDITOR | UNITY_EDITOR_64 | UNITY_EDITOR_OSX
            return isValid;
        }


        // public agent methods

        internal void StartAgent(string message)
        {
            string appToken = "";

#if UNITY_ANDROID
                appToken = androidApplicationToken;
#endif

#if UNITY_IPHONE
            appToken = iOSApplicationToken;
#endif


            try
            {
                if (validatePluginImpl())
                {
                    agentInstance.start(appToken);

                    String id = startDefaultInteraction("StartAgent");
                    runDefaultTrace();
                    stopDefaultInteraction();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        /// <summary>
        /// Used to test New Relic crash reporting.
        /// Calling this message will cause an immediate crash
        /// </summary>
        /// <param name="message">Exception error message</param>
        static public void CrashNow(string message)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.crashNow(message);
            }
        }

        /// <summary>
        /// used internally
        /// </summary>
        /// <param name="features">a bit-wise ored value indicating which features should be enabled</param>
        static public void EnableFeatures(int features)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.enableFeatures(features);
            }
        }

        /// <summary>
        /// used internally
        /// </summary>
        /// <param name="features">a bit-wise ored value indicating which features should be disabled</param>
        static public void DisableFeatures(int features)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.disableFeatures(features);
            }
        }

        /// <summary>
        /// used internally
        /// </summary>
        /// <param name="enabled">If set to <c>true</c> enabled.</param>
        static public void EnableCrashReporting(bool enabled)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.enableCrashReporting(enabled);
            }
        }

        /// <summary>
        /// used internally
        /// </summary>
        /// <param name="version">will replace the default version New Relic will report for this app</param>
        static public void SetApplicationVersion(string version)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.setApplicationVersion(version);
            }
        }

        /// <summary>
        /// used internally
        /// </summary>
        /// <param name="buildNumber">will replace the default build number New Relic will report for this app</param>
        static public void SetApplicationBuild(string buildNumber)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.setApplicationBuild(buildNumber);
            }
        }

        /// <summary>
        /// returns the current session idetifier used by New Relic.
        /// </summary>
        /// <returns>The session identifier.</returns>
        static public string CurrentSessionId()
        {
            string sessionId = null;

            if (validatePluginImpl())
            {
                sessionId = instance.agentInstance.currentSessionId();
            }

            return sessionId;
        }


        // Custom Instrumentation

        /// <summary>
        /// This method will start an interaction trace using <c>name</c> as the name shown in New Relic.
        /// The interaction will record all instrumented methods until a timout occurs or <code>stopCurrentInteraction</code> is called.
        /// </summary>
        /// <returns>
        /// Returns an interaction identifier that can be passed to <code>stopCurrentInteraction()</code>
        /// which will stop the current interaction if it hasn't timed out.
        /// </returns>
        /// <param name="name">Name.</param>
        static public string StartInteractionWithName(string name)
        {
            string interactionName = null;

            if (validatePluginImpl())
            {
                interactionName = instance.agentInstance.startInteractionWithName(name);
            }

            return interactionName;
        }

        /// <summary>
        /// This method will stop the Interaction trace associated with <c>interactionIdentifier</c>
        /// which is returned by <c>StartInteractionWithName()</c> It's not necessary to call this method to
        /// complete an interaction trace. (An interaction trace will intelligently complete on its own.)
        /// However, use this method if you want a more discrete interaction period.
        /// </summary>
        /// <param name="interactionIdentifier">an identifier generated by <c>StartInteractionWithName()</c></param>
        static public void StopCurrentInteraction(string interactionIdentifier)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.stopCurrentInteraction(interactionIdentifier);
            }
        }

        /// <summary>
        /// This method adds a new method trace to the currently running Interaction Trace.
        /// If no interaction trace is running, nothing will happen. This method should be
        /// called at the beginning of the method you wish to instrument. The timer parameter
        /// is a New Relic defined object that only needs to be created just prior to calling this
        ///	method and must stay in memory until it is passed to the <c>endTracingMethodWithTimer()</c>
        /// method call at the end of the custom instrumented method.
        /// </summary>
        /// <param name="methodName">the method name we are tracing</param>
        /// <param name="className">the class owning the method</param>
        /// <param name="timer">timer object used to time the method</param>
        /// <param name="category">a category defined by <c>NRTraceType</c> </param>
        static public void StartTracingMethod(String methodName, string className, Timer timer, NRTraceType category)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.startTracingMethod(methodName, className, timer, category);
            }
        }

        /// <summary>
        /// This method should be called at the end of any method you instrument with <c>startTracingMethod()</c>.
        /// Failure to do so will result in an unhealthy trace timeout of the currently running interaction.
        /// If no interaction is running this method is a no-op.
        /// </summary>
        /// <param name="timer">The timer used in the <c>StartTracingMethod()</c> </param>
        static public void EndTracingMethodWithTimer(Timer timer)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.endTracingMethodWithTimer(timer);
            }
        }


        // Metrics
        /// <summary>
        /// This method will record a metric without units and a value of 1
        /// </summary>
        /// <param name="name">the metric name.</param>
        /// <param name="category">a descriptive category</param>
        static public void RecordMetricWithName(string name, string category)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.recordMetricWithName(name, category);
            }
        }

        /// <summary>
        /// This method will record a metric without units
        /// </summary>
        /// <param name="name">the metric name</param>
        /// <param name="category">a descriptive category</param>
        /// <param name="value">the value to record.</param>
        static public void RecordMetricWithName(string name, string category, double value)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.recordMetricWithName(name, category, value);
            }
        }

        /// <summary>
        /// This method adds on the last with the addition of setting the value Unit
        /// The unit names may be mixed case and must consist strictly of alphabetical characters
        /// as well as the _, % and / symbols. Case is preserved.
        /// Recommendation: Use uncapitalized words, spelled out in full. For example, use second not Sec.
        /// While there are a few predefined units please feel free to add your own by typecasting an NSString.
        /// </summary>
        /// <param name="name">the metric name</param>
        /// <param name="category">a descriptive category</param>
        /// <param name="value">the value to record.</param>
        /// <param name="valueUnits">the units of value</param>
        static public void RecordMetricWithName(string name, string category, double value, MetricUnit valueUnits)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.recordMetricWithName(name, category, value, valueUnits);
            }
        }

        /// <summary>
        ///This method adds on the last with the addition of setting the optional parameter countUnits.
        /// The unit names may be mixed case and must consist strictly
        /// of alphabetical characters as well as the _, % and / symbols. Case is preserved.
        /// Recommendation: Use uncapitalized words, spelled out in full. For example, use second not Sec.
        /// While there are a few predefined units please feel free to add your own by typecasting an NSString.
        /// </summary>
        /// <param name="name">the metric name</param>
        /// <param name="category">a descriptive category</param>
        /// <param name="value">the value to record.</param>
        /// <param name="valueUnits">the units of value</param>
        /// <param name="countUnits">represents the unit of the metric</param>
        static public void RecordMetricWithName(string name,
                                                 string category,
                                                 double value,
                                                 MetricUnit valueUnits,
                                                 MetricUnit countUnits)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.recordMetricWithName(name, category, value, valueUnits, countUnits);
            }
        }


        // Networking
        /// <summary>
        /// New Relic will track the URL, response time, status code, and data send/received.
        /// If the response headers dictionary contains a X-NewRelic-AppData header, New Relic
        /// will track the association between the mobile app and the web server and
        /// display the correlation and the server vs. network vs. queue time in the New Relic UI.
        ///
        /// If the HTTP status code indicates an error (400 and above) New Relic will also
        /// track this request as a server error, optionally capturing the response type
        /// and encoding from the headers dictionary and the response body data as a
        // server error in the New Relic UI.
        /// </summary>
        /// <param name="URL">the URL of the request</param>
        /// <param name="httpMethod">HTTP method</param>
        /// <param name="timer">A timer created when the network request started</param>
        /// <param name="headers">Headers dictionary (optional)</param>
        /// <param name="httpStatusCode">http response code</param>
        /// <param name="bytesSent">Bytes sent.</param>
        /// <param name="bytesReceived">Bytes received.</param>
        /// <param name="responseData">Response data.</param>
        /// <param name="parameters">unused</param>
        static public void NoticeHttpTransaction(string URL,
                                                 string httpMethod,
                                                 int httpStatusCode,
                                                 long startTime,
                                                 long endTime,
                                                 int bytesSent,
                                                 int bytesReceived,
                                                 string responseBody,
                                                 Dictionary<string, object> dtHeaders
                                                )
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.noticeHttpTransaction(URL, httpMethod, httpStatusCode, startTime, endTime, bytesSent, bytesReceived, responseBody, dtHeaders);
            }
        }

        /// <summary>
        /// records a failed network transaction.
        /// </summary>
        /// <param name="url">the URL of the request</param>
        /// <param name="httpMethod">HTTP method</param>
        /// <param name="timer">a timer created when the network request was started</param>
        /// <param name="failureCode">Failure code defined from <c>NewRelicAgent.NetworkFailureCode</c> list</param>
        /// <param name="message">optional descriptive message, unused in iOS</param>
        static public void NoticeNetworkFailure(string url,
                                                 string httpMethod,
                                                 Timer timer,
                                                 NewRelicAgent.NetworkFailureCode failureCode,
                                                 string message)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.noticeNetworkFailure(url, httpMethod, timer, failureCode, message);
            }
        }

        /// <summary>
        /// records a failed network transaction.
        /// </summary>
        /// <param name="url">the URL of the request</param>
        /// <param name="httpMethod">HTTP method</param>
        /// <param name="timer">a timer created when the network request was started</param>
        /// <param name="failureCode">Failure code defined from <c>NewRelicAgent.NetworkFailureCode</c> list</param>
        /// <param name="message">optional descriptive message, unused in iOS</param>
        static public void NoticeNetworkFailure(string url,
                                                 string httpMethod,
                                                 long startTime,
                                                 long endTime,
                                                 NewRelicAgent.NetworkFailureCode failureCode,
                                                 string message)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.noticeNetworkFailure(url, httpMethod, startTime, endTime, failureCode, message);
            }
        }


        /// <summary>
        /// Sets the size of the max event pool.
        /// default 1000 events at a time.
        /// if more events are recorded before <c>maxEventBufferTime</c> seconds elapsed, events will be sampled using
        /// a Reservoir Sampling algorithm.
        /// If <c>maxEventBufferTime</c> seconds elapse, the existing event buffer will be transmitted and then emptied.
        /// </summary>
        /// <param name="size">max even pool size.</param>
        static public void SetMaxEventPoolSize(uint size)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.setMaxEventPoolSize(size);
            }
        }

        /// <summary>
        /// Change the maximum length of time before the SDK sends queued events to New Relic.
        /// the default timeout is 600 seconds.
        /// If the user keeps your app open for longer than that, any stored events will be
        /// transmitted and the timer resets.
        /// </summary>
        /// <param name="seconds">max event buffer time</param>
        static public void SetMaxEventBufferTime(uint seconds)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.setMaxEventBufferTime(seconds);
            }
        }

        /// <summary>
        /// records an attribute that will be added to all events in this app install
        /// </summary>
        /// <returns><c>true</c>, if attribute was set, <c>false</c> otherwise.</returns>
        /// <param name="name">the attribute name</param>
        /// <param name="value">the attribute value</param>
        static public bool SetAttribute(string name, string value)
        {
            bool result = false;

            if (validatePluginImpl())
            {
                result = instance.agentInstance.setAttribute(name, value);
            }

            return result;
        }

        /// <summary>
        /// records an attribute that will be added to all events in this app install
        /// </summary>
        /// <returns><c>true</c>, if attribute was set, <c>false</c> otherwise.</returns>
        /// <param name="name">the attribute name</param>
        /// <param name="value">the attribute value</param>
        static public bool SetAttribute(string name, double value)
        {
            bool result = false;

            if (validatePluginImpl())
            {
                result = instance.agentInstance.setAttribute(name, value);
            }

            return result;
        }

        /// <summary>
        /// increments an attribute that will be added to all events in this app install
        /// if no attribute exists one will be created with value of 1
        /// </summary>
        /// <returns><c>true</c>, if attribute was set, <c>false</c> otherwise.</returns>
        /// <param name="name">the attribute name</param>
        static public bool IncrementAttribute(string name)
        {
            bool result = false;

            if (validatePluginImpl())
            {
                result = instance.agentInstance.incrementAttribute(name);
            }

            return result;
        }

        /// <summary>
        /// increments an attribute that will be added to all events in this app install
        /// if no attribute exists one will be created with value of <c>amount</c>
        /// </summary>
        /// <returns><c>true</c>, if attribute was incremented, <c>false</c> otherwise.</returns>
        /// <param name="name">Name.</param>
        /// <param name="amount">Amount.</param>
        static public bool IncrementAttribute(string name, double amount)
        {
            bool result = false;

            if (validatePluginImpl())
            {
                result = instance.agentInstance.incrementAttribute(name, amount);
            }

            return result;
        }

        /// <summary>
        /// Removes the attribute with <c>name</c>
        /// </summary>
        /// <returns><c>true</c>, if attribute was removed, <c>false</c> otherwise.</returns>
        /// <param name="name">Name.</param>
        static public bool RemoveAttribute(string name)
        {
            bool result = false;

            if (validatePluginImpl())
            {
                result = instance.agentInstance.removeAttribute(name);
            }

            return result;
        }

        /// <summary>
        /// Removes all attributes.
        /// </summary>
        /// <returns><c>true</c>, if all attributes was removed, <c>false</c> otherwise.</returns>
        static public bool RemoveAllAttributes()
        {
            bool result = false;

            if (validatePluginImpl())
            {
                result = instance.agentInstance.removeAllAttributes();
            }

            return result;
        }

        /// <summary>
        /// Record BreadCrumb.
        /// </summary>
        /// <returns><c>true</c>, if all attributes was removed, <c>false</c> otherwise.</returns>
        /// <param name="name">Name of the breadcrumb. Will be stored in the 'category' attribute of Mobile events in New Relic Insights</param>
        /// <param name="attributes">dictionary of attributes associated with the event. Attributes must be a string or numeric value.</param>
        static public bool RecordBreadCrumb(string name, Dictionary<string, object> attributes)
        {
            bool result = false;

            if (validatePluginImpl())
            {
                result = instance.agentInstance.recordBreadcrumb(name, attributes);
            }

            return result;
        }


        static public bool RecordCustomEvent(string eventName, Dictionary<string, object> attributes)
        {
            bool result = false;

            if (validatePluginImpl())
            {
                result = instance.agentInstance.recordCustomEvent(eventName, attributes);
            }

            return result;
        }



        static public NewRelicHttpClientHandler getHttpMessageHandler()
        {
            return new NewRelicHttpClientHandler(instance.agentInstance);
        }



        static public void RecordException(Exception exception)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.recordHandleException(exception);
            }
        }

        static public void SetUserId(string userId)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.setUserId(userId);
            }
        }

        static public Dictionary<string, object> NoticeDistributedTrace()
        {
            if (validatePluginImpl())
            {
                return instance.agentInstance.noticeDistributedTrace();
            }

            return null;
        }

        /// <summary>
        ///Sets the maximum size of total data that can be stored for offline storage.By default, mobile monitoring can collect a maximum of 100 megaBytes of offline storage.
        ///When a data payload fails to send because the device doesn't have an internet connection, it can be stored in the file system until an internet connection has been made.
        ///After a typical harvest payload has been successfully sent, all offline data is sent to New Relic and cleared from storage.
        /// </summary>
        /// <param name="megabytes">size of the max offline events storage.</param>
        static public void SetMaxOfflineStorageSize(uint megabytes)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.setMaxOfflineStorageSize(megabytes);
            }
        }


        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        static public void LogInfo(String message)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.LogInfo(message);
            }
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        static public void LogError(String message)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.LogError(message);
            }
        }

        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        static public void LogVerbose(String message)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.LogVerbose(message);
            }
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        static public void LogWarning(String message)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.LogWarning(message);
            }
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        static public void LogDebug(String message)
        {
            if (validatePluginImpl())
            {
                instance.agentInstance.LogDebug(message);
            }
        }

        /// <summary>
        /// Logs a message with a specified log level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The message to log.</param>
        static public void Log(NewRelicAgent.AgentLogLevel level, String message)
        {
            if (validatePluginImpl())
            {
                String logLevel = LogLevelString(level);
                instance.agentInstance.Log(level, message);
            }
        }



        /// <summary>
        /// Logs a set of attributes.
        /// </summary>
        /// <param name="attributes">The attributes to log.</param>
        static public void LogAttributes(Dictionary<string, object> attributes)
        {
            if (validatePluginImpl())
            {



                if ((attributes.ContainsKey("level") && attributes.GetValueOrDefault("level").GetType() == typeof(AgentLogLevel)))
                {
                    attributes["level"] = LogLevelString((AgentLogLevel)attributes.GetValueOrDefault("level"));
                }



                instance.agentInstance.LogAttributes(attributes);
            }
        }


        public static String LogLevelString(AgentLogLevel agentLogLevel)
        {

            String logLevel = "INFO";

            if (agentLogLevel.Equals(AgentLogLevel.DEBUG))
            {
                logLevel = "DEBUG";
            }
            else if (agentLogLevel.Equals(AgentLogLevel.ERROR))
            {
                logLevel = "ERROR";
            }
            else if (agentLogLevel.Equals(AgentLogLevel.INFO))
            {
                logLevel = "INFO";
            }
            else if (agentLogLevel.Equals(AgentLogLevel.VERBOSE))
            {
                logLevel = "VERBOSE";
            }
            else if (agentLogLevel.Equals(AgentLogLevel.WARNING))
            {
                logLevel = "WARN";
            }



            return logLevel;
        }
    }
}
