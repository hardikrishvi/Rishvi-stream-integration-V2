﻿using System;

namespace Rishvi.Modules.Core.Data
{
    public interface IModificationHistory
    {
        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
    }
}