namespace Rishvi.Modules.ShippingIntegrations.Models.ConfigStageClasses
{
    public static class ContactStage
    {
        public static Classes.ConfigStage GetContactStage
        {
            get
            {
                return new Classes.ConfigStage()
                {
                    WizardStepDescription = "Customer enters some details at this stage",
                    WizardStepTitle = "Customer Details",
                    ConfigItems = new List<Classes.ConfigItem>()
                    {
                        new Classes.ConfigItem()
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
                        new Classes.ConfigItem()
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
                        new Classes.ConfigItem()
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
                        new Classes.ConfigItem()
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
                        new Classes.ConfigItem()
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
                        new Classes.ConfigItem()
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
                        new Classes.ConfigItem()
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
                        new Classes.ConfigItem()
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
                        new Classes.ConfigItem()
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
                        new Classes.ConfigItem()
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
                            ListValues = new List<Classes.ConfigItemListItem>()
                            {
                                new Classes.ConfigItemListItem()
                                {
                                    Display = "United Kingdom",
                                    Value = "GB"
                                },
                                new Classes.ConfigItemListItem()
                                {
                                    Display = "Germany",
                                    Value = "DE"
                                },
                                new Classes.ConfigItemListItem()
                                {
                                    Display = "France",
                                    Value = "FR"
                                },
                                new Classes.ConfigItemListItem()
                                {
                                    Display = "United States",
                                    Value = "US"
                                }
                            }
                        },
                        new Classes.ConfigItem()
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
                        new Classes.ConfigItem()
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
                        new Classes.ConfigItem()
                        {
                            ConfigItemId = "AutoOrderSync",
                            Description = "Auto Order Sync",
                            GroupName = "Auto Order Sync",
                            MustBeSpecified = true,
                            Name = "Auto Order Sync",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 13,
                            ValueType = Classes.ConfigValueType.BOOLEAN
                        },
                        new Classes.ConfigItem()
                        {
                            ConfigItemId = "AutoOrderDespatchSync",
                            Description = "Auto Order Despatch Sync",
                            GroupName = "Auto Order Despatch Sync",
                            MustBeSpecified = true,
                            Name = "Auto Order Despatch Sync",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 14,
                            ValueType = Classes.ConfigValueType.BOOLEAN
                        },
                        new Classes.ConfigItem()
                        {
                            ConfigItemId = "UseDefaultLocation",
                            Description = "Use Default Location",
                            GroupName = "Use Default Location",
                            MustBeSpecified = true,
                            Name = "Use Default Location",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 15,
                            ValueType = Classes.ConfigValueType.BOOLEAN
                        },
                        new Classes.ConfigItem()
                        {
                            ConfigItemId = "DefaultLocation",
                            Description = "Default Location",
                            GroupName = "Default Location",
                            MustBeSpecified = true,
                            Name = "Default Location",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 16,
                            ValueType = Classes.ConfigValueType.STRING
                        },
                        new Classes.ConfigItem()
                        {
                            ConfigItemId = "LinnHour",
                            Description = "Order Sync Days determine how far back Linnworks will go to retrieve and sync orders from linnworks to stream Default days is 1",
                            GroupName = "Linn Hour",
                            MustBeSpecified = true,
                            Name = "Order Sync Days",
                            ReadOnly = false,
                            SelectedValue = "2400",
                            SortOrder = 17,
                            ValueType = Classes.ConfigValueType.INT
                        },
                        new Classes.ConfigItem()
                        {
                            ConfigItemId = "SendChangeToStream",
                            Description = "Send Change To Stream",
                            GroupName = "Send Change To Stream",
                            MustBeSpecified = true,
                            Name = "Send Change To Stream",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 18,
                            ValueType = Classes.ConfigValueType.BOOLEAN
                        },
                        new Classes.ConfigItem()
                        {
                            ConfigItemId = "LABELREFERENCE",
                            Description = "Label Reference",
                            GroupName = "Label Reference",
                            MustBeSpecified = true,
                            Name = "Label Reference",
                            ReadOnly = false,
                            SelectedValue = "",
                            SortOrder = 29,
                            ValueType = Classes.ConfigValueType.LIST,
                            ListValues = new List<Classes.ConfigItemListItem>()
                            {
                                new Classes.ConfigItemListItem()
                                {
                                    Display = "Linnworks Order Id",
                                    Value = "LinnworksOrderId"
                                },
                                new Classes.ConfigItemListItem()
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
