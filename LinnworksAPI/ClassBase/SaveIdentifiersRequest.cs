namespace LinnworksAPI
{
    /// <summary>
    /// The identifier to save. For a new identifier, only the name, tag and image url are required. 
    /// </summary>
    public class SaveIdentifiersRequest
    {
        public Identifier Identifier { get; set; }
    }
}