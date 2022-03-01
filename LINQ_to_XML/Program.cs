using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Security;
using System.Net;

namespace LINQ_to_XML
{
    class Program
    {
        public static void displayFunctions()
        {
            Utilities.GetSystemUpdate();

            Console.WriteLine("1- Sign in");
            Console.WriteLine("2- Edit (Administrator only)");
            Console.WriteLine("3- Sign Up");
            Console.WriteLine("0- Exit");
        }

        public static void setPlayerLatestSettings(XElement user)
        {
            XElement userSettings = user.Element("GameSettings");
            Console.Write("Level:".PadRight(8) + userSettings.Element("Level").Value);
            Console.WriteLine("\t\t\t" + "Energy:".PadRight(8) + userSettings.Element("Energy").Value);
            Console.Write("XP:".PadRight(8) + userSettings.Element("XP").Value);
            Console.WriteLine("\t\t\t" + "Water:".PadRight(8) + userSettings.Element("Water").Value);
            Console.WriteLine("Coins:".PadRight(8) + userSettings.Element("Coins").Value);
            Console.WriteLine("Money:".PadRight(8) + userSettings.Element("Money").Value);
            Console.WriteLine();
        }

        public static void validOptions_Users(XElement user)
        {

            setPlayerLatestSettings(user);

            Console.WriteLine("1- Check your warehouse");
            Console.WriteLine("2- Check your garden");
            Console.WriteLine("3- Buy new items");
            Console.WriteLine("4- Customize your look");
            Console.WriteLine("5- Sell Product");
            Console.WriteLine("6- Visit friends");
            Console.WriteLine("7- Add friends");
            Console.WriteLine("8- Check Mailbox");
            Console.WriteLine("0- Sign Out\n");
            Console.WriteLine();
        }
        public static void Enter(XElement user)
        {
            Console.WriteLine("\nHello " + user.Element("avatar").Element("alias").Value + "!\n");
            int option = -1;
            while (option != 0)
            {
                Utilities.UpdateUserLevel(user);

                validOptions_Users(user);
                
                Console.Write("Your Option:\t");
                string line = Console.ReadLine();
                try
                {
                    option = int.Parse(line);
                }
                catch
                {
                    Console.WriteLine("Stop Playing with letters!\n");
                    /* eza ma 7atayt hey l instruction... sallim jadal knt eyelo case 6 aw 5 aw 7ayala
                    w jit ana raja3ne la hon bde na2e ra2em, eza eltello ".." aw "...." bi fawetne 3ala ekhir case
                    kenet fiya... w bi ele stop playing
                    so eza kenet 6, w tl3t menna, w ayadtelo bel input "..", 7a y2ele stop playing with characters w yfawetne 3al 6
                    so b2ayid hey 7ata ghyrlo l value tb3 valeur l option -1 w ma khali hu yet7akamle bi ekhir we7de...
                    cz l -1 7a tre7le 3ala l default, Invalid operation ( w 7a yzedle stop playing men wara l parsing l ma zobit)
                    so lama ello ".." aw "...." 7a yre7le 3l case li kenet fiya men abel, w hiye l default
                    aw btw ma droure bas "...." fi ykoun 7ayala value gher ma ata3it bel parsing
                    so l option ma tghyrt deja men abel, 7a tdal tfawetne ka2an b3dne mna2iya... so je l'ecrase
                     */
                    option = -1;

                }

                if (option == 0)
                    break;

                Utilities.GetSystemUpdate();
                switch (option)
                {
                    case 1:
                        Utilities.checkWarehouse(user);
                        break;
                    case 2:
                        Utilities.GoToMyGarden(user);
                        break;
                    case 3:
                        Utilities.buyItems(user);
                        break;
                    case 4:
                        Console.WriteLine("Store under construction... Come again later!");
                        break;
                    case 5:
                        Utilities.SellProduct(user);
                        break;
                    case 6:
                        Utilities.displayFriendList(user);
                        break;
                    case 7:
                        Utilities.AddFriends(user);
                        break;
                    case 8:
                        Utilities.CheckMailBox(user);
                        break;
                    case 0:
                        break;
                    default:
                        Console.WriteLine("Invalid Operation\n");
                        break;
                }


            }

        }

        public static string setPassword()
        {
            SecureString password = new SecureString();
            ConsoleKeyInfo key;

            //The whole reason for using the SecureString object is to avoid creating a string object (which is loaded into memory and kept there in plaintext until garbage collection).
            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (!char.IsControl(key.KeyChar))
                {
                    password.AppendChar(key.KeyChar);
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password.RemoveAt(password.Length - 1);
                        Console.Write("\b \b");
                    }
                }
            }
            while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return new NetworkCredential("", password).Password;
            
        }


        static void Main(string[] args)
        {
            int option = -1;
            while (option != 0)
            {
                option = -1;
                displayFunctions();

                Console.Write("\nSelect Option:\t");
                string line = Console.ReadLine();
                try
                {
                    option = int.Parse(line);
                }
                catch
                {
                    Console.WriteLine("Stop Playing with letters!\n");
                }

                switch(option)
                {
                    case 0:
                        Console.WriteLine("Goodbye!");
                        break;

                    case 1:
                        string username, password;
                        Console.Write("Username:\t");
                        username = Console.ReadLine();
                        Console.Write("Password:\t");
                        password = setPassword();

                        XElement tempUser = Utilities.AuthenticationLogIn(username, password);
                        if (tempUser != null)
                        {
                            Enter(tempUser);
                        }
                        else
                            Console.WriteLine("Invalid Username or Password\n");
                        //Utilities.EmployeesOperations();
                        break;
                    case 2:
                        Console.Write("Administration restricted area!\nPassword:\t");
                        string passwordAdmin = setPassword();
                        break;
                    case 3:
                        Utilities.AddNewAccount();
                        break;
                    default:
                        Console.WriteLine("Invalid Operation\n");
                        break;
                }
            }

            //User u1 = Utilities.ExtractUserByID(1);
            //Debug.WriteLine(u1.Fullname);
        }
    }
}

