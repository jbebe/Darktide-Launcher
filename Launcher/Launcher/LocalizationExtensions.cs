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
            [Language_id.french] = "Lancer le jeu automatiquement",
            [Language_id.german] = "Spiel automatisch starten",
            [Language_id.spanish] = "Iniciar el juego automáticamente",
            [Language_id.russian] = "Запустить игру автоматически",
            [Language_id.portuguese_brazil] = "Iniciar jogo automaticamente",
            [Language_id.italian] = "Avvia il gioco automaticamente",
            [Language_id.polish] = "Uruchom grę automatycznie",
            [Language_id.korean] = "자동으로 게임 시작",
            [Language_id.japanese] = "ゲームを自動的に起動する",
            [Language_id.chinese_simplified] = "自动启动游戏",
            [Language_id.chinese_traditional] = "自动启动游戏",
        });

        public string AutorunTooltip => GetLocalization(new Dictionary<Language_id, string>
        {
            [Language_id.english] = "If set, the game is automatically launched after a 3 seconds delay",
            [Language_id.french] = "Si défini, le jeu est lancé automatiquement après un délai de 3 secondes",
            [Language_id.german] = "Wenn eingestellt, wird das Spiel automatisch nach einer Verzögerung von 3 Sekunden gestartet",
            [Language_id.spanish] = "Si se configura, el juego se inicia automáticamente después de un retraso de 3 segundos.",
            [Language_id.russian] = "Если установлено, игра автоматически запустится с задержкой в ​​3 секунды.",
            [Language_id.portuguese_brazil] = "Se definido, o jogo será iniciado automaticamente após um atraso de 3 segundos",
            [Language_id.italian] = "Se impostato, il gioco viene avviato automaticamente dopo un ritardo di 3 secondi",
            [Language_id.polish] = "Jeśli ustawione, gra zostanie uruchomiona automatycznie po 3 sekundach opóźnienia",
            [Language_id.korean] = "설정하면 게임은 3초 지연 후 자동으로 시작됩니다.",
            [Language_id.japanese] = "設定すると、3秒後にゲームが自動的に起動します。",
            [Language_id.chinese_simplified] = "如果设置，游戏将在 3 秒延迟后自动启动",
            [Language_id.chinese_traditional] = "如果设置，游戏将在 3 秒延迟后自动启动",
        });

        private string GetLocalization(Dictionary<Language_id, string> translationMap) =>
            translationMap.TryGetValue(Id, out var translation) ? translation : translationMap[DefaultLanguageId];
    }
}
