namespace ModMenu
{
    public static class MenuConstants
    {
        public static readonly int modMenuScrollSensitivity = 90;

        public static readonly int buttonLineSpacing = 28;


        public static int TitleLabelHeight => titleLabelHeight[(int)ModMenu.menuSpacing.Value];
        public static int TitleLabelTopMargin => titleLabelTopMargin[(int)ModMenu.menuSpacing.Value];
        public static readonly int titleLabelLength = 52;
        public static readonly int titleLabelSpacePadding = 2;


        public static int OptionSelectorHeight => optionSelectorHeight[(int)ModMenu.menuSpacing.Value];
        public static readonly int optionSelectorWidth = 250;
        public static int MenuLineSpacing => menuLineSpacing[(int)ModMenu.menuSpacing.Value];


        public static int ConfigSectionHeight => configSectionHeight[(int)ModMenu.menuSpacing.Value];
        public static int ConfigSectionTopMargin => configSectionTopMargin[(int)ModMenu.menuSpacing.Value];


        public static int ConfigDescriptionHeight => configDescriptionHeight[(int)ModMenu.menuSpacing.Value];
        public static int ConfigDescriptionBottomMargin => configDescriptionBottomMargin[(int)ModMenu.menuSpacing.Value];
        public static readonly int configDescriptionLength = 70;


        public static readonly int inputTextAreaWidth = 260;
        public static readonly int inputTextAreaHeight = 35;



        // States
        private static readonly int[] titleLabelHeight = [70, 95, 95, 105];
        private static readonly int[] titleLabelTopMargin = [25, 35, 40, 20];
        private static readonly int[] optionSelectorHeight = [45, 45, 50, 55];
        private static readonly int[] menuLineSpacing = [0, 5, 15, 30];
        private static readonly int[] configSectionHeight = [70, 80, 80, 85];
        private static readonly int[] configSectionTopMargin = [0, 5, 0, 0];
        private static readonly int[] configDescriptionHeight = [22, 24, 24, 24];
        private static readonly int[] configDescriptionBottomMargin = [10, 28, 30, 30];
    }
}