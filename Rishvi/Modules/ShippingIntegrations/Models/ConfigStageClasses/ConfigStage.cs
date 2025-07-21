namespace Rishvi.Modules.ShippingIntegrations.Models.ConfigStageClasses
{
    public static class UserConfigStage
    {
        public static Classes.ConfigStage GetUserConfigStage(Rishvi.Models.Authorization authConfig)
        {
            return new Classes.ConfigStage()
            {
                WizardStepDescription = "Customer config can be changed",
                WizardStepTitle = "Customer Details",
                ConfigItems = new List<Classes.ConfigItem>() {
                    new Classes.ConfigItem()
                    {
                        ConfigItemId = "ClientId",
                        Description = "Client Id",
                        GroupName = "Account Details",
                        MustBeSpecified = true,
                        Name = "Client Id",
                        ReadOnly = false,
                        SelectedValue = authConfig.ClientId,
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
                        SelectedValue = authConfig.ClientSecret,
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
                        SelectedValue = authConfig.AccountName,
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
                        SelectedValue = authConfig.CompanyName,
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
                        SelectedValue = authConfig.AddressLine1,
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
                        SelectedValue = authConfig.AddressLine2,
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
                        SelectedValue = authConfig.AddressLine3,
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
                        SelectedValue = authConfig.City,
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
                        SelectedValue = authConfig.County,
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
                        SelectedValue = authConfig.CountryCode,
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
                        SelectedValue = authConfig.ContactPhoneNo,
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
                        SelectedValue = authConfig.PostCode,
                        SortOrder = 12,
                        ValueType = Classes.ConfigValueType.STRING
                    },
                    new Classes.ConfigItem()
                    {
                        ConfigItemId = "AutoOrderSync",
                        Description = "Automatically syncs new and updated orders from stream",
                        GroupName = "Account Configuration",
                        MustBeSpecified = true,
                        Name = "Order Sync",
                        ReadOnly = false,
                        SelectedValue = authConfig.AutoOrderSync.ToString(),
                        SortOrder = 13,
                        ValueType = Classes.ConfigValueType.BOOLEAN
                    },
                    new Classes.ConfigItem()
                    {
                        ConfigItemId = "AutoOrderDespatchSync",
                        Description = "Triggers automatic order dispatch syncing with stream system",
                        GroupName = "Account Configuration",
                        MustBeSpecified = true,
                        Name = "Dispatch Sync",
                        ReadOnly = false,
                        SelectedValue = authConfig.AutoOrderDespatchSync.ToString(),
                        SortOrder = 14,
                        ValueType = Classes.ConfigValueType.BOOLEAN
                    },
                    new Classes.ConfigItem()
                    {
                        ConfigItemId = "UseDefaultLocation",
                        Description = "Use Default Location",
                        GroupName = "Account Configuration",
                        MustBeSpecified = true,
                        Name = "Use Default Location",
                        ReadOnly = false,
                        SelectedValue = authConfig.UseDefaultLocation.ToString(),
                        SortOrder = 15,
                        ValueType = Classes.ConfigValueType.BOOLEAN
                    },
                    new Classes.ConfigItem()
                    {
                        ConfigItemId = "DefaultLocation",
                        Description = "Default Location",
                        GroupName = "Account Configuration",
                        MustBeSpecified = true,
                        Name = "Default Location",
                        ReadOnly = false,
                        SelectedValue = authConfig.DefaultLocation.ToString(),
                        SortOrder = 16,
                        ValueType = Classes.ConfigValueType.STRING
                    },
                    new Classes.ConfigItem()
                    {
                        ConfigItemId = "LinnDays",
                        Description = "Order Sync Days determine how far back Linnworks will go to retrieve and sync orders from linnworks to stream Default days is 1",
                        GroupName = "Account Configuration",
                        MustBeSpecified = true,
                        Name = "Order Sync Days",
                        ReadOnly = false,
                        SelectedValue = authConfig.LinnDays.ToString(),
                        SortOrder = 17,
                        ValueType = Classes.ConfigValueType.INT
                    },
                    new Classes.ConfigItem()
                    {
                        ConfigItemId = "SendChangeToStream",
                        Description = "Send Change To Stream",
                        GroupName = "Account Configuration",
                        MustBeSpecified = true,
                        Name = "Send Change To Stream",
                        ReadOnly = false,
                        SelectedValue = authConfig.SendChangeToStream.ToString(),
                        SortOrder = 18,
                        ValueType = Classes.ConfigValueType.BOOLEAN
                    },
                    new Classes.ConfigItem()
                    {
                        ConfigItemId = "HandsOnDate",
                        Description = "If enabled, the system will automatically push the current date as the Hands-on Date to Stream",
                        GroupName = "Account Configuration",
                        MustBeSpecified = true,
                        Name = "Hands On Date",
                        ReadOnly = false,
                        SelectedValue = authConfig.HandsOnDate.ToString(),
                        SortOrder = 19,
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
                        SelectedValue = authConfig.LabelReference,
                        SortOrder = 13,
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
