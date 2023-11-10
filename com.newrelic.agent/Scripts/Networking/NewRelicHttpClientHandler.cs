using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NewRelic.Utilities;

namespace NewRelic.Networking
{

    public class NewRelicHttpClientHandler : HttpClientHandler
    {
        private NewRelic agentInstance;

        public NewRelicHttpClientHandler()
        {
        }

        public NewRelicHttpClientHandler(NewRelic agentInstance)
        {
            this.agentInstance = agentInstance;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            HttpResponseMessage httpResponseMessage;


            Dictionary<string, object> dtHeaders = agentInstance.noticeDistributedTrace();

            Dictionary<string, object> traceAttributes = new Dictionary<string, object>();

            foreach (var header in dtHeaders)
            {
                if (header.Key.Equals(NRConstants.TRACE_PARENT) || header.Key.Equals(NRConstants.TRACE_STATE) || header.Key.Equals(NRConstants.NEWRELIC))
                {
                    request.Headers.Add(header.Key, header.Value.ToString());
                    traceAttributes.Add(header.Key, header.Value.ToString());
                }
#if UNITY_ANDROID

                    if (header.Key.Equals("trace.id") || header.Key.Equals("guid") || header.Key.Equals("id"))
                    {
                        traceAttributes.Add(header.Key, header.Value.ToString());
                    }
#endif

            }
            Console.WriteLine("--------- Added Headers -------");
            httpResponseMessage = await base.SendAsync(request, cancellationToken);
            Console.WriteLine("--------- Base send async successful");
            var endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            NewRelicAgent.NoticeHttpTransaction(request.RequestUri.ToString(), request.Method.ToString(), ((int)httpResponseMessage.StatusCode), startTime, endTime, 0, httpResponseMessage.ToString().Length, httpResponseMessage.ToString(), traceAttributes);
            return httpResponseMessage;
        }
    }
}
