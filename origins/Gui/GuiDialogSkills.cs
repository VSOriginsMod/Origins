using Vintagestory.API.Client;

namespace Origins.Gui
{
    /// <summary>
    /// Represents a GUI dialog for displaying player skills.
    /// </summary>
    internal class GuiDialogSkills : GuiDialog
    {
        //The name of the GUI element tag for the purpose of updating text
        public const string SkillsDialog = "Skills Dialog";

        /// <summary>
        /// Constructor for the GuiDialogSkills class.
        /// </summary>
        /// <param name="capi">The ICoreClientAPI object used to pass to the base class and compose the dialog.</param>
        public GuiDialogSkills(ICoreClientAPI capi) : base(capi)
        {
            ComposeGUI();
        }

        /// <summary>
        /// The name of the keybinding that triggers the GUI element
        /// </summary>
        public override string ToggleKeyCombinationCode => "Skill Interface";

        /// <summary>
        /// This method is called when the GUI is opened.
        /// It overrides the base implementation of the OnGuiOpened method.
        /// </summary>
        public override void OnGuiOpened()
        {
            UpdateSkillsText();
        }

        /// <summary>
        /// Creates the GUI window for the skills UI.
        /// </summary>
        //Creatures the GUI Window
        private void ComposeGUI()
        {
            // Auto-sized dialog at the center of the screen
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            // Just a simple 300x300 pixel box
            ElementBounds textBounds = ElementBounds.Fixed(0, 40, 300, 100);

            // Background boundaries. Again, just make it fit it's child elements, then add the text as a child element
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(textBounds);

            // Lastly, create the dialog
            SingleComposer = capi.Gui.CreateCompo("myAwesomeDialog", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("SkillsText!", OnTitleBarClose)
                .AddDynamicText("Text", CairoFont.WhiteDetailText(), textBounds, SkillsDialog)
                .Compose()
            ;
        }

        /// <summary>
        /// Called when the GUI is opened
        /// </summary>
        //Called when the GUI is opened
        private void OnTitleBarClose()
        {
            TryClose();
        }

        //<summary>
        //Updates the text for the skills screen
        //</summary>
        //<remakrs>
        //Text is a placeholder, it still needs to be hooked up to retireve the player's skills
        //</remarks>
        private void UpdateSkillsText()
        {
            string skilltext = "Skills:\n";

            foreach (var attr in capi.World.Player.WorldData.EntityPlayer.WatchedAttributes)
            {
                capi.Logger.Debug(attr.Key);
                if (!attr.Key.StartsWith("s_"))
                {
                    continue;
                }
                skilltext += "\t" + attr.Key + ": " + attr.Value.ToString() + "\n";
            }

            capi.Logger.Debug("found following:" + skilltext);

            SingleComposer.GetDynamicText(SkillsDialog)?.SetNewText(skilltext);
        }
    }
}