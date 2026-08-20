using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using SlayTheSpire2.LAN.Multiplayer.Models;

namespace SlayTheSpire2.LAN.Multiplayer.Components
{
    internal partial class LanLobbyListEntry : PanelContainer
    {
        public event Action? Selected;

        public LanLobbyListEntry(LanLobbyInfo lobbyInfo)
        {
            MouseFilter = MouseFilterEnum.Stop;
            CustomMinimumSize = new Vector2(300, 36);

            var styleBox = new StyleBoxTexture
            {
                Texture = GD.Load<CompressedTexture2D>("res://images/ui/tiny_nine_patch.png"),
                TextureMarginLeft = 8, TextureMarginTop = 8, TextureMarginRight = 8, TextureMarginBottom = 8,
                ContentMarginLeft = 8, ContentMarginTop = 4, ContentMarginRight = 8, ContentMarginBottom = 4,
                ModulateColor = new Color(Colors.Black, 0.4f)
            };
            AddThemeStyleboxOverride("panel", styleBox);

            var row = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
            this.AddChildSafely(row);

            var nameLabel = new MegaLabel
                { SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
            ApplyLabelTheme(nameLabel);
            nameLabel.SetTextAutoSize($"{lobbyInfo.HostName} ({lobbyInfo.Mode})");
            row.AddChildSafely(nameLabel);

            var addressLabel = new MegaLabel { MouseFilter = MouseFilterEnum.Ignore };
            ApplyLabelTheme(addressLabel);
            addressLabel.SetTextAutoSize($"{lobbyInfo.Address}:{lobbyInfo.Port}");
            row.AddChildSafely(addressLabel);

            GuiInput += inputEvent =>
            {
                if (inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
                {
                    Selected?.Invoke();
                }
            };
        }

        /// <summary>
        /// MegaLabel._Ready asserts that a "font" theme override is present (it throws an
        /// InvalidOperationException otherwise, to work around a Godot engine bug), so every label
        /// we construct from scratch has to be styled before it enters the tree.
        /// </summary>
        private static void ApplyLabelTheme(MegaLabel label)
        {
            label.AutoSizeEnabled = false;
            label.MinFontSize = 24;

            label.AddThemeColorOverride("font_color", new Color(1.0f, 0.922f, 0.761f));
            label.AddThemeColorOverride("font_shadow_color", new Color(Colors.Black, 0.251f));

            label.AddThemeFontOverride("font", GD.Load<Font>("res://themes/kreon_bold_glyph_space_one.tres"));
            label.AddThemeFontSizeOverride("font_size", 23);
        }
    }
}
