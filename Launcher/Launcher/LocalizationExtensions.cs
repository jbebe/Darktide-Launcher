using System.Collections.Generic;
using static ResourceDictionary.LocalizationManager;

namespace Launcher
{
    public class LocalizationExtensions
    {
        private const Language_id DefaultLanguageId = Language_id.english;

        private Language_id Id { get; }

        public LocalizationExtensions(Language_id id)
        {
            Id = id;
        }

        public string Autorun => GetLocalization(new Dictionary<Language_id, string>
        {
            [Language_id.english] = "Launch game automatically",
            [Language_id.french] = "Launch game automatically",
            [Language_id.german] = "Launch game automatically",
            [Language_id.spanish] = "Launch game automatically",
            [Language_id.russian] = "Launch game automatically",
            [Language_id.portuguese_brazil] = "Launch game automatically",
            [Language_id.italian] = "Launch game automatically",
            [Language_id.polish] = "Launch game automatically",
            [Language_id.korean] = "Launch game automatically",
            [Language_id.japanese] = "Launch game automatically",
            [Language_id.chinese_simplified] = "Launch game automatically",
            [Language_id.chinese_traditional] = "Launch game automatically",
        });

        public string AutorunTooltip => Autorun;

        private string GetLocalization(Dictionary<Language_id, string> translationMap) =>
            translationMap.TryGetValue(Id, out var translation) ? translation : translationMap[DefaultLanguageId];
    }
}
