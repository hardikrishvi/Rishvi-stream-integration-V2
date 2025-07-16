using System.Collections.Generic;

namespace LinnworksAPI
{
    public interface IMacroController
    {
        GetInstalledMacrosResponse GetInstalledMacros(GetInstalledMacroRequest request);
        List<MacroRegister> GetMacroConfigurations(GetMacroConfigurationsRequest request);
    }
}