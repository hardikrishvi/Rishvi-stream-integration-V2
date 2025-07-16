using System;

namespace LinnworksAPI
{
    public class OrderFolder
    {
        /// <summary>
        /// Folder ID 
        /// </summary>
		public Guid pkFolderId { get; set; }

        /// <summary>
        /// Folder name 
        /// </summary>
		public String FolderName { get; set; }
    }
}