using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using TMPro;

namespace ColorblindText
{
    [BepInPlugin("com.colorblindtext.mod", "Colorblind Text Mod", "1.0.0")]
    public class ColorblindPlugin : BasePlugin
    {
        public static ConfigEntry<bool> ColorblindEnabled;

        public override void Load()
        {
            ColorblindEnabled = Config.Bind("General", "ColorblindTextEnabled", true, "Enable colorblind text identifiers on players and in meetings.");

            var harmony = new Harmony("com.colorblindtext.mod");
            harmony.PatchAll();
        }

        public static string GetColorName(int colorId)
        {
            return colorId switch
            {
                0 => "<color=#C51111>Red</color>",
                1 => "<color=#132ED1>Blue</color>",
                2 => "<color=#117F2D>Green</color>",
                3 => "<color=#ED54BA>Pink</color>",
                4 => "<color=#EF7D0E>Orange</color>",
                5 => "<color=#F5F525>Yellow</color>",
                6 => "<color=#3F474E>Black</color>",
                7 => "<color=#D6E0F0>White</color>",
                8 => "<color=#6B2FBB>Purple</color>",
                9 => "<color=#71491E>Brown</color>",
                10 => "<color=#38FFDD>Cyan</color>",
                11 => "<color=#50EF39>Lime</color>",
                12 => "<color=#4A6AC9>Cornflower</color>",
                13 => "<color=#B13E53>Magenta</color>",
                14 => "<color=#FF8CB5>Rose</color>",
                15 => "<color=#4B2E58>Dark Purple</color>",
                16 => "<color=#388A64>Fortegreen</color>",
                17 => "<color=#66763A>Olive</color>",
                18 => "<color=#459BEA>Sky Blue</color>",
                19 => "<color=#C4A47C>Tan</color>",
                20 => "<color=#E87EE9>Orchid</color>",
                21 => "<color=#FFAFD4>Peach</color>",
                22 => "<color=#8C9BAE>Gray</color>",
                23 => "<color=#621F2B>Maroon</color>",
                24 => "<color=#296B6B>Teal</color>",
                25 => "<color=#FF6A6A>Coral</color>",
                26 => "<color=#1E2A5E>Navy Blue</color>",
                27 => "<color=#A3FFB4>Mint</color>",
                28 => "<color=#C2BEE8>Lavender</color>",
                29 => "<color=#FFC82E>Gold</color>",
                30 => "<color=#FFF68F>Banana</color>",
                31 => "<color=#FFD900>Saturated Yellow</color>",
                32 => "<color=#A6FF00>Acid Green</color>",
                33 => "<color=#8AAB90>Sage</color>",
                _ => $"<color=#FFFFFF>Color {colorId}</color>"
            };
        }
    }

    [HarmonyPatch(typeof(PlayerControl))]
    public static class PlayerNamePatch
    {
        [HarmonyPatch(nameof(PlayerControl.SetName))]
        [HarmonyPostfix]
        public static void SetNamePostfix(PlayerControl __instance) => UpdatePlayerText(__instance);

        [HarmonyPatch(nameof(PlayerControl.SetColor))]
        [HarmonyPostfix]
        public static void SetColorPostfix(PlayerControl __instance, int __0) => UpdatePlayerText(__instance);

        public static void UpdatePlayerText(PlayerControl player)
        {
            if (!ColorblindPlugin.ColorblindEnabled.Value) return;
            if (player == null || player.Data == null || player.nameText == null) return;

            int colorId = player.Data.ColorId;
            string colorLabel = ColorblindPlugin.GetColorName(colorId);
            
            string baseName = player.Data.PlayerName;
            if (baseName.Contains("\n")) baseName = baseName.Split('\n')[0];
            if (baseName.Contains("<size")) baseName = baseName.Split('<')[0].Trim();

            string targetText = $"{baseName}\n<size=80%>{colorLabel}</size>";
            if (player.nameText.text != targetText)
            {
                player.nameText.text = targetText;
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud))]
    public static class MeetingHudPatch
    {
        [HarmonyPatch(nameof(MeetingHud.Update))]
        [HarmonyPostfix]
        public static void UpdatePostfix(MeetingHud __instance)
        {
            if (!ColorblindPlugin.ColorblindEnabled.Value) return;
            if (__instance?.playerStates == null) return;

            foreach (var voteArea in __instance.playerStates)
            {
                if (voteArea == null || voteArea.NameText == null) continue;

                var goName = voteArea.gameObject.name;
                var parentName = voteArea.transform.parent != null ? voteArea.transform.parent.name : "";
                if (goName.Contains("Chat") || parentName.Contains("Chat") || parentName.Contains("Bubble") || parentName.Contains("Notification")) continue;

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player != null && player.PlayerId == voteArea.TargetPlayerId && player.Data != null)
                    {
                        int colorId = player.Data.ColorId;
                        string colorLabel = ColorblindPlugin.GetColorName(colorId);
                        
                        string baseName = player.Data.PlayerName;
                        if (baseName.Contains("\n")) baseName = baseName.Split('\n')[0];
                        if (baseName.Contains("<size")) baseName = baseName.Split('<')[0].Trim();

                        string targetText = $"{baseName} ({colorLabel})";
                        
                        if (voteArea.NameText.text != targetText)
                        {
                            voteArea.NameText.text = targetText;
                            voteArea.NameText.enableAutoSizing = false;
                            voteArea.NameText.fontSize = 1.7f;
                            voteArea.NameText.enableWordWrapping = false;
                        }
                        break;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
    public static class ChatBubblePatch
    {
        [HarmonyPrefix]
        public static bool SetNamePrefix(ChatBubble __instance, ref string playerName)
        {
            if (!ColorblindPlugin.ColorblindEnabled.Value) return true;

            if (!string.IsNullOrEmpty(playerName))
            {
                string baseName = playerName;
                if (baseName.Contains("\n")) baseName = baseName.Split('\n')[0];
                if (baseName.Contains("<size")) baseName = baseName.Split('<')[0].Trim();

                // Find matching player color to append inline modern format
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player != null && player.Data != null && player.Data.PlayerName != null)
                    {
                        string pBase = player.Data.PlayerName.Split('\n')[0].Split('<')[0].Trim();
                        if (pBase == baseName)
                        {
                            int colorId = player.Data.ColorId;
                            string colorLabel = ColorblindPlugin.GetColorName(colorId);
                            playerName = $"{baseName} <size=80%>({colorLabel})</size>";
                            break;
                        }
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManagerUpdatePatch
    {
        [HarmonyPostfix]
        public static void Postfix(HudManager __instance)
        {
            if (!ColorblindPlugin.ColorblindEnabled.Value) return;
            
            // 1. Update overworld nameplates
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null || player.nameText == null) continue;

                int colorId = player.Data.ColorId;
                string colorLabel = ColorblindPlugin.GetColorName(colorId);
                
                string baseName = player.Data.PlayerName;
                if (baseName.Contains("\n")) baseName = baseName.Split('\n')[0];
                if (baseName.Contains("<size")) baseName = baseName.Split('<')[0].Trim();

                string targetText = $"{baseName}\n<size=80%>{colorLabel}</size>";
                if (player.nameText.text != targetText)
                {
                    player.nameText.text = targetText;
                }
            }

            // 2. Update HUD trackers and Alive Players list (Strictly ignoring all chat UI)
            var allTexts = UnityEngine.Object.FindObjectsOfType<TextMeshPro>();
            foreach (var textMesh in allTexts)
            {
                if (textMesh == null || string.IsNullOrEmpty(textMesh.text)) continue;

                var parentName = textMesh.transform.parent != null ? textMesh.transform.parent.name : "";
                var goName = textMesh.gameObject.name;

                if (parentName.Contains("Task") || textMesh.text.Contains("TOTAL TASKS") || textMesh.text.Contains("has voted")) continue;
                if (parentName.Contains("Chat") || parentName.Contains("Bubble") || goName.Contains("Chat") || goName.Contains("Bubble")) continue;

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player != null && player.Data != null)
                    {
                        string baseName = player.Data.PlayerName;
                        if (string.IsNullOrEmpty(baseName)) continue;

                        if (baseName.Contains("\n")) baseName = baseName.Split('\n')[0];
                        if (baseName.Contains("<size")) baseName = baseName.Split('<')[0].Trim();

                        if (parentName.Contains("Player") || parentName.Contains("Item") || parentName.Contains("Alive") || goName.Contains("Name"))
                        {
                            if (textMesh.text == baseName || textMesh.text.StartsWith(baseName + "\n") || textMesh.text.StartsWith(baseName + " ("))
                            {
                                int colorId = player.Data.ColorId;
                                string colorLabel = ColorblindPlugin.GetColorName(colorId);
                                string targetText = $"{baseName}\n<size=80%>{colorLabel}</size>";

                                if (textMesh.text != targetText)
                                {
                                    textMesh.text = targetText;
                                    textMesh.enableAutoSizing = false;
                                    textMesh.fontSize = 2.0f;
                                    textMesh.enableWordWrapping = true;
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}