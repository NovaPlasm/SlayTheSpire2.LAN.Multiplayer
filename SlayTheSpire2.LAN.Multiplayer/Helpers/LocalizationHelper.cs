using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;

namespace SlayTheSpire2.LAN.Multiplayer.Helpers
{
    /// <summary>
    /// Resolves this mod's own localization keys without ever throwing.
    /// <para>
    /// LocTable.GetRawText throws a LocException for unknown keys. Because our UI is built from
    /// inside Harmony prefixes on game methods (NCharacterSelectScreen._Ready,
    /// NSettingsScreen.LocalizeLabels, ...), a throw there aborts the game method itself and
    /// leaves core screens half-initialized - which then fails with a NullReferenceException the
    /// next time the game touches them. Never let a missing translation reach the game: fall back
    /// to the built-in English text instead.
    /// </para>
    /// </summary>
    internal static class LocalizationHelper
    {
        /// <summary>
        /// English text for every key this mod owns, mirroring <c>localization/eng/*.json</c>.
        /// Used when the loose JSON could not be merged (missing/unreadable localization folder).
        /// Keep in sync when adding keys.
        /// </summary>
        private static readonly Dictionary<string, string> EnglishFallbacks = new()
        {
            // main_menu_ui
            ["SlayTheSpire2.LAN.Multiplayer.COPIED"] = "Copied",
            ["SlayTheSpire2.LAN.Multiplayer.IP_ADDRESS_TITLE"] = "IP Address:",
            ["SlayTheSpire2.LAN.Multiplayer.LOCAL_IP_ADDRESS_TITLE"] = "Local IP Address:",
            ["SlayTheSpire2.LAN.Multiplayer.IPV6_ADDRESS_TITLE"] = "IPV6 Address:",

            // settings_ui
            ["SlayTheSpire2.LAN.Multiplayer.HOST_PORT"] = "Host Port",
            ["SlayTheSpire2.LAN.Multiplayer.HOST_MAX_PLAYERS"] = "Host Max Players",
            ["SlayTheSpire2.LAN.Multiplayer.PLAYER_NAME"] = "Player Name",
            ["SlayTheSpire2.LAN.Multiplayer.NET_ID"] = "NetID",

            // gameplay_ui
            ["SlayTheSpire2.LAN.Multiplayer.DISABLE_DRAWING"] = "Disable drawing"
        };

        private static readonly HashSet<string> WarnedKeys = [];

        /// <summary>
        /// Localized text for <paramref name="key"/>, or the built-in English text if the key is
        /// not present in the live LocTable.
        /// </summary>
        public static string Text(string table, string key)
        {
            try
            {
                var text = new LocString(table, key).GetFormattedText();
                if (!string.IsNullOrEmpty(text))
                    return text;
            }
            catch (Exception ex)
            {
                WarnOnce(table, key, ex.Message);
            }

            return EnglishFallbacks.TryGetValue(key, out var fallback) ? fallback : key;
        }

        /// <summary>
        /// True when <paramref name="key"/> resolves in the live LocTable. Use before handing a
        /// LocString to game code that would otherwise throw on a missing key.
        /// </summary>
        public static bool Exists(string table, string key)
        {
            try
            {
                return !string.IsNullOrEmpty(new LocString(table, key).GetFormattedText());
            }
            catch (Exception ex)
            {
                WarnOnce(table, key, ex.Message);
                return false;
            }
        }

        private static void WarnOnce(string table, string key, string message)
        {
            if (WarnedKeys.Add($"{table}/{key}"))
            {
                Log.Warn(
                    $"[LAN Multiplayer] Localization key {key} missing from table={table}, using built-in English text. " +
                    $"Is the mod's localization folder installed next to the DLL? ({message})");
            }
        }
    }
}
