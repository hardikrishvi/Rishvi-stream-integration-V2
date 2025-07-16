using System;

namespace LinnworksAPI
{
    public class PictureSource
    {
        public Guid PictureId { get; set; }

        public String Source { get; set; }

        public Boolean IsMain { get; set; }

        public Int32 SortOrder { get; set; }
    }
}