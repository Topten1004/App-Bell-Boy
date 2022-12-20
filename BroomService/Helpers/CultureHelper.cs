using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BroomService.Helpers
{
    public class LanguageMessageHandler : DelegatingHandler
    {
        private const string LangenUS = "en-US";
        private const string LangfrFR = "fr-FR";
        private const string LangheIL = "he-IL";
        private const string LangruRU = "ru-RU";
        private readonly List<string> _supportedLanguages = new List<string> { LangenUS, LangfrFR, LangheIL, LangruRU };

        private bool SetHeaderIfAcceptLanguageMatchesSupportedLanguage(HttpRequestMessage request)
        {
            foreach (var lang in request.Headers.AcceptLanguage)
            {
                if (_supportedLanguages.Contains(lang.Value))
                {
                    SetCulture(request, lang.Value);
                    return true;
                }
            }

            return false;
        }

        private bool SetHeaderIfGlobalAcceptLanguageMatchesSupportedLanguage(HttpRequestMessage request)
        {
            foreach (var lang in request.Headers.AcceptLanguage)
            {
                var globalLang = lang.Value.Substring(0, 2);
                if (_supportedLanguages.Any(t => t.StartsWith(globalLang)))
                {
                    SetCulture(request, _supportedLanguages.FirstOrDefault(i => i.StartsWith(globalLang)));
                    return true;
                }
            }

            return false;
        }

        private void SetCulture(HttpRequestMessage request, string lang)
        {
            request.Headers.AcceptLanguage.Clear();
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(lang));
            Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!SetHeaderIfAcceptLanguageMatchesSupportedLanguage(request))
            {
                // Whoops no localization found. Lets try Globalisation
                if (!SetHeaderIfGlobalAcceptLanguageMatchesSupportedLanguage(request))
                {
                    // no global or localization found
                    SetCulture(request, LangenUS);
                }
            }

            var response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }
}