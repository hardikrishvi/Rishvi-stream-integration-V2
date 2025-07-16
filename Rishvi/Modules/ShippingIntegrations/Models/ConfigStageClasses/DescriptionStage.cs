namespace Rishvi.Modules.ShippingIntegrations.Models.ConfigStageClasses
{
    public static class DescriptionStage
    {
        public static Classes.ConfigurationStage GetDescriptionStage
        {
            get
            {
                return new Classes.ConfigurationStage
                {
                    WizardStepDescription = "Here you can insert a link to a registration for form example " +
                                            "<a href='http://www.gmail.com?token=[{token}]'>Register Here</a> " +
                                            "where you can replace the token with pass through token.",
                    WizardStepTitle = "Very flexible description and instructions",
                    ConfigItems = new List<Classes.ConfigItem>
                    {
                        new Classes.ConfigItem
                        {
                            ConfigItemId = "BOOLEANVALUE",
                            Description = "Some question?",
                            GroupName = "",
                            MustBeSpecified = true,
                            Name = "Some question",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 1,
                            ValueType = Classes.ConfigValueType.BOOLEAN
                        }
                    }
                };
            }
        }
    }
}
