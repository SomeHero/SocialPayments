using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.External.Models
{
    public class UserModels
    {
        public class UserResponse
        {
            public Guid userId { get; set; }
            public string userName { get; set; }
            public string emailAddress { get; set; }
            public bool isConfirmed { get; set; }
            public int passwordFailuresSinceLastSuccess { get; set; }
            public DateTime? lastPasswordFailureDate { get; set; }
            public DateTime? createDate { get; set; }
            public string MobileNumber { get; set; }
            public bool IsLockedOut { get; set; }
            public string TimeZone { get; set; }
            public string Culture { get; set; }
            public string UserStatus { get; set; }
            public DateTime LastLoggedIn { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Zip { get; set; }
            public string SenderName { get; set; }
            public List<UserAttribute> UserAttributes { get; set; }
        }
        public class SubmitUserRequest
        {

        }
        public class UpdateUserRequest
        {

        }
        public class UserAttribute
        {
            public string AttributeName { get; set; }
            public string AttributeValue { get; set; }
        }
    }
}