﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class UserPayPointHistory
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string PayPointURI { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
