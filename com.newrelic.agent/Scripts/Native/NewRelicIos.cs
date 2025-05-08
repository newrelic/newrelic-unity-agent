using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Text.RegularExpressions;
using NewRelic.Utilities;


#if UNITY_IPHONE
namespace NewRelic.Native
{

    public class NewRelicIos : NewRelic
	{


		[DllImport("__Internal")]
		private static extern System.IntPtr NR_generateDistributedTracingHeaders();

		[DllImport("__Internal")]
		private static extern string NR_dictionarygetStringValueByKey(System.IntPtr dictionary, string key);

		[DllImport("__Internal")]
		private static extern System.IntPtr NR_ArrayCreate();
		[DllImport("__Internal")]
		private static extern void NR_arrayInsertDictionary(System.IntPtr array, System.IntPtr dic);

		[DllImport("__Internal")]
		private static extern void NR_dictionaryInsertArray(System.IntPtr dictionary, string key, System.IntPtr value);

		[DllImport("__Internal")]
		private static extern System.IntPtr NR_dictionaryCreate();
		[DllImport("__Internal")]
		private static extern void NR_dictionaryInsertString(System.IntPtr dict, string key, string value);
		[DllImport("__Internal")]
		private static extern void NR_dictionaryInsertInt64(System.IntPtr dict, string key, Int64 value);
		[DllImport("__Internal")]
		private static extern void NR_dictionaryInsertUInt64(System.IntPtr dict, string key, UInt64 value);
		[DllImport("__Internal")]
		private static extern void NR_dictionaryInsertDouble(System.IntPtr dict, string key, double value);
		[DllImport("__Internal")]
		private static extern void NR_dictionaryInsertFloat(System.IntPtr dict, string key, float value);
		[DllImport("__Internal")]
		private static extern void NR_dictionaryInsertBool(System.IntPtr dict, string key, bool value);


		[DllImport("__Internal")]
		private static extern void NR_useSSL(bool useSSL);

		[DllImport("__Internal")]
		private static extern void NR_logLevel(int logLevel);

		[DllImport("__Internal")]
		private static extern void NR_crashNow(string message);

		[DllImport("__Internal")]
		private static extern void NewRelic_startWithApplicationToken(string appToken);

		[DllImport("__Internal")]
		private static extern void NR_enableFeatures(int features);

		[DllImport("__Internal")]
		private static extern void NR_disableFeatures(int features);

		[DllImport("__Internal")]
		private static extern void NR_enableCrashReporting(bool enabled);

		[DllImport("__Internal")]
		private static extern void NR_setApplicationVersion(String version);

		[DllImport("__Internal")]
		private static extern void NR_setApplicationBuild(String buildNumber);

		[DllImport("__Internal")]
		private static extern String NR_currentSessionId();

		[DllImport("__Internal")]
		private static extern String NR_startInteractionWithName(String name);

		[DllImport("__Internal")]
		private static extern void NR_stopCurrentInteraction(String interactionIdentifier);

		[DllImport("__Internal")]
		private static extern void NR_startTracingMethod(String methodName, String className, System.IntPtr timer, int category);

		[DllImport("__Internal")]
		private static extern void NR_endTracingMethodWithTimer(System.IntPtr timer);

		//metrics
		[DllImport("__Internal")]
		private static extern void NR_recordMetricsWithName(String name, String category);

		[DllImport("__Internal")]
		private static extern void NR_recordMetricsWithNameValue(String name, String category, double value);

		[DllImport("__Internal")]
		private static extern void NR_recordMetricsWithNameValueUnits(String name, String category, double value, String valueUnits);

		[DllImport("__Internal")]
		private static extern void NR_recordMetricsWithNameValueAndCountUnits(String name, String category, double value, String valueUnits, String countUnits);


		//Networking
		[DllImport("__Internal")]
		private static extern void NR_noticeNetworkRequest(string url, string httpMethod, int statusCode, long startTime, long endTime, long bytesSent, long bytesReceived, string responseBody, System.IntPtr traceAttributes);
		[DllImport("__Internal")]
		private static extern void NR_noticeNetworkFailureWithTimer(string url,
															string httpMethod,
															System.IntPtr timer,
															int failureCode);

        [DllImport("__Internal")]
        private static extern void NR_noticeNetworkFailure(string url,
                                                    string httpMethod,
                                                    long startTime,
													long endTime,
                                                    int failureCode);

        //Insights Events


        [DllImport("__Internal")]
		private static extern bool NR_setMaxEventPoolSize(uint size);

		[DllImport("__Internal")]
		private static extern void NR_setMaxEventBufferTime(uint seconds);

        [DllImport("__Internal")]
        private static extern void NR_setMaxOfflineStorageSize(uint megabytes);

        [DllImport("__Internal")]
		private static extern bool NR_setAttributeStringValue(String name, String value);

		[DllImport("__Internal")]
		private static extern bool NR_setAttributeDoubleValue(String name, double value);

		[DllImport("__Internal")]
		private static extern bool NR_incrementAttribute(String name);

		[DllImport("__Internal")]
		private static extern bool NR_incrementAttributeWithValue(String name, double amount);

		[DllImport("__Internal")]
		private static extern bool NR_removeAttribute(String name);

		[DllImport("__Internal")]
		private static extern bool NR_removeAllAttributes();

		[DllImport("__Internal")]
		private static extern bool NR_setUserId(String userId);

		[DllImport("__Internal")]
		private static extern bool NR_recordBreadcrumb(string name, System.IntPtr nSDict);

		[DllImport("__Internal")]
		private static extern void NR_setPlatform(String version);

		[DllImport("__Internal")]
		private static extern bool NR_recordCustomEvent(string eventName, System.IntPtr attributes);

		[DllImport("__Internal")]
		private static extern void NR_recordHandledExceptionWithStackTrace(System.IntPtr attributes);
		
		[DllImport("__Internal")]
		private static extern void NR_logInfo(string message);
		
		[DllImport("__Internal")]
		private static extern void NR_logError(string message);
		
		[DllImport("__Internal")]
		private static extern void NR_logWarning(string message);
		
		[DllImport("__Internal")]
		private static extern void NR_logVerbose(string message);
		
		[DllImport("__Internal")]
		private static extern void NR_logDebug(string message);
		
		[DllImport("__Internal")]
		private static extern void NR_logAttributes(System.IntPtr nSDict);
		

		
		

		public void useSSL(bool useSSL)
		{
			NR_useSSL(useSSL);
		}

		public NewRelicIos(NewRelicAgent plugin) : base(plugin)
		{
			useSSL(plugin.usingSSL);

			if (plugin.applicationBuild != null && plugin.applicationBuild.Length > 0)
			{
				setApplicationBuild(plugin.applicationBuild);
			}

			if (plugin.applicationVersion != null && plugin.applicationVersion.Length > 0)
			{
				setApplicationVersion(plugin.applicationVersion);
			}

			logLevel((int)plugin.logLevel);

			enableCrashReporting(plugin.crashReporting);

			if (plugin.interactionTracing)
			{
				enableFeatures((int)NewRelicAgent.FeatureFlag.InteractionTracing);
			}
			else
			{
				disableFeatures((int)NewRelicAgent.FeatureFlag.InteractionTracing);
			}

			if (plugin.swiftInteractionTracing)
			{
				enableFeatures((int)NewRelicAgent.FeatureFlag.SwiftInteractionTracing);
			}
			else
			{
				disableFeatures((int)NewRelicAgent.FeatureFlag.SwiftInteractionTracing);
			}

			if (plugin.URLSessionInstrumentation)
			{
				enableFeatures((int)NewRelicAgent.FeatureFlag.URLSessionInstrumentation);
			}
			else
			{
				disableFeatures((int)NewRelicAgent.FeatureFlag.URLSessionInstrumentation);
			}


			if (plugin.httpResponseBodyCapture)
			{
				enableFeatures((int)NewRelicAgent.FeatureFlag.HttpResponseBodyCapture);
			}
			else
			{
				disableFeatures((int)NewRelicAgent.FeatureFlag.HttpResponseBodyCapture);
			}

			if (plugin.experimentalNetworkingInstrumentation)
			{
				enableFeatures((int)NewRelicAgent.FeatureFlag.ExperimentalNetworkingInstrumentation);
			}
			else
			{
				disableFeatures((int)NewRelicAgent.FeatureFlag.ExperimentalNetworkingInstrumentation);
			}

			if (plugin.analyticsEvents)
			{
				enableFeatures((int)NewRelicAgent.FeatureFlag.AnalyticsEvents);
			}
			else
			{
				disableFeatures((int)NewRelicAgent.FeatureFlag.AnalyticsEvents);
			}

			if (plugin.offlineStorageEnabled)
			{
				enableFeatures((int)NewRelicAgent.FeatureFlag.OfflineStorage);
			}
			else
			{
				disableFeatures((int)NewRelicAgent.FeatureFlag.OfflineStorage);
			}

            if (plugin.newEventSystemEnabled)
			{
				enableFeatures((int)NewRelicAgent.FeatureFlag.NewEventSystem);
			}
			else
			{
				disableFeatures((int)NewRelicAgent.FeatureFlag.NewEventSystem);
			}

            if (plugin.backgroundReportingEnabled)
			{
				enableFeatures((int)NewRelicAgent.FeatureFlag.BackgroundReporting);
			}
			else
			{
				disableFeatures((int)NewRelicAgent.FeatureFlag.BackgroundReporting);
			}
		}

		public void logLevel(int logLevel)
		{
			NR_logLevel(logLevel);
		}

		override public void start(string applicationToken)
		{
			NR_setPlatform("1.4.7");
			NewRelic_startWithApplicationToken(applicationToken);
		}

		override public void crashNow(string message)
		{
			NR_crashNow(message);
		}


		// Configuration

		override public void enableFeatures(int features)
		{
			NR_enableFeatures(features);
		}

		override public void disableFeatures(int features)
		{
			NR_disableFeatures(features);
		}

		override public void enableCrashReporting(bool enabled)
		{
			NR_enableCrashReporting(enabled);
		}

		override public void setApplicationVersion(string version)
		{
			NR_setApplicationVersion(version);
		}

		override public void setApplicationBuild(string buildNumber)
		{
			NR_setApplicationBuild(buildNumber);
		}

		override public string currentSessionId()
		{
			return NR_currentSessionId();
		}

		// Custom Instrumentation

		override public string startInteractionWithName(string name)
		{
			return NR_startInteractionWithName(name);
		}

		override public void stopCurrentInteraction(string interactionIdentifier)
		{
			NR_stopCurrentInteraction(interactionIdentifier);
		}

		override public void startTracingMethod(string methodName, string objectName, Timer timer, NewRelicAgent.NRTraceType category)
		{
			NR_startTracingMethod(methodName, objectName, timer.handle, (int)category);
		}

		override public void endTracingMethodWithTimer(Timer timer)
		{
			NR_endTracingMethodWithTimer(timer.handle);
		}

		// Metrics
		private Dictionary<NewRelicAgent.MetricUnit, string> metricUnitCache = new Dictionary<NewRelicAgent.MetricUnit, string>();

		private string enumToMetricUnitString(NewRelicAgent.MetricUnit unit)
		{
			string metricUnit = null;

			if (metricUnitCache.ContainsKey(unit))
			{
				metricUnit = metricUnitCache[unit];
			}
			else
			{
				switch (unit)
				{
					case NewRelicAgent.MetricUnit.BYTES:
						metricUnit = "bytes";
						break;
					case NewRelicAgent.MetricUnit.BYTES_PER_SECOND:
						metricUnit = "bytes/second";
						break;
					case NewRelicAgent.MetricUnit.OPERATIONS:
						metricUnit = "op";
						break;
					case NewRelicAgent.MetricUnit.PERCENT:
						metricUnit = "%";
						break;
					case NewRelicAgent.MetricUnit.SECONDS:
						metricUnit = "sec";
						break;
				}
				metricUnitCache.Add(unit, metricUnit);
			}
			return metricUnit;
		}

		override public void recordMetricWithName(string name, string category)
		{
			NR_recordMetricsWithName(name, category);
		}

		override public void recordMetricWithName(string name, string category, double value)
		{
			NR_recordMetricsWithNameValue(name, category, value);
		}

		override public void recordMetricWithName(string name, string category, double value, NewRelicAgent.MetricUnit valueUnits)
		{
			NR_recordMetricsWithNameValueUnits(name, category, value, enumToMetricUnitString(valueUnits));
		}

		override public void recordMetricWithName(string name,
													string category,
													double value,
													NewRelicAgent.MetricUnit valueUnits,
													NewRelicAgent.MetricUnit countUnits)
		{
			NR_recordMetricsWithNameValueAndCountUnits(name, category, value, enumToMetricUnitString(valueUnits), enumToMetricUnitString(countUnits));
		}

		// Networking



		override public void noticeNetworkFailure(string url,
											string httpMethod,
											Timer timer,
											NewRelicAgent.NetworkFailureCode failureCode,
											string message)
		{
            NR_noticeNetworkFailureWithTimer(url,
									httpMethod,
									timer.handle,
									(int)failureCode);
		}

        override public void noticeNetworkFailure(string url,
                                    string httpMethod,
                                    long startTime,
                                    long endTime,
                                    NewRelicAgent.NetworkFailureCode failureCode,
                                    string message)
        {
            NR_noticeNetworkFailure(url,
                                    httpMethod,
                                    startTime,
									endTime,
                                    (int)failureCode);
        }


        // Insights Events


        override public void setMaxEventPoolSize(uint size)
		{
			NR_setMaxEventPoolSize(size);
		}

		override public void setMaxEventBufferTime(uint seconds)
		{
			NR_setMaxEventBufferTime(seconds);
		}

		override public void setMaxOfflineStorageSize(uint megabytes)
		{
            NR_setMaxOfflineStorageSize(megabytes);
		}

		override public bool setAttribute(string name, string value)
		{
			return NR_setAttributeStringValue(name, value);
		}

		override public bool setAttribute(string name, double value)
		{
			return NR_setAttributeDoubleValue(name, value);
		}

		override public bool incrementAttribute(string name)
		{
			return NR_incrementAttribute(name);
		}

		override public bool setUserId(string userId)
		{
			return NR_setUserId(userId);
		}

		override public bool incrementAttribute(string name, double amount)
		{
			return NR_incrementAttributeWithValue(name, amount);
		}

		override public bool removeAttribute(string name)
		{
			return NR_removeAttribute(name);
		}

		override public bool removeAllAttributes()
		{
			return NR_removeAllAttributes();
		}

		override public void recordHandleException(Exception exception)
		{
			logMessageHandler(exception.Message, exception.StackTrace, LogType.Exception);
		}
		

		override public bool recordBreadcrumb(string name, Dictionary<string, object> attributes)
		{
			System.IntPtr NSDict = CreateNSDictionaryFromDictionary(attributes);
			return NR_recordBreadcrumb(name, NSDict);
		}


		override public bool recordCustomEvent(string eventName, Dictionary<string, object> attributes)
		{
			System.IntPtr NSDict = CreateNSDictionaryFromDictionary(attributes);
			return NR_recordCustomEvent(eventName, NSDict);

		}

		override public Dictionary<string, object> noticeDistributedTrace()
		{

			System.IntPtr NSDict = NR_generateDistributedTracingHeaders();
			Dictionary<string, object> dtHeaders = new Dictionary<string, object>();
			dtHeaders.Add(NRConstants.TRACE_PARENT, NR_dictionarygetStringValueByKey(NSDict, NRConstants.TRACE_PARENT));
			dtHeaders.Add(NRConstants.TRACE_STATE, NR_dictionarygetStringValueByKey(NSDict, NRConstants.TRACE_STATE));
			dtHeaders.Add(NRConstants.NEWRELIC, NR_dictionarygetStringValueByKey(NSDict, NRConstants.NEWRELIC));

			return dtHeaders;

		}



		private static System.IntPtr CreateNSDictionaryFromDictionary(Dictionary<string, object> dict)
		{
			System.IntPtr handle = NR_dictionaryCreate();
			foreach (KeyValuePair<string, object> kvp in dict)
			{
				if (kvp.Value is string)
				{
					NR_dictionaryInsertString(handle, kvp.Key, (string)kvp.Value);
				}
				else if (kvp.Value is SByte ||
					kvp.Value is Byte ||
					kvp.Value is Int16 ||
					kvp.Value is Int32 ||
					kvp.Value is Int64 ||
					kvp.Value is long)
				{
					Int64 value64 = Convert.ToInt64(kvp.Value);
					NR_dictionaryInsertInt64(handle, kvp.Key, value64);
				}
				else if (kvp.Value is UInt16 ||
					kvp.Value is UInt32 ||
					kvp.Value is UInt64)
				{
					UInt64 uvalue64 = Convert.ToUInt64(kvp.Value);
					NR_dictionaryInsertUInt64(handle, kvp.Key, uvalue64); ;
				}
				else if (kvp.Value is float ||
					kvp.Value is Single ||
					kvp.Value is Double ||
					kvp.Value is Decimal)
				{
					double dblValue = Convert.ToDouble(kvp.Value);
					NR_dictionaryInsertDouble(handle, kvp.Key, dblValue);
				}
				else if (kvp.Value is bool)
				{
					NR_dictionaryInsertBool(handle, kvp.Key, (bool)kvp.Value);
				}
				else
				{
					UnityEngine.Debug.LogError("ERROR: New Relic dictionary bridge: object \'" +
						kvp.Value.GetType().Name +
						"\' invalid type for dictionary value. Values must be string, numeric, or boolean.");
					return System.IntPtr.Zero;
				}
			}
			return handle;

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
				var attributes = NR_dictionaryCreate();


				NR_dictionaryInsertString(attributes, "message", logString);
				NR_dictionaryInsertString(attributes, "stacktrace", stackTrace);
				NR_dictionaryInsertString(attributes, "logLevel", ConvertUnityLogTypesToNewRelicLogType(type));

				NR_logAttributes(attributes);

			}
		}

		private static void throwUnityException(string cause, string message, string stackTrace)
		{


			Regex stackFrameRegex = new Regex("(\\S+)\\s*\\(.*?\\)\\s*(?:(?:\\[.*\\]\\s*in\\s|\\(at\\s*\\s*)(.*):(\\d+))?",
												RegexOptions.IgnoreCase | RegexOptions.Multiline);

			List<StackFrame> stackFrames = new List<StackFrame>();

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
					stackFrames.Add(new StackFrame(className, methodName, fileName, Convert.ToInt32(lineNumber)));
					//unityException.Call("appendStackFrame", className, methodName, fileName, Convert.ToInt32(lineNumber));
				}
				catch (Exception e)
				{
					UnityEngine.Debug.LogError("NewReliciOS: appendStackFrame[" + e.Message + "]");
				}
			}

			var _stackFramesArray = NR_ArrayCreate();
			foreach (StackFrame length in stackFrames)
			{

				var handle = NR_dictionaryCreate();

				NR_dictionaryInsertString(handle, "file", (string)length.FileName);
				NR_dictionaryInsertString(handle, "class", (string)length.ClassName);
				NR_dictionaryInsertString(handle, "method", (string)length.MethodName);
				NR_dictionaryInsertInt64(handle, "line", length.LineNumber);

				NR_arrayInsertDictionary(_stackFramesArray, handle);
			}


			var error = NR_dictionaryCreate();


			NR_dictionaryInsertString(error, "name", message);
			NR_dictionaryInsertString(error, "reason", message);
			NR_dictionaryInsertString(error, "cause", cause);
			NR_dictionaryInsertBool(error, "fatal", false);
			NR_dictionaryInsertArray(error, "stackTraceElements", _stackFramesArray);

			NR_recordHandledExceptionWithStackTrace(error);
		}

		public override void noticeHttpTransaction(string url, string httpMethod, int statusCode, long startTime, long endTime, long bytesSent, long bytesReceived, string responseBody, Dictionary<string, object> dtHeaders)
		{
			System.IntPtr NSDict = CreateNSDictionaryFromDictionary(dtHeaders);
			NR_noticeNetworkRequest(url, httpMethod, statusCode, startTime, endTime, bytesSent, bytesReceived, responseBody, NSDict);

		}
		
		public override void LogInfo(string message)
		{
			NR_logInfo(message);

		}

		public override void LogError(string message)
		{
			NR_logError(message);
		}

		public override void LogVerbose(string message)
		{
			NR_logVerbose(message);
		}

		public override void LogWarning(string message)
		{
			NR_logWarning(message);
		}

		public override void LogDebug(string message)
		{
			NR_logDebug(message);
		}

		public override void Log(NewRelicAgent.AgentLogLevel level, string message)
		{
			String logLevel = "INFO";
            
			if(level == NewRelicAgent.AgentLogLevel.ERROR)
			{
				logLevel = "ERROR";
			}
			else if(level == NewRelicAgent.AgentLogLevel.VERBOSE)
			{
				logLevel = "VERBOSE";
			}
			else if(level == NewRelicAgent.AgentLogLevel.WARNING)
			{
				logLevel = "WARNING";
			}
			else if(level == NewRelicAgent.AgentLogLevel.DEBUG)
			{
				logLevel = "DEBUG";
			}
            
			Dictionary<string,object> attributes = new Dictionary<string, object>();
			attributes.Add("logLevel",logLevel);
			attributes.Add("message",message);
			LogAttributes(attributes);
		}

		public override void LogAttributes(Dictionary<string, object> attributes)
		{
			System.IntPtr NSDict = CreateNSDictionaryFromDictionary(attributes);
			NR_logAttributes(NSDict);
		}

		static String ConvertUnityLogTypesToNewRelicLogType(LogType type)
        {
            String logLevel = "INFO";

            if (type.Equals(LogType.Assert))
            {
                logLevel = "VERBOSE";
            }else if (type.Equals(LogType.Log))
            {
                logLevel = "INFO";
            }
            else if (type.Equals(LogType.Warning))
            {
                logLevel = "WARN";
            }
            else if (type.Equals(LogType.Error))
            {
                logLevel = "ERROR";
            }

            return logLevel;
        }
	}

	internal class StackFrame
	{
		public StackFrame(string className, string methodName, string fileName, int lineNumber)
		{
			ClassName = className;
			MethodName = methodName;
			FileName = fileName;
			LineNumber = lineNumber;
		}

		public string ClassName { get; }
		public string MethodName { get; }
		public string FileName { get; }
		public int LineNumber { get; }
	}
}
#endif
