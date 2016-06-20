﻿using System;
using System.Collections.Generic;

namespace FaithEngage.Core.Cards
{
    public class CardActionResultArgs
    {
        public Dictionary<string,string> Responses {
            get;
            set;
        }

        public Guid? DestinationDisplayUnit {
            get;
            set;
        }
			
    }
}
