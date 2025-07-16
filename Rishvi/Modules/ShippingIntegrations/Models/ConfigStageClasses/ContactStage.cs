namespace Rishvi.Modules.ShippingIntegrations.Models.ConfigStageClasses
{
    public static class ContactStage
    {
        public static Classes.ConfigurationStage GetContactStage
        {
            get
            {
                return new Classes.ConfigurationStage
                {
                    WizardStepDescription = "Customer enters some details at this stage",
                    WizardStepTitle = "Customer Details",
                    ConfigItems = new List<Classes.ConfigItem>
                    {
                        new Classes.ConfigItem
                        {
                            ConfigItemId = "ClientId",
                            Description = "Client Id",
                            GroupName = "Account Details",
                            MustBeSpecified = true,
                            Name = "Client Id",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 1,
                            ValueType = Classes.ConfigValueType.STRING
                        },
                        new Classes.ConfigItem
                        {
                            ConfigItemId = "ClientSecret",
                            Description = "Client Secret",
                            GroupName = "Account Details",
                            MustBeSpecified = true,
                            Name = "Client Secret",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 2,
                            ValueType = Classes.ConfigValueType.PASSWORD
                        },
                        new Classes.ConfigItem
                        {
                            ConfigItemId = "NAME",
                            Description = "Contact name",
                            GroupName = "Sender Address",
                            MustBeSpecified = true,
                            Name = "Contact Name",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 3,
                            ValueType = Classes.ConfigValueType.STRING
                        },
                        new Classes.ConfigItem
                        {
                            ConfigItemId = "COMPANY",
                            Description = "Company name",
                            GroupName = "Sender Address",
                            MustBeSpecified = true,
                            Name = "Company Name",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 4,
                            ValueType = Classes.ConfigValueType.STRING
                        },
                        new Classes.ConfigItem
                        {
                            ConfigItemId = "ADDRESS1",
                            Description = "Address line 1",
                            GroupName = "Sender Address",
                            MustBeSpecified = true,
                            Name = "Address 1",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 5,
                            ValueType = Classes.ConfigValueType.STRING
                        },
                        new Classes.ConfigItem
                        {
                            ConfigItemId = "ADDRESS2",
                            Description = "Street Number",
                            GroupName = "Sender Address",
                            MustBeSpecified = false,
                            Name = "Street Number",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 6,
                            ValueType = Classes.ConfigValueType.STRING
                        },
                        new Classes.ConfigItem
                        {
                            ConfigItemId = "ADDRESS3",
                            Description = "Address",
                            GroupName = "Sender Address",
                            MustBeSpecified = false,
                            Name = "Address",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 7,
                            ValueType = Classes.ConfigValueType.STRING
                        },
                        new Classes.ConfigItem
                        {
                            ConfigItemId = "CITY",
                            Description = "Town/City name",
                            GroupName = "Sender Address",
                            MustBeSpecified = true,
                            Name = "Town/City",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 8,
                            ValueType = Classes.ConfigValueType.STRING
                        },
                        new Classes.ConfigItem
                        {
                            ConfigItemId = "REGION",
                            Description = "Region",
                            GroupName = "Sender Address",
                            MustBeSpecified = true,
                            Name = "Region",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 9,
                            ValueType = Classes.ConfigValueType.STRING
                        },
                        new Classes.ConfigItem
                        {
                            ConfigItemId = "COUNTRY",
                            Description = "Country",
                            GroupName = "Sender Address",
                            MustBeSpecified = true,
                            Name = "Country",
                            ReadOnly = true,
                            SelectedValue = "GB",
                            SortOrder = 10,
                            ValueType = Classes.ConfigValueType.LIST,
                            ListValues = new List<Classes.ConfigItemListItem>
                            {
                                new Classes.ConfigItemListItem
                                {
                                    Display = "United Kingdom",
                                    Value = "GB"
                                },
                                new Classes.ConfigItemListItem
                                {
                                    Display = "Germany",
                                    Value = "DE"
                                },
                                new Classes.ConfigItemListItem
                                {
                                    Display = "France",
                                    Value = "FR"
                                },
                                new Classes.ConfigItemListItem
                                {
                                    Display = "United States",
                                    Value = "US"
                                }
                            }
                        },
                        new Classes.ConfigItem
                        {
                            ConfigItemId = "TELEPHONE",
                            Description = "Telephone",
                            GroupName = "Sender Address",
                            MustBeSpecified = true,
                            Name = "Telephone",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 11,
                            ValueType = Classes.ConfigValueType.STRING
                        },
                        new Classes.ConfigItem
                        {
                            ConfigItemId = "POSTCODE",
                            Description = "Postal Code",
                            GroupName = "Sender Address",
                            MustBeSpecified = true,
                            Name = "Postal Code",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 12,
                            ValueType = Classes.ConfigValueType.STRING
                        },
                        new Classes.ConfigItem
                        {
                            ConfigItemId = "LABELREFERENCE",
                            Description = "Label Reference",
                            GroupName = "Label Reference",
                            MustBeSpecified = true,
                            Name = "Label Reference",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 13,
                            ValueType = Classes.ConfigValueType.LIST,
                            ListValues = new List<Classes.ConfigItemListItem>
                            {
                                new Classes.ConfigItemListItem
                                {
                                    Display = "Linnworks Order Id",
                                    Value = "LinnworksOrderId"
                                },
                                new Classes.ConfigItemListItem
                                {
                                    Display = "Channel Referance",
                                    Value = "ChannelReferance"
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}
