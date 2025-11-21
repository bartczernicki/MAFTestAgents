using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Pipeline;

namespace MAFTestAgents
{
    public sealed class ApiVersionOverridePolicy : HttpPipelinePolicy
    {
        private readonly string _apiVersion;

        public ApiVersionOverridePolicy(string apiVersion) => _apiVersion = apiVersion;

        public override void Process(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
        {
            OverrideApiVersion(message);
            ProcessNext(message, pipeline);
        }

        public override ValueTask ProcessAsync(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
        {
            OverrideApiVersion(message);
            return ProcessNextAsync(message, pipeline);
        }

        private void OverrideApiVersion(HttpMessage message)
        {
            var uri = message.Request.Uri; // RequestUriBuilder
                                           // Uri.Query is a raw string without leading '?'
            var existing = uri.Query;
            var parts = new List<(string k, string v)>();

            if (!string.IsNullOrEmpty(existing))
            {
                foreach (var pair in existing.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var i = pair.IndexOf('=');
                    var k = i >= 0 ? Uri.UnescapeDataString(pair[..i]) : Uri.UnescapeDataString(pair);
                    var v = i >= 0 ? Uri.UnescapeDataString(pair[(i + 1)..]) : "";
                    // drop any existing api-version
                    if (!k.Equals("api-version", StringComparison.OrdinalIgnoreCase))
                        parts.Add((k, v));
                }
            }

            // add/replace with our desired value
            parts.Add(("api-version", _apiVersion));

            var sb = new StringBuilder();
            for (int i = 0; i < parts.Count; i++)
            {
                if (i > 0) sb.Append('&');
                sb.Append(Uri.EscapeDataString(parts[i].k))
                  .Append('=')
                  .Append(Uri.EscapeDataString(parts[i].v));
            }
            uri.Query = sb.ToString();
        }
    }
}
