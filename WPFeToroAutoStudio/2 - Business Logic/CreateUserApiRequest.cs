using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Applenium._2___Business_Logic
{
    internal class CreateUserApiRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string OrigCid { get; set; }
        public string Fax { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string DateOfBirth { get; set; }
        public string RegIP { get; set; }
        public string Language { get; set; }
        public int PlayerLevel { get; set; }
        public int SerialID { get; set; }

        public CreateUserApiRequest(string username, string password, string email, string origCid, string fax,
            string address, string city, string state, string country, string zip, string dateOfBirth, string regIP,
            string language, int playerLevel, int serialId)
        {
            //"Username": "foobarforev123qwe1", "Password": "123456", "Email": "fo123jas1d@gmail.com", "OrigCid": "123123123", "Fax": "123123123123", "Address": "asdasdasdasd", "City": "asdasdasdasd", "State": "asdasdasd", "Country": "France", "Zip": "424242", "DateOfBirth": "1966-6-6", "RegIP": "127.0.0.1", "Language": "1", "PlayerLevel": 4, "ReferralId": 42};
            Username = username;
            Password = password;
            PlayerLevel = playerLevel;
            SerialID = serialId;
            Email = email;
            Fax = fax;
            OrigCid = origCid;
            Address = address;
            City = city;
            State = state;
            Country = country;
            Zip = zip;
            DateOfBirth = dateOfBirth;
            RegIP = regIP;
            Language = language;





        }

    }
}