using BroomService.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;

namespace BroomService.Helpers
{
    public class LanguageMang
    {
        public static List<Languages> AvailableLanguages = new List<Languages>
        {
            new Languages {
                LanguageFullName = Resource.english, LanguageCultureName = "en"
            },
            new Languages {
                LanguageFullName = Resource.french, LanguageCultureName = "fr-FR"
            },
            new Languages {
                LanguageFullName = Resource.russian, LanguageCultureName = "ru-RU"
            },
            new Languages {
                LanguageFullName = Resource.hebrew, LanguageCultureName = "he-IL"
            }
        };

        public static bool IsLanguageAvailable(string lang)
        {
            return AvailableLanguages.Where(a => a.LanguageCultureName.Equals(lang)).FirstOrDefault() != null ? true : false;
        }
        public static string GetDefaultLanguage()
        {
            return AvailableLanguages[0].LanguageCultureName;
        }
        public void SetLanguage(string lang)
        {
            try
            {
                if (!IsLanguageAvailable(lang)) lang = GetDefaultLanguage();
                var cultureInfo = new CultureInfo(lang);
                var netLocale = cultureInfo.ToString().Replace("_", "-");
                var ci = new System.Globalization.CultureInfo(netLocale);
                Thread.CurrentThread.CurrentUICulture = ci;
                Thread.CurrentThread.CurrentCulture = ci;
                HttpCookie langCookie = new HttpCookie("culture", lang);
                langCookie.Expires = DateTime.Now.AddYears(1);
                HttpContext.Current.Response.Cookies.Add(langCookie);
             }
            catch (Exception) { }
        }
    }

    public class Languages
    {
        public string LanguageFullName
        {
            get;
            set;
        }
        public string LanguageCultureName
        {
            get;
            set;
        }
    }
}