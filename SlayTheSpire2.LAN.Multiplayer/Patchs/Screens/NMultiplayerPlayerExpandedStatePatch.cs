using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using SlayTheSpire2.LAN.Multiplayer.Helpers;
using SlayTheSpire2.LAN.Multiplayer.Services;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    [HarmonyPatch(typeof(NMultiplayerPlayerExpandedState), "_Ready")]
    internal class NMultiplayerPlayerExpandedStateReadyPatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(NMapDrawings), "GetDrawingStateForPlayer")]
        private static object GetDrawingStateForPlayer(NMapDrawings instance, ulong playerId)
        {
            throw new NotImplementedException();
        }

        private static void Prefix(NMultiplayerPlayerExpandedState __instance, Player ____player)
        {
            // Guarded: a throw here would abort NMultiplayerPlayerExpandedState._Ready and break
            // the in-run player panel.
            PatchGuard.Run(nameof(NMultiplayerPlayerExpandedStateReadyPatch),
                () => AddDisableDrawingTickbox(__instance, ____player));
        }

        private static void AddDisableDrawingTickbox(NMultiplayerPlayerExpandedState __instance, Player player)
        {
            if (player.NetId != RunManager.Instance.NetService.NetId)
            {
                var container = __instance.GetNode("ScreenContents/Container");

                var disableDrawing = PreloadManager.Cache
                    .GetScene(SceneHelper.GetScenePath("screens/card_library/card_library_tickbox"))
                    .Instantiate<NLibraryStatTickbox>();

                disableDrawing.Name = "DisableDrawing";
                disableDrawing.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

                var marginContainer = (MarginContainer)container.GetNode("MarginContainer");
                marginContainer.RemoveThemeConstantOverride("margin_top");

                container.AddChildSafely(disableDrawing);
                container.MoveChild(disableDrawing, 0);

                disableDrawing.SetLabel(LocalizationHelper.Text("gameplay_ui",
                    "SlayTheSpire2.LAN.Multiplayer.DISABLE_DRAWING"));

                var lanMapDrawingsService = LanMapDrawingsService.Instance;

                disableDrawing.IsTicked =
                    lanMapDrawingsService.DisableDrawingHashSet.Contains(player.NetId);

                disableDrawing.Toggled += tickBox =>
                {
                    if (NMapScreen.Instance == null)
                        return;

                    var drawingState = GetDrawingStateForPlayer(NMapScreen.Instance.Drawings, player.NetId);

                    var drawViewport = Traverse.Create(drawingState).Field("drawViewport").GetValue<SubViewport>();

                    if (drawViewport != null)
                    {
                        if (tickBox.IsTicked)
                        {
                            foreach (var line2D in drawViewport.GetChildren().OfType<Line2D>())
                            {
                                line2D.Visible = false;
                            }

                            lanMapDrawingsService.DisableDrawingHashSet.Add(player.NetId);
                        }
                        else
                        {
                            foreach (var line2D in drawViewport.GetChildren().OfType<Line2D>())
                            {
                                line2D.Visible = true;
                            }

                            lanMapDrawingsService.DisableDrawingHashSet.Remove(player.NetId);
                        }
                    }
                };
            }
        }
    }
}