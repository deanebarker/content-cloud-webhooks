﻿using DeaneBarker.Optimizely.Webhooks.Helpers;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Validation;
using EPiServer.Web;
using System.Net;
using System.Text.Json;
using ILogger = EPiServer.Logging.ILogger;

namespace DeaneBarker.Optimizely.Webhooks.Serializers
{
    public class PostJsonWebhookSerializer : IWebhookSerializer
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(PostXmlWebhookSerializer));

        public object SerializationConfig { get; set; }

        public HttpWebRequest Serialize(Webhook webhook)
        {           
            var requestBody = SerializeIContentAsJson(webhook.Content);
            logger.Debug($"Serialized content {webhook.Content.ContentLink} into {requestBody.Length} byte(s). ID: {webhook.Id}");

            var request = new WebRequestBuilder()
                .AsPost()
                .ToUrl(webhook.Target)
                .WithBody(requestBody)
                .WithQuerystringArg("action", webhook.Action)
                .WithContentType("application/json");

            return request.Build();
        }

        // I broke this out to its own method so that if someone inherits this class and overrides Serialize, they don't have to figure out this code
        protected string SerializeIContentAsJson(IContent content)
        {
            var contentConvertingService = ServiceLocator.Current.GetInstance<ContentConvertingService>();

            var converterContext = new ConverterContext(
                   contentReference: ((IContent)content).ContentLink,
                   language: System.Globalization.CultureInfo.CurrentCulture,
                   contentApiOptions: new ContentApiOptions("", false, false, ""),
                   contextMode: ContextMode.Default,
            select: "",
            expand: "",
            excludePersonalizedContent: false
                   );
            var obj = contentConvertingService.ConvertToContentApiModel((IContent)content, converterContext);

            var serializer = new Newtonsoft.Json.JsonSerializer();
            var sw = new StringWriter();
            serializer.Serialize(sw, obj);
            return sw.ToString();
        }

        public IEnumerable<ValidationError> ValidateConfig(string config)
        {
            throw new NotImplementedException();
        }
    }
}