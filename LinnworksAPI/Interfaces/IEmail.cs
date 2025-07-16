using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public interface IEmailController
    {
        GenerateAdhocEmailResponse GenerateAdhocEmail(GenerateAdhocEmailRequest request);
        GenerateFreeTextEmailResponse GenerateFreeTextEmail(GenerateFreeTextEmailRequest request);
        EmailTemplate GetEmailTemplate(Int32 pkEmailTemplateRowId);
        List<EmailTemplateHeader> GetEmailTemplates();
    }
}