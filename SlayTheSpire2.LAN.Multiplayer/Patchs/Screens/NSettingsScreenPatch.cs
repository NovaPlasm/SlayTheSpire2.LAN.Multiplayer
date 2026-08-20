using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using SlayTheSpire2.LAN.Multiplayer.Components;
using SlayTheSpire2.LAN.Multiplayer.Helpers;
using SlayTheSpire2.LAN.Multiplayer.Services;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    /// <summary>Localization keys for the LAN rows this mod adds to the general settings panel.</summary>
    internal static class LanSettingsLocKeys
    {
        internal const string HostPort = "SlayTheSpire2.LAN.Multiplayer.HOST_PORT";
        internal const string HostMaxPlayers = "SlayTheSpire2.LAN.Multiplayer.HOST_MAX_PLAYERS";
        internal const string PlayerName = "SlayTheSpire2.LAN.Multiplayer.PLAYER_NAME";
        internal const string NetId = "SlayTheSpire2.LAN.Multiplayer.NET_ID";
    }

    [HarmonyPatch(typeof(NSettingsScreen), "_Ready")]
    internal class NSettingsScreenReadyPatch
    {
        private const string HostPortKey = LanSettingsLocKeys.HostPort;
        private const string HostMaxPlayersKey = LanSettingsLocKeys.HostMaxPlayers;
        private const string PlayerNameKey = LanSettingsLocKeys.PlayerName;
        private const string NetIdKey = LanSettingsLocKeys.NetId;

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(NSettingsPanel), "RefreshSize")]
        private static void RefreshSize(NSettingsPanel instance)
        {
            throw new NotImplementedException();
        }

        private static void Prefix(NSettingsScreen __instance)
        {
            // Guarded: a throw here would abort NSettingsScreen._Ready and break the whole
            // settings screen, not just our four extra rows.
            PatchGuard.Run(nameof(NSettingsScreenReadyPatch), () => AddLanSettings(__instance));
        }

        private static void AddLanSettings(NSettingsScreen __instance)
        {
            var moddingNode = __instance.GetNode("%Modding");

            var vBoxContainerNode = moddingNode.GetParent();

            var hostPortDivider = (ColorRect)__instance.GetNode("%ModdingDivider").Duplicate();
            hostPortDivider.Name = "HostPortDivider";
            hostPortDivider.Visible = true;
            vBoxContainerNode.AddChildSafely(hostPortDivider);
            vBoxContainerNode.MoveChild(hostPortDivider, moddingNode.GetIndex() + 1);

            var hostPort = (MarginContainer)moddingNode.Duplicate();
            hostPort.Name = "HostPort";
            hostPort.RemoveChildSafely(hostPort.GetNode("ModdingButton"));
            hostPort.Visible = true;

            var hostPortLineEdit = new SpinBox
            {
                Name = "HostPortInput", CustomMinimumSize = new Vector2(324, 64),
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd, Step = 1, MinValue = 0, MaxValue = 65535
            };

            hostPortLineEdit.GetLineEdit().Alignment = HorizontalAlignment.Center;
            hostPort.AddChildSafely(hostPortLineEdit);

            hostPortLineEdit.Value = SettingsService.Instance.SettingsModel.HostPort;
            hostPortLineEdit.ValueChanged += value =>
            {
                SettingsService.Instance.SettingsModel.HostPort = (ushort)value;
                SettingsService.Instance.WriteSettings();
            };

            vBoxContainerNode.AddChildSafely(hostPort);
            vBoxContainerNode.MoveChild(hostPort, hostPortDivider.GetIndex() + 1);

            var hostPortLabel = (MegaRichTextLabel)hostPort.GetNode("Label");
            hostPortLabel.SetTextAutoSize(LocalizationHelper.Text("settings_ui", HostPortKey));

            var hostMaxPlayersDivider = (ColorRect)__instance.GetNode("%ModdingDivider").Duplicate();
            hostMaxPlayersDivider.Name = "HostMaxPlayersDivider";
            hostMaxPlayersDivider.Visible = true;
            vBoxContainerNode.AddChildSafely(hostMaxPlayersDivider);
            vBoxContainerNode.MoveChild(hostMaxPlayersDivider, moddingNode.GetIndex() + 1);

            var hostMaxPlayers = (MarginContainer)moddingNode.Duplicate();
            hostMaxPlayers.Name = "HostMaxPlayers";
            hostMaxPlayers.RemoveChildSafely(hostMaxPlayers.GetNode("ModdingButton"));
            hostMaxPlayers.Visible = true;

            var hostMaxPlayersInput = new SpinBox
            {
                Name = "HostMaxPlayersInput", CustomMinimumSize = new Vector2(324, 64),
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd, Step = 1, MinValue = 2,
            };

            hostMaxPlayersInput.GetLineEdit().Alignment = HorizontalAlignment.Center;
            hostMaxPlayers.AddChildSafely(hostMaxPlayersInput);

            hostMaxPlayersInput.Value = SettingsService.Instance.SettingsModel.HostMaxPlayers;
            hostMaxPlayersInput.ValueChanged += value =>
            {
                SettingsService.Instance.SettingsModel.HostMaxPlayers = (int)value;
                SettingsService.Instance.WriteSettings();
            };

            vBoxContainerNode.AddChildSafely(hostMaxPlayers);
            vBoxContainerNode.MoveChild(hostMaxPlayers, hostMaxPlayersDivider.GetIndex() + 1);

            var hostMaxPlayersLabel = (MegaRichTextLabel)hostMaxPlayers.GetNode("Label");
            hostMaxPlayersLabel.SetTextAutoSize(LocalizationHelper.Text("settings_ui", HostMaxPlayersKey));

            var playerNameDivider = (ColorRect)__instance.GetNode("%ModdingDivider").Duplicate();
            playerNameDivider.Name = "PlayerNameDivider";
            playerNameDivider.Visible = true;
            vBoxContainerNode.AddChildSafely(playerNameDivider);
            vBoxContainerNode.MoveChild(playerNameDivider, moddingNode.GetIndex() + 1);

            var playerName = (MarginContainer)moddingNode.Duplicate();
            playerName.Name = "PlayerName";
            playerName.RemoveChildSafely(playerName.GetNode("ModdingButton"));
            playerName.Visible = true;

            var marginContainer = new MarginContainer();
            playerName.AddChildSafely(marginContainer);

            marginContainer.AddThemeConstantOverride("margin_right", 18);

            var playerNameInput = new PlayerNameLineEdit
            {
                Name = "PlayerNameInput", CustomMinimumSize = new Vector2(308, 64),
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd, Alignment = HorizontalAlignment.Center,
                MaxLength = 16
            };

            marginContainer.AddChildSafely(playerNameInput);

            playerNameInput.Text = SettingsService.Instance.SettingsModel.PlayerName;
            playerNameInput.TextChanged += value =>
            {
                if (playerNameInput.IsEmpty || !playerNameInput.IsInvalid)
                {
                    SettingsService.Instance.SettingsModel.PlayerName = value;
                    LanPlayerNameService.Instance.SetHostPlayerName();
                    SettingsService.Instance.WriteSettings();
                }
            };

            vBoxContainerNode.AddChildSafely(playerName);
            vBoxContainerNode.MoveChild(playerName, playerNameDivider.GetIndex() + 1);

            var playerNameLabel = (MegaRichTextLabel)playerName.GetNode("Label");
            playerNameLabel.SetTextAutoSize(LocalizationHelper.Text("settings_ui", PlayerNameKey));

            var netIdDivider = (ColorRect)__instance.GetNode("%ModdingDivider").Duplicate();
            netIdDivider.Name = "NetIDDivider";
            netIdDivider.Visible = true;
            vBoxContainerNode.AddChildSafely(netIdDivider);
            vBoxContainerNode.MoveChild(netIdDivider, moddingNode.GetIndex() + 1);

            var netId = (MarginContainer)moddingNode.Duplicate();
            netId.Name = "NetID";
            netId.RemoveChildSafely(netId.GetNode("ModdingButton"));
            netId.Visible = true;

            var netIdInput = new SpinBox
            {
                Name = "NetIDInput", CustomMinimumSize = new Vector2(324, 64),
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd, Step = 1, MinValue = 2, MaxValue = ulong.MaxValue
            };

            netIdInput.GetLineEdit().Alignment = HorizontalAlignment.Center;
            netId.AddChildSafely(netIdInput);

            netIdInput.Value = SettingsService.Instance.SettingsModel.NetId;
            netIdInput.ValueChanged += value =>
            {
                SettingsService.Instance.SettingsModel.NetId = (ulong)value;
                SettingsService.Instance.WriteSettings();
            };

            var netIdLabel = (MegaRichTextLabel)netId.GetNode("Label");
            netIdLabel.SetTextAutoSize(LocalizationHelper.Text("settings_ui", NetIdKey));

            vBoxContainerNode.AddChildSafely(netId);
            vBoxContainerNode.MoveChild(netId, netIdDivider.GetIndex() + 1);

            var generalSettings = (NSettingsPanel)vBoxContainerNode.GetParent();
            RefreshSize(generalSettings);
        }
    }

    [HarmonyPatch(typeof(NSettingsScreen), "LocalizeLabels")]
    internal class NSettingsScreenLocalizeLabelsPatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(NSettingsScreen), "LocHelper")]
        private static void LocHelper(Node settingsLineNode, LocString locString)
        {
            throw new NotImplementedException();
        }

        private static void Prefix(NSettingsScreen __instance)
        {
            // Guarded: a throw here would abort NSettingsScreen.LocalizeLabels, which the game
            // calls from _Ready and on every language change.
            PatchGuard.Run(nameof(NSettingsScreenLocalizeLabelsPatch), () =>
            {
                var content = __instance.GetNode<NSettingsPanel>("%GeneralSettings").Content;

                Localize(content, "HostPort", LanSettingsLocKeys.HostPort);
                Localize(content, "HostMaxPlayers", LanSettingsLocKeys.HostMaxPlayers);
                Localize(content, "PlayerName", LanSettingsLocKeys.PlayerName);
                Localize(content, "NetID", LanSettingsLocKeys.NetId);
            });
        }

        private static void Localize(Node content, string nodeName, string key)
        {
            var settingsLineNode = content.GetNodeOrNull(nodeName);
            if (settingsLineNode == null)
                return;

            // LocHelper resolves the LocString itself and throws on an unknown key. When our
            // localization files were not merged, leave the text set in NSettingsScreenReadyPatch
            // (which already falls back to built-in English) instead.
            if (!LocalizationHelper.Exists("settings_ui", key))
                return;

            LocHelper(settingsLineNode, new LocString("settings_ui", key));
        }
    }
}