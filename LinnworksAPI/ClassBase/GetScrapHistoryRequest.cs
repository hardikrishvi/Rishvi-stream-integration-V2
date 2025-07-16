using System;

namespace LinnworksAPI
{
    public class GetScrapHistoryRequest
    {
        public Int32 PageNumber { get; set; }

        public Int32 EntriesPerPage { get; set; }
    }
}