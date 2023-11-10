using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NewRelic.Utilities;

namespace NewRelic.Networking
{

    public class NRWrappedUnityWebRequest
    {

        private static ConcurrentDictionary<UnityWebRequest, RequestInfo> onGoingrequests = new ConcurrentDictionary<UnityWebRequest, RequestInfo>();

        public static UnityWebRequestAsyncOperation SendWebRequest(UnityWebRequest unityWebRequest)
        {
            Action<AsyncOperation> completionCallback = NoOpCompletionCallback;

            try
            {
                completionCallback = ExecuteBeforeMethod(unityWebRequest);
            }
            catch (Exception e)
            {
                NewRelicAgent.RecordException(e);
            }

            // Invoke the real method
            var asyncOperation = unityWebRequest.SendWebRequest();

            try
            {
                ExecuteAfterMethod(asyncOperation, completionCallback);
            }
            catch (Exception e)
            {
                NewRelicAgent.RecordException(e);
            }

            // Return the async operation without yielding it so that the behavior of the real caller
            // does not change.
            return asyncOperation;
        }

        public static void DisposeWebRequest(IDisposable reuqest)
        {

            if (reuqest is UnityWebRequest unityWebRequest)
            {
                if (onGoingrequests.TryRemove(unityWebRequest, out var requestInfo))
                {
                    TrackRequestWhenCompleted(requestInfo);
                }
            }


            reuqest.Dispose();
        }

        private static Action<AsyncOperation> ExecuteBeforeMethod(UnityWebRequest unityWebRequest)
        {

#if UNITY_ANDROID
        Dictionary<string, object> dtHeaders = NewRelicAgent.NoticeDistributedTrace();
        var requestInfo = new RequestInfo
        {
            Uri = unityWebRequest.url,
            Method = unityWebRequest.method,
            StartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            Request = unityWebRequest
        };

        foreach (var header in dtHeaders)
        {
            if (header.Key.Equals(NRConstants.TRACE_PARENT) || header.Key.Equals(NRConstants.TRACE_STATE) || header.Key.Equals(NRConstants.NEWRELIC))
            {
                unityWebRequest.SetRequestHeader(header.Key, header.Value.ToString());
                requestInfo.TraceAttributes.Add(header.Key, header.Value.ToString());
            }

            
            if (header.Key.Equals("trace.id") || header.Key.Equals("guid") || header.Key.Equals("id"))
            {
                requestInfo.TraceAttributes.Add(header.Key, header.Value.ToString());
            }
            
        }

        onGoingrequests.TryAdd(unityWebRequest, requestInfo);
#endif

            return CompletionCallback;


        }

        private static void CompletionCallback(AsyncOperation operation)
        {
            if (operation is UnityWebRequestAsyncOperation asyncOperation)
                if (onGoingrequests.TryRemove(asyncOperation.webRequest, out var info))
                    TrackRequestWhenCompleted(info);
        }

        private static void ExecuteAfterMethod(UnityWebRequestAsyncOperation asyncOperation, Action<AsyncOperation> completionCallback)
        {
            if (asyncOperation.isDone)
            {
                completionCallback(asyncOperation);
            }
            else
            {
                asyncOperation.completed += completionCallback;
            }
        }

        private static void TrackRequestWhenCompleted(RequestInfo requestInfo)
        {
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var request = requestInfo.Request;

#if UNITY_ANDROID
        NewRelicAgent.NoticeHttpTransaction(requestInfo.Uri, requestInfo.Method.ToUpper(), Convert.ToInt32(request.responseCode), requestInfo.StartTime, endTime, (int)request.uploadedBytes, (int)request.downloadedBytes, request.downloadHandler.text, requestInfo.TraceAttributes);
#endif
        }

        private static void NoOpCompletionCallback(AsyncOperation _)
        {
        }

        private class RequestInfo
        {
            public string Uri;
            public string Method;
            public long StartTime;
            public Dictionary<string, object> TraceAttributes = new Dictionary<string, object>();
            public UnityWebRequest Request;
        }
    }

}