using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace Localization.SqlLocalizer.DbStringLocalizer
{
    public class SqlStringLocalizer : IStringLocalizer
    {
        private const string DEFAULT_CULTURE = "en-US";

        private readonly Dictionary<string, string> _localizations;

        private readonly DevelopmentSetup _developmentSetup;
        private readonly string _resourceKey;
        private bool _returnKeyOnlyIfNotFound;
        private bool _createNewRecordWhenLocalisedStringDoesNotExist;

        public SqlStringLocalizer(Dictionary<string, string> localizations, DevelopmentSetup developmentSetup, string resourceKey, bool returnKeyOnlyIfNotFound, bool createNewRecordWhenLocalisedStringDoesNotExist)
        {
            _localizations = localizations;
            _developmentSetup = developmentSetup;
            _resourceKey = resourceKey;
            _returnKeyOnlyIfNotFound = returnKeyOnlyIfNotFound;
            _createNewRecordWhenLocalisedStringDoesNotExist = createNewRecordWhenLocalisedStringDoesNotExist;
        }
        public LocalizedString this[string name]
        {
            get
            {
                bool notSucceed;
                var text = GetText(name, out notSucceed);

                // If value is null, then get value from fallbackCulture
                var fallbackCulture = new CultureInfo(DEFAULT_CULTURE);
                if (fallbackCulture != null && text == null && !notSucceed)
                {
                    text = GetText(name, out notSucceed, fallbackCulture);
                }

                // Temporary fix for incomplete translations (removing the culture suffix).
                string[] cultureSuffixes = { "en-US", "fr-FR", "de-DE", "it-IT", "es-ES", "tr-TR" };
                if (text != null && cultureSuffixes.Any(x => text.EndsWith($".{x}")))
                {
                    text = text.Substring(0, text.Length - ".en-US".Length);
                }

                return new LocalizedString(name, text, notSucceed);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var localizedString = this[name];
                return new LocalizedString(name, String.Format(localizedString.Value, arguments), localizedString.ResourceNotFound);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetText(string key, out bool notSucceed, CultureInfo culture = null)
        {

#if NET451
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
#elif NET46
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
#else
            if (culture is null)
            {
                culture = CultureInfo.CurrentCulture;
            }
#endif

            string computedKey = $"{key}.{culture}";
            string parentComputedKey = $"{key}.{culture.Parent.TwoLetterISOLanguageName}";

            string result;
            if (_localizations.TryGetValue(computedKey, out result) || _localizations.TryGetValue(parentComputedKey, out result))
            {
                notSucceed = false;
                return result;
            }
            else
            {
                notSucceed = true;
                // Note: The additional check for the culture name, should prevent our database
                // to have too many irrelevant localization entries.
                if (_createNewRecordWhenLocalisedStringDoesNotExist
                    && string.Equals(culture.Name, DEFAULT_CULTURE, StringComparison.OrdinalIgnoreCase))
                {
                    _developmentSetup.AddNewLocalizedItem(key, culture.ToString(), _resourceKey);
                    _localizations.Add(computedKey, computedKey);
                    return computedKey;
                }
                if (_returnKeyOnlyIfNotFound)
                {
                    return key;
                }

                return _resourceKey + "." + computedKey;
            }
        }
    }
}
