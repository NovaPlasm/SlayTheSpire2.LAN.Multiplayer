using MegaCrit.Sts2.Core.Logging;

namespace SlayTheSpire2.LAN.Multiplayer.Helpers
{
    /// <summary>
    /// Runs mod code that is invoked from inside a Harmony patch, containing any exception.
    /// <para>
    /// An exception thrown from a prefix/postfix propagates out of the patched game method, so a
    /// mod-side failure while injecting UI aborts things like NCharacterSelectScreen._Ready. The
    /// screen is then left half-initialized and the game crashes with a NullReferenceException
    /// later - even on vanilla code paths that have nothing to do with this mod. Losing our UI is
    /// always better than breaking the game, so log and carry on.
    /// </para>
    /// </summary>
    internal static class PatchGuard
    {
        public static void Run(string context, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Log.Error($"[LAN Multiplayer] {context} failed and was skipped to keep the game running: {ex}");
            }
        }

        /// <summary>
        /// As <see cref="Run(string,Action)"/>, returning <paramref name="fallback"/> on failure.
        /// Use for prefixes whose return value decides whether the original method runs.
        /// </summary>
        public static T Run<T>(string context, Func<T> action, T fallback)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                Log.Error($"[LAN Multiplayer] {context} failed and was skipped to keep the game running: {ex}");
                return fallback;
            }
        }
    }
}
