using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempMail
{
    class Email
    {
        public string email { get; set; }
        public string key { get; set; }
        public string error { get; set; }
    }

    class Letter
    {
        public int id { get; set; }
        public string date { get; set; }
        public string subject { get; set; }
        public string from { get; set; }
        public string error { get; set; }
    }
    
    class Letters
    {
        public Letter[] letters { get; set; }
        public string error { get; set; }
    }

    class Body
    {
        public string message { get; set; }
        public string error { get; set; }
    }

    class User
    {
        public string hash { get; set; }
        public string error { get; set; }
    }
    class Limit
    {
        public int? limit { get; set; }
        public string error { get; set; }
    }
}
