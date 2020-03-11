using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;

namespace VJInfomationManager210220
{
    class Class1
    {
        public void caigi()
        {
        }
        public string PhoneStandardizedHandle(string phone)
        {
            string returnphone = "";
            phone = phone.Replace(" ", "");
            string[] ListFirstNumberAccept = File.ReadAllLines(Application.StartupPath + "\\emailphoneruler.txt");
            char[] RemoveChar = ListFirstNumberAccept[0].ToCharArray();
            bool AcceptThisPhone = false;
            for (int a = 2; a < ListFirstNumberAccept.Length; a++)
            {
                string[] ListNetwork = ListFirstNumberAccept[a].Split(',');
                for (int b = 1; b < ListNetwork.Length; b++)
                {
                    if (Regex.IsMatch(phone, "^" + ListNetwork[b]))
                    {
                        AcceptThisPhone = true;
                        break;
                    }
                }
                if (AcceptThisPhone)
                    break;
            }
            if (AcceptThisPhone)
            {
                try
                {
                    MatchCollection coll = Regex.Matches(phone, @"^84(\d{9}$)");
                    phone = "0" + coll[0].Groups[1].Value;
                }
                catch { }
                if (phone.Length == 10)
                    returnphone = phone;
            }
            return returnphone;
        }
        public string HandlePhoneNetwork(string phone)
        {
            string PhoneNetwork = "Other";
            string[] ListFirstNumberAccept = File.ReadAllLines(Application.StartupPath + "\\emailphoneruler.txt");
            //char[] RemoveChar = ListFirstNumberAccept[0].ToCharArray();
            bool IsANetwork = false;
            for (int a = 2; a < ListFirstNumberAccept.Length; a++)
            {
                string[] ListNetwork = ListFirstNumberAccept[a].Split(',');
                for (int b = 1; b < ListNetwork.Length; b++)
                {
                    if (Regex.IsMatch(phone, "^" + ListNetwork[b]))
                    {
                        IsANetwork = true;
                        PhoneNetwork = ListNetwork[0];
                        break;
                    }
                }
                if (IsANetwork)
                    break;
            }
            return PhoneNetwork;
        }
        public string EmailStandardizedHandle(string email)
        {
            string returnemail = "";
            char[] ListCharacterNotAccept = File.ReadAllLines(Application.StartupPath + "\\emailphoneruler.txt")[1].ToCharArray();
            if (email.Split('@').Length == 2)
                if (email.Split('.').Length >= 2)
                {
                    bool AcceptThisEmail = true;
                    for (int a = 0; a < ListCharacterNotAccept.Length; a++)
                        if (email.Split(ListCharacterNotAccept[a]).Length > 1)
                        {
                            AcceptThisEmail = false;
                            break;
                        }
                    if (AcceptThisEmail)
                        returnemail = email;
                }
            return returnemail;
        }
    }
}
