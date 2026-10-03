namespace ModMenu
{
    public static class MenuConstants
    {
        public static readonly int modMenuScrollSensitivity = 90;

        public static readonly int buttonLineSpacing = 28;


        public static int TitleLabelHeight { get { return titleLabelHeight[(int)ModMenu.menuSpacing.Value]; } }
        public static int TitleLabelTopMargin { get { return titleLabelTopMargin[(int)ModMenu.menuSpacing.Value]; } }
        public static readonly int titleLabelLength = 54;
        public static readonly int titleLabelSpacePadding = 2;


        public static int OptionSelectorHeight { get { return optionSelectorHeight[(int)ModMenu.menuSpacing.Value]; } }
        public static readonly int optionSelectorWidth = 250;
        public static int MenuLineSpacing { get { return menuLineSpacing[(int)ModMenu.menuSpacing.Value]; } }


        public static int ConfigSectionHeight { get { return configSectionHeight[(int)ModMenu.menuSpacing.Value]; } }
        public static int ConfigSectionTopMargin { get { return configSectionTopMargin[(int)ModMenu.menuSpacing.Value]; } }


        public static int ConfigDescriptionHeight { get { return configDescriptionHeight[(int)ModMenu.menuSpacing.Value]; } }
        public static int ConfigDescriptionBottomMargin { get { return configDescriptionBottomMargin[(int)ModMenu.menuSpacing.Value]; } }
        public static readonly int configDescriptionLength = 70;


        public static readonly int inputTextAreaWidth = 260;
        public static readonly int inputTextAreaHeight = 35;



        // States
        private static readonly int[] titleLabelHeight = [70, 95, 95, 105];
        private static readonly int[] titleLabelTopMargin = [25, 50, 50, 20];
        private static readonly int[] optionSelectorHeight = [45, 45, 50, 55];
        private static readonly int[] menuLineSpacing = [0, 5, 20, 50];
        private static readonly int[] configSectionHeight = [70, 80, 85, 85];
        private static readonly int[] configSectionTopMargin = [10, 10, 0, 0];
        private static readonly int[] configDescriptionHeight = [22, 24, 24, 24];
        private static readonly int[] configDescriptionBottomMargin = [15, 30, 30, 30];
    }
}