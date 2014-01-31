using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using HtmlAgilityPack;

namespace Forever.WebCrawler
{
    public class WebDocument
    {
        public Uri RequestUrl { get; protected set; }
        protected HtmlDocument HtmlDocument { get; set; }
        private bool _loaded = false;

        public WebDocument(Uri requestUrl)
        {
            RequestUrl = requestUrl;
            HtmlDocument = new HtmlDocument();
        }


        public void Request()
        {
            if (!_loaded)
            {
                var request = WebRequest.Create(RequestUrl.AbsoluteUri);
                var stream = request.GetResponse().GetResponseStream();
                HtmlDocument.Load(stream);
                _loaded = true;
            }
            else
            {
                throw new InvalidOperationException("This instance has already been loaded.");
            }
        }


        public IEnumerable<Uri> ExtractLinks()
        {
            var doc = this.HtmlDocument;
            var uris = new List<Uri>();

            foreach (var link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                var uri = ExtractUri(link.Attributes["href"].Value);
                if (uri != null)
                {
                    uris.Add(uri);
                }

            }
            return uris;
        }

        private Uri ExtractUri(string href)
        {
            if(Uri.IsWellFormedUriString(href, UriKind.Absolute))
            {
                return new Uri(href);
            }
            else 
            {
                var pageUrl = RequestUrl.Host;
                var newUrl = string.Format("{0}://{1}{2}", RequestUrl.Scheme, pageUrl, href);
                if (Uri.IsWellFormedUriString(newUrl, UriKind.Absolute))
                {
                    return new Uri(newUrl);
                }
            }
            return null;
        }

        public IEnumerable<string> ExtractBlocksOfText()
        {
            var texts = new List<string>();
            foreach(var span in HtmlDocument.DocumentNode.SelectNodes("//span[text()]"))
            {
                texts.Add(span.InnerText);
            }
            return texts;
        }
    }
}
