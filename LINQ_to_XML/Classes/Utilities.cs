using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Globalization;
using System.Security;
using System.Net;

namespace LINQ_to_XML
{
    public static class Utilities
    {
        public static string GlobalPath = "..\\..\\Data\\";

        public static String path = GlobalPath + "CountryStory.xml";
        public static XElement xElement = XElement.Load(path);
        
        #region employees
        public static XElement AuthenticationLogIn(string username, string password)
        {
            string user="", pswd="";
            foreach (XElement xEle in xElement.Descendants("bio"))
            {
                try
                {
                    user = xEle.Element("username").Value;
                    pswd = xEle.Element("mot_de_passe").Value;
                }
                catch
                {
                    user = "";
                    pswd = "";
                }

                if (user.Equals(username) && pswd.Equals(password))
                    return xEle.Parent;
            }
            return null;
        } 

        public static bool existedItemInWarehouse(XElement user, uint productID)
        {
            foreach (var item in user.Descendants("myItem"))
            {
                if (uint.Parse(item.Element("idItem").Value) == productID)
                    return true;
            }
            return false;
        }


        public static void buyItems(XElement user)
        {
            int option = -1;

            while (option != 0)
            {
                Console.WriteLine("1- Buy Items");
                Console.WriteLine("2- Display Details");
                Console.WriteLine("3- Display Stock");
                Console.WriteLine("0- Back");


                Console.Write("Your Option:\t");
                string line = Console.ReadLine();
                try
                {
                    option = int.Parse(line);

                    switch (option)
                    {
                        case 1:
                            Console.Write("\t> Buy # ");
                            string line1 = Console.ReadLine();
                            
                            uint productID = 0;
                            try
                            {
                                productID = Convert.ToUInt32(line1);
                                BuyProduct(user, productID);
                            }
                            catch
                            {
                                Console.WriteLine("Stop Playing with letters!\n");
                            }
                            break;
                        case 2:
                            Console.Write("\t> Search # ");
                            string str = Console.ReadLine();

                            int prodID = 0;
                            try
                            {
                                prodID = Convert.ToInt32(str);
                                getProductInfo(prodID);
                            }
                            catch
                            {
                                Console.WriteLine("Stop Playing with letters!\n");
                            }
                            break;
                        case 3:
                            displayStock(user);
                            break;
                        case 0:
                            return;
                        default:
                            Console.WriteLine("Invalid Operation\n");
                            break;
                    }
                }
                catch
                {
                    Console.WriteLine("Stop Playing with letters!\n");
                }

            }
        }

        public static void acheterProduit(XElement user, uint productID, string type)
        {
            bool foundItem = false;

            foreach (var product in xElement.Descendants(type))
            {
                if ((uint.Parse(product.Attribute(type + "_id").Value) == productID) && (product.Attribute("released_in_stock").Value.Equals("yes")))
                {
                    if (product.Element(type + "_Data").Element("edition").Attribute("type").Value.Equals("available"))
                    {
                        foundItem = true;
                        int lockLevel = lockedOrUnlockedItem(user, product.Element(type + "_Data"));
                        if (lockLevel == 0)
                        {
                            int existedMoney = int.Parse(user.Element("GameSettings").Element("Coins").Value);
                            int neededMoney = int.Parse(product.Element("Cost").Element("price").Element("finalPrice").Value);
                            
                            if (existedMoney >= neededMoney)
                            {
                                string name = product.Element(type + "_Data").Element("name").Value;

                                if (!existedItemInWarehouse(user, productID))
                                {
                                    user.Element("Warehouse").Add(
                                    new XElement(new XElement("myItem",
                                                    new XElement("idItem", productID),
                                                new XElement("nameItem", name),
                                                new XElement("quantityItem", 1),
                                                new XElement("priceItem" , new XAttribute("CurrencyUnit" , "CountryCoin"), neededMoney))));
                                    //xElement.Save(path);
                                }
                                else
                                {
                                    foreach (var existedProduct in user.Descendants("myItem"))
                                    {
                                        if (int.Parse(existedProduct.Element("idItem").Value) == productID)
                                        {
                                            int totalExisted = int.Parse(existedProduct.Element("quantityItem").Value);
                                            existedProduct.SetElementValue("quantityItem", ++totalExisted);
                                            //xElement.Save(path);
                                        }
                                    }
                                }

                                int finalbudget = existedMoney - neededMoney;
                                user.Element("GameSettings").SetElementValue("Coins", finalbudget);
                                xElement.Save(path);
                                Console.WriteLine(name + " succesfully bought!");
                            }
                            else
                            {
                                int stillNeeded = neededMoney - existedMoney;
                                Console.WriteLine("Not Enough Money!! You still need " + stillNeeded + " " + product.Element("Cost").Element("price").Attribute("CurrencyUnit").Value);
                            }
                            //break;
                        }
                        else
                        {
                            Console.WriteLine("Can't buy item!! Wait till Level " + lockLevel + "\n");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unavailable Item!!");
                    }
                }
            }
            if(!foundItem)
                Console.WriteLine("Product ID not available!! Press 3- to check all the existed items");
        }

        public static void BuyProduct(XElement user, uint productID)
        {
            if (productID < 100)
                Console.WriteLine("Private Section!!");
            else if (productID < 200)
                acheterProduit(user, productID, "Seed");
            else if (productID < 300)
                acheterProduit(user, productID, "Tree");
            else if (productID < 400)
                acheterProduit(user, productID, "Pet");
            else if (productID < 500)
                acheterProduit(user, productID, "Company");
            else if (productID < 600)
                acheterProduit(user, productID, "Gadget");
            else
                Console.WriteLine("Product ID not available!! Press 3- to check all the existed items");
        }

      
        public static void checkWarehouse(XElement user)
        {
            IEnumerable<XElement> items = from x in user.Element("Warehouse").Descendants("myItem")
                                            orderby int.Parse(x.Element("idItem").Value)
                                        select x;
            Console.WriteLine("#ID\tQtt\tName");
            Console.WriteLine("========================================");
            foreach (XElement xE in items)
            {
                Console.Write(xE.Element("idItem").Value + "-\t");
                Console.Write(xE.Element("quantityItem").Value + "\t");
                Console.WriteLine(xE.Element("nameItem").Value);
            }
            Console.WriteLine();
             
        }
        

        public static int lockedOrUnlockedItem(XElement user, XElement item)
        {
            int CurrentLevel = int.Parse(user.Element("GameSettings").Element("Level").Value);

            int neededLevel = int.Parse(item.Element("levelToUnlock").Value);

            if (CurrentLevel < neededLevel)
                return neededLevel;
            return 0;
        }

        public static void affichageStock(string type, XElement user)
        {
            foreach (var elt in xElement.Descendants(type))
            {
                if (elt.Attribute("released_in_stock").Value.Equals("yes"))
                {
                    Console.Write(elt.Attribute(type + "_id").Value + "- \t");
                    string dataName = elt.Element(type + "_Data").Element("name").Value;
                    Console.Write(dataName.PadRight(20));
                    Console.Write(elt.Element("Cost").Element("price").Element("finalPrice").Value + "\t");
                    Console.Write(elt.Element("Cost").Element("price").Attribute("CurrencyUnit").Value + "\t");

                    string dataBrand = elt.Element(type + "_Data").Element("brand").Value;
                    Console.Write(dataBrand.PadRight(14));

                    string dataProvider = elt.Element(type + "_Data").Element("provider").Value;
                    Console.Write(dataProvider.PadRight(12));

                    int neededLevel = lockedOrUnlockedItem(user, elt.Element(type + "_Data"));
                    if (neededLevel != 0)
                        Console.WriteLine("\tLocked Till Level " + neededLevel);
                    else
                        Console.WriteLine();

                }
            }
        }

        public static void displayStock(XElement user)
        {
            Console.WriteLine("SEEDS");
            Console.WriteLine("====================================================================================================");
            affichageStock("Seed", user);

            Console.WriteLine("\nTREES");
            Console.WriteLine("====================================================================================================");
            affichageStock("Tree", user);
    
            Console.WriteLine("\nANIMALS");
            Console.WriteLine("====================================================================================================");
            affichageStock("Pet", user);
        
            Console.WriteLine("\nCOMPANIES");
            Console.WriteLine("====================================================================================================");
            affichageStock("Company", user);
        
            Console.WriteLine("\nACCESSORIES");
            Console.WriteLine("====================================================================================================");
            affichageStock("Gadget", user);
    
            /*
            foreach (var item in xElement.Descendants("name"))
            {
                if (item.Parent.Parent.Attribute("released_in_stock").Value.Equals("yes"))
                {
                    Console.Write(item.Parent.Parent.Attribute("seed_id").Value + "- \t");
                    Console.Write(item.Parent.Parent.Element("Seed_Data").Element("name").Value + "\t");
                    Console.Write(item.Parent.Parent.Element("Cost").Element("price").Element("finalPrice").Value + " ");
                    Console.Write(item.Parent.Parent.Element("Cost").Element("price").Attribute("CurrencyUnit").Value + "\t");
                    Console.Write(item.Parent.Parent.Element("Seed_Data").Element("brand").Value + "\t");
                    Console.WriteLine(item.Parent.Parent.Element("Seed_Data").Element("provider").Value + "\t");
                }
                //Console.Write()
            }
            */
        }

        public static void DisplayWorkToDo()
        {
            Console.WriteLine("\n1- Plant\t2- Harvest\t3- Water");
            Console.WriteLine("4- Unharvest\t5- Move\t\t0- Back\n");
        }

        public static int getUserGardenSize(XElement user)
        {
            int bounds = 0;
            foreach (var level in xElement.Descendants("LevelRule"))
                if ((level.Attribute("id").Value).Equals(user.Element("GameSettings").Element("Level").Value))
                    bounds = int.Parse(level.Element("groundSize").Value);
            return bounds;
        }

        public static void UpdateGardenInfo(XElement user)
        {
            foreach (var item in user.Descendants("pickedItem"))
            {
                if(item.Element("statusItem").Value.Equals("In Progress"))
                {
                    //eza ken plant aw tree & atta3 w2t l s2aye, men 7ota waterneed
                    //eza ken 7ayawen & atta3 w2t l akel, men 7ota feedneed
                    if(int.Parse(item.Element("idPickedItem").Value)>= 100 && int.Parse(item.Element("idPickedItem").Value) < 200)
                    {
                        int needMoreWater = 0;
                        int neededTime = 0;
                        string unityTime = "";
                        int convertFactor = 0;
                        foreach (var p in xElement.Descendants("Seed"))
                        {
                            if(p.Attribute("Seed_id").Value.Equals(item.Element("idPickedItem").Value))
                            {
                                needMoreWater = int.Parse(p.Element("Cost").Element("Other_Costs").Element("WateringCardinal").Value);
                                neededTime = int.Parse(p.Element("Seed_Data").Element("MaturityTime").Value);
                                unityTime = p.Element("Seed_Data").Element("MaturityTime").Attribute("TimeUnit").Value;
                                if (unityTime == "minute") convertFactor = 60;
                                if (unityTime == "hour") convertFactor = 3600;
                            }
                        }
                        if(int.Parse(item.Element("RessourceDone").Value) < needMoreWater)
                        {
                            DateTime introDate = DateTime.ParseExact(item.Element("dateIntro").Value, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                            TimeSpan elapsedSpan = new TimeSpan((long) (DateTime.Now - introDate).Ticks);

                            if (elapsedSpan.TotalSeconds > ((neededTime/needMoreWater)*convertFactor))
                            {
                                item.SetElementValue("statusItem", "WaterNeed");
                                xElement.Save(path);
                            }
                        }
                        else
                        {
                            DateTime introDate = DateTime.ParseExact(item.Element("dateIntro").Value, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                            TimeSpan elapsedSpan = new TimeSpan((long)(DateTime.Now - introDate).Ticks);

                            if (elapsedSpan.TotalSeconds > ((neededTime / needMoreWater) * convertFactor))
                            {
                                item.SetElementValue("statusItem", "Ready");
                                xElement.Save(path);
                            }
                        }
                        
                    }
                }
                if(item.Element("statusItem").Value.Equals("Ready"))
                {
                    //Console.WriteLine(item.Element("dateIntro").Value);
                    
                    DateTime introDate = DateTime.ParseExact(item.Element("dateIntro").Value, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                    
                    long diff = (DateTime.Now - introDate).Ticks;
                    TimeSpan elapTime = new TimeSpan(diff);

                    int TimeExceedToRotten = 0;

                    if (int.Parse(item.Element("idPickedItem").Value) >= 100 && int.Parse(item.Element("idPickedItem").Value) < 200)
                    {
                        foreach (var seed in xElement.Descendants("Seed"))
                        {
                            if (seed.Attribute("Seed_id").Value.Equals(item.Element("idPickedItem").Value))
                            {
                                switch (seed.Element("Seed_Data").Element("OverMaturityTime").Attribute("TimeUnit").Value)
                                {
                                    case "hour":
                                        TimeExceedToRotten = int.Parse(seed.Element("Seed_Data").Element("OverMaturityTime").Value) * 3600;
                                        break;
                                    case "minute":
                                        TimeExceedToRotten = int.Parse(seed.Element("Seed_Data").Element("OverMaturityTime").Value) * 60;
                                        break;
                                }
                            }
                        }
                    }

                    if (int.Parse(item.Element("idPickedItem").Value) >= 200 && int.Parse(item.Element("idPickedItem").Value) < 300)
                    {
                        foreach (var seed in xElement.Descendants("Tree"))
                        {
                            if (seed.Attribute("Tree_id").Value.Equals(item.Element("idPickedItem").Value))
                            {
                                switch (seed.Element("Tree_Data").Element("OverMaturityTime").Attribute("TimeUnit").Value)
                                {
                                    case "hour":
                                        TimeExceedToRotten = int.Parse(seed.Element("Tree_Data").Element("OverMaturityTime").Value) * 3600;
                                        break;
                                    case "minute":
                                        TimeExceedToRotten = int.Parse(seed.Element("Tree_Data").Element("OverMaturityTime").Value) * 60;
                                        break;
                                }
                            }
                        }
                    }


                    if (elapTime.TotalSeconds > (long) TimeExceedToRotten)
                    {
                        item.SetElementValue("statusItem", "Rotten");
                        xElement.Save(path);
                    }
                }
                if(item.Element("statusItem").Value.Equals("Pending"))
                {
                    //eza kholis l pending time, bi sir FeedNeed 
                }
            }
        }
       

        public static void DisplayVisuallyMyGarden(XElement user)
        {
            UpdateGardenInfo(user);

            int bounds = getUserGardenSize(user);
            char[,] garden = new char[bounds, bounds];

            for (int i = 0; i < bounds; i++)
            {
                for (int j = 0; j < bounds; j++)
                {
                    garden[i, j] = '.';
                }
            }

            foreach (var item in user.Descendants("pickedItem"))
            {
                if (item.Element("statusItem").Value.Equals("Pending"))
                    garden[int.Parse(item.Element("x").Value) - 1, int.Parse(item.Element("y").Value) - 1] = '?';
                if (item.Element("statusItem").Value.Equals("Rotten"))
                    garden[int.Parse(item.Element("x").Value) - 1, int.Parse(item.Element("y").Value) - 1] = 'x';
                if (item.Element("statusItem").Value.Equals("Ready"))
                    garden[int.Parse(item.Element("x").Value) - 1, int.Parse(item.Element("y").Value) - 1] = 'D';
                if (item.Element("statusItem").Value.Equals("WaterNeed"))
                    garden[int.Parse(item.Element("x").Value) - 1, int.Parse(item.Element("y").Value) - 1] = 'W';
                if (item.Element("statusItem").Value.Equals("FeedNeed"))
                    garden[int.Parse(item.Element("x").Value) - 1, int.Parse(item.Element("y").Value) - 1] = 'F';
                if (item.Element("statusItem").Value.Equals("Decoration"))
                    garden[int.Parse(item.Element("x").Value) - 1, int.Parse(item.Element("y").Value) - 1] = '=';
                if (item.Element("statusItem").Value.Equals("In Progress"))
                    garden[int.Parse(item.Element("x").Value) - 1, int.Parse(item.Element("y").Value) - 1] = '~';

            }

            for (int i = 0; i < bounds+1; i++)
            {
                Console.Write(i.ToString().PadRight(3));
            }
            Console.WriteLine();

            for (int i = 1; i <= bounds; i++)
            {

                Console.Write(i.ToString().PadRight(3)); 
                for(int j = 0; j < bounds; j++)
                {
                    Console.Write(garden[i-1, j].ToString().PadRight(3));
                }
                Console.WriteLine();
            }
            Console.WriteLine();

        }

        public static char[,] getGarden (XElement user)
        {
            UpdateGardenInfo(user);

            int bounds = getUserGardenSize(user);
            char[,] garden = new char[bounds, bounds];

            for (int i = 0; i < bounds; i++)
            {
                for (int j = 0; j < bounds; j++)
                {
                    garden[i, j] = '.';
                }
            }

            foreach (var item in user.Descendants("pickedItem"))
            {
                if (item.Element("statusItem").Value.Equals("Pending"))
                    garden[int.Parse(item.Element("x").Value) - 1, int.Parse(item.Element("y").Value) - 1] = '?';
                if (item.Element("statusItem").Value.Equals("Rotten"))
                    garden[int.Parse(item.Element("x").Value) - 1, int.Parse(item.Element("y").Value) - 1] = 'x';
                if (item.Element("statusItem").Value.Equals("Ready"))
                    garden[int.Parse(item.Element("x").Value) - 1, int.Parse(item.Element("y").Value) - 1] = 'D';
                if (item.Element("statusItem").Value.Equals("WaterNeed"))
                    garden[int.Parse(item.Element("x").Value) - 1, int.Parse(item.Element("y").Value) - 1] = 'W';
                if (item.Element("statusItem").Value.Equals("FeedNeed"))
                    garden[int.Parse(item.Element("x").Value) - 1, int.Parse(item.Element("y").Value) - 1] = 'F';
                if (item.Element("statusItem").Value.Equals("Decoration"))
                    garden[int.Parse(item.Element("x").Value) - 1, int.Parse(item.Element("y").Value) - 1] = '=';
                if (item.Element("statusItem").Value.Equals("In Progress"))
                    garden[int.Parse(item.Element("x").Value) - 1, int.Parse(item.Element("y").Value) - 1] = '~';

            }
            return garden;
        }

        public static void GoToMyGarden(XElement user)
        {
            //DisplayDetaillyMyGarden(user);
            int option = -1;
            while(option!=0)
            {
                Program.setPlayerLatestSettings(user);
                DisplayVisuallyMyGarden(user);

                DisplayWorkToDo();
                Console.Write("Work to do? > ");

                //ShowAdvice(user);
                string line1 = Console.ReadLine();

                if(int.TryParse(line1, out option))
                {
                    switch(option)
                    {
                        case 1:
                            Plant(user);
                            break;
                        case 2:
                            Harvest(user);
                            break;
                        case 3:
                            Water(user);
                            break;
                        case 4:
                            Unharvest(user);
                            break;
                        case 5:
                            Move(user);
                            break;
                        case 0:
                            break;
                        default:
                            Console.WriteLine("Invalid Operation!");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Stop Playing with letters!\n");

                }
            }
        }

        public static void PlantSectionWork()
        {
            Console.WriteLine("1- Check your warehouse");
            Console.WriteLine("2- Plant/Add item in your garden");
            Console.WriteLine("0- Back");
        }

        public static void Plant(XElement user)
        {
            int option = -1;

            string line1;
            while(option!= 0)
            {
                PlantSectionWork();
                Console.Write("\nWhat to do? > ");
                line1 = Console.ReadLine();
                if(int.TryParse(line1, out option))
                {
                    switch(option)
                    {
                        case 1:
                            Utilities.checkWarehouse(user);
                            break;
                        case 2:
                            Utilities.GetFromWarehouse(user);
                            break;
                        case 0:
                            break;
                        default:
                            Console.WriteLine("Invalid Operation!");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Stop Playing With Characters!!");
                }
            }

        }

        public static bool TheresAPlace(XElement user, int maxLimit)
        {
            if(user.Descendants("pickedItem").Count() < (maxLimit*maxLimit))
                return true;
            return false;
        }

        public static void GetFromWarehouse(XElement user)
        {
            int bounds = getUserGardenSize(user);

            if (TheresAPlace(user, bounds))
            {
                Console.WriteLine("What to plant?\t(press 0 to stop)");
                int option = -1;
                
                while (option != 0)
                {
                    Console.Write("Plant \t> ");
                    string line1 = Console.ReadLine();
                    if (int.TryParse(line1, out option))
                    {
                        if (option == 0)
                            break;

                        if(option >= 1000)
                        {
                            Console.WriteLine("Item can't be planted!");
                            break;
                        }

                        if(option >= 200 && option < 1000)
                        {
                            Console.WriteLine("\nSorry...");
                            Console.WriteLine("Will be released in the next version of the Game");
                            Console.WriteLine("Same \"Seed\" Strategy, but we increase the function calls for each level of growth!");
                            Console.WriteLine("No added value concerning XML goals\n");
                            break;
                        }

                        bool endit = false;
                        int toDeleteLater = 0;

                        bool foundItem = false;
                        bool canPlant = false;

                        foreach (var item in user.Descendants("myItem"))
                        {
                            if (int.Parse(item.Element("idItem").Value) == option)
                            {
                                Console.WriteLine("Choose a coordinate:");
                                Console.Write("\tx: ");
                                string sX = Console.ReadLine();
                                Console.Write("\ty: ");
                                string sY = Console.ReadLine();
                                int x, y;
                                if (int.TryParse(sX, out x))
                                {
                                    if (int.TryParse(sY, out y))
                                    {
                                        if (x >= 1 && x <= bounds)
                                        {
                                            if (y >= 1 && y <= bounds)
                                            {
                                                char[,] ground = getGarden(user);
                                                if (ground[x - 1, y - 1] == '.')
                                                {
                                                    string statement = "";

                                                    if (option >= 500)
                                                        statement = "Decoration";
                                                    else
                                                        if (option >= 300)
                                                        statement = "FeedNeed";
                                                    else
                                                        if (option >= 100)
                                                        statement = "WaterNeed";
                                                    else
                                                        statement = "Decoration";

                                                    int extraXPSeeding = 0;
                                                    int SeedingCost = 0;
                                                    if (option >= 100 && option < 200)
                                                        foreach (var seed in xElement.Descendants("Seed"))
                                                        {
                                                            if (seed.Attribute("Seed_id").Value == option.ToString())
                                                            {
                                                                extraXPSeeding = int.Parse(seed.Element("Incomes").Element("XP_Gain").Element("seeding").Value);
                                                                SeedingCost = int.Parse(seed.Element("Cost").Element("Other_Costs").Element("PlantCost").Value);
                                                            }
                                                        }
                                                    if (int.Parse(user.Element("GameSettings").Element("Energy").Value) >= SeedingCost)
                                                    {
                                                        user.Element("GameSettings").SetElementValue("XP", int.Parse(user.Element("GameSettings").Element("XP").Value) + extraXPSeeding);
                                                        user.Element("GameSettings").SetElementValue("Energy", int.Parse(user.Element("GameSettings").Element("Energy").Value) - SeedingCost);

                                                        user.Element("Garden").Element("usedPlace").Add(new XElement("pickedItem",
                                                            new XElement("idPickedItem", option),
                                                            new XElement("x", x),
                                                            new XElement("y", y),
                                                            new XElement("dateIntro", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt")),
                                                            new XElement("RessourceDone", 0),
                                                            new XElement("statusItem", statement)));
                                                        canPlant = true;

                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("No Energy!!");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Place Already Taken!");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Y out of bounds");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("X out of bounds");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Stop Playin With Characters!! Correct Y");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Stop Playin With Characters!! Correct X");
                                }


                                foundItem = true;

                                if (canPlant)
                                {

                                    int qtt = int.Parse(item.Element("quantityItem").Value);
                                    if (qtt > 1)
                                    {
                                        item.SetElementValue("quantityItem", --qtt);
                                    }
                                    else
                                    {
                                        endit = true;
                                        toDeleteLater = item.ElementsBeforeSelf().Count();
                                    }
                                }

                            }
                        }

                        if (canPlant)
                        {
                            string consequence = foundItem ? "Succesfully Planted!" : "Item Not Found!";
                            Console.WriteLine(consequence);
                        }
                        if (endit)
                        {
                            user.Descendants("myItem").ElementAt(toDeleteLater).Remove();
                        }
                        xElement.Save(path);

                    }
                    else
                    {
                        Console.WriteLine("Stop Playing With Characters!!");
                    }
                }
            }
            else
            {
                Console.WriteLine("No Available Space\n");
            }
        }
        



        public static void AddFriends(XElement user)
        {
            string searchFor = "";
            while (!searchFor.Equals(".."))
            {
                Console.WriteLine("\nSearch for friends... Insert: \t\t(Type .. to get back)");
                Console.Write("\t> ");

                searchFor = Console.ReadLine();
                if (searchFor.Equals(".."))
                    break;

                int count = 0;
                foreach (var alias in xElement.Descendants("alias"))
                {
                    if(alias.Value.ToLowerInvariant().Contains(searchFor.ToLowerInvariant()) || searchFor.ToLowerInvariant().Contains(alias.Value.ToLowerInvariant()))
                    {
                        XElement suggestion = alias.Parent.Parent;
                        if (!suggestion.Attribute("id").Value.Equals(user.Attribute("id").Value))
                        {
                            count++;
                            Console.Write(suggestion.Attribute("id").Value + "- ");
                            Console.Write(alias.Value.PadRight(15));
                            Console.Write(suggestion.Element("bio").Element("email").Value.PadRight(40));
                            Console.WriteLine(suggestion.Element("bio").Element("mes_addresses").Element("addresse").Element("pays").Value);
                        }
                    }
                }
                if(count == 0)
                    Console.WriteLine("No matching friends... Try again please");
                else
                {
                    int selectedID = -1;
                    while(selectedID != 0)
                    {
                        Console.WriteLine("\n\nSend Request to:\t\t(Type 0 to get back)");
                        Console.Write("\t> ");

                        string selectedIDstr = Console.ReadLine();
                        if (selectedIDstr.Equals("0"))
                            break;

                        if (int.TryParse(selectedIDstr, out selectedID))
                        {
                            bool found = false;
                            foreach(var requestedFriend in xElement.Descendants("client"))
                            {
                                if(int.Parse(requestedFriend.Attribute("id").Value) == selectedID)
                                {
                                    found = true;
                                    //bade et2akad eza deja be3etlo men abel aw lah...
                                    //eza lah, eb3atlo request
                                    //eza eh, ello eno deja be3etlo
                                    // l request bet bayin de8re awal ma t3ml sign in... checkForRequests();
                                    //by3ml ya accept ya ignore... eza 3emil accept, bzid ID ba3ed 3ala l friendship
                                    //else bem7e l request
                                    bool isFriend = false;
                                    foreach (var recentFriend in user.Descendants("friend"))
                                    {
                                        if (int.Parse(recentFriend.Attribute("id").Value) == selectedID)
                                        {
                                            Console.WriteLine("Already Added!");
                                            isFriend = true;
                                        }
                                    }

                                    if (!isFriend)
                                    {
                                        bool empty = true;
                                        foreach (var checkForRequest in requestedFriend.Descendants("fromID"))
                                        {
                                            empty = false;
                                            if (checkForRequest.Value.Equals(user.Attribute("id").Value))
                                                Console.WriteLine("Request already sent!");
                                            else

                                            {
                                                requestedFriend.Element("mailBox").Add(new XElement("msg", new XAttribute("type", "F"), new XElement("fromID", user.Attribute("id").Value)));
                                                Console.WriteLine("Request Succesfully Sent");
                                                //Console.WriteLine(requestedFriend.Element("friendshipRequests"));
                                            }
                                        }
                                        if (empty)
                                        {
                                            requestedFriend.Element("mailBox").Add(new XElement("msg", new XAttribute("type", "F"), new XElement("fromID", user.Attribute("id").Value)));
                                            Console.WriteLine("Request Succesfully Sent");
                                            //Console.WriteLine(requestedFriend.Element("friendshipRequests"));
                                        }
                                        xElement.Save(path);
                                    }
                                }
                            }
                            if(!found)
                            {
                                Console.WriteLine("Make sure you have typed an existed ID!!");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Stop Playing With Characters!!");
                        }
                        
                    }
                }
            }
        }


        public static void CheckMailBox(XElement user)
        {
            int count = 0;
            ArrayList waitingToDelete = new ArrayList();

            foreach (var message in user.Descendants("msg"))
            {
                count++;
                switch(message.Attribute("type").Value)
                {
                    case "F":
                        foreach (var source in xElement.Descendants("client"))
                        {
                            if(source.Attribute("id").Value.Equals(message.Element("fromID").Value))
                            {
                                Console.WriteLine(source.Element("avatar").Element("alias").Value + " has sent you a friend request");
                            
                                string ans = "";
                                while (!(ans.Equals("Y") || ans.Equals("y") || ans.Equals("N") || ans.Equals("n") || ans.Equals("L") || ans.Equals("l")))
                                {
                                    Console.Write("Accept? (Y, N, L)\t> ");

                                    ans = Console.ReadLine();
                                    switch (ans)
                                    {
                                        case "Y":
                                        case "y":
                                            addToFriendList(user, source);
                                            waitingToDelete.Add(message.ElementsBeforeSelf().Count());
                                            break;
                                        case "N":
                                        case "n":
                                            waitingToDelete.Add(message.ElementsBeforeSelf().Count());
                                            break;
                                        case "L":
                                        case "l":
                                            break;
                                        default:
                                            Console.WriteLine("Invalid Operation!");
                                            break;
                                    }
                                }
                            }
                        }
                        break;

                    case "G":
                        break;
                    case "I":
                        break;
                }
            }
            if(count == 0)
            {
                Console.WriteLine("Empty MailBox!");
            }

            removeRequest(user, waitingToDelete);
        }


        public static void addToFriendList(XElement user, XElement newFriend)
        {
            user.Element("friendList").Add(new XElement("friend", new XAttribute("id", newFriend.Attribute("id").Value)));
            newFriend.Element("friendList").Add(new XElement("friend", new XAttribute("id", user.Attribute("id").Value)));
            xElement.Save(path);    
        }

        public static void removeRequest(XElement user, ArrayList waitingToDelete)
        {
            for(int i = waitingToDelete.Count - 1; i >= 0; i--)
            {
                user.Descendants("msg").ElementAt((int)waitingToDelete[i]).Remove();
                xElement.Save(path);
            }
        }

        public static void displayFriendList(XElement user)
        {
            int count = 0;
            ArrayList friendID = new ArrayList();
            foreach (var friend in user.Descendants("friend"))
            {
                foreach (var friendUser in xElement.Descendants("client"))
                {
                    if(friend.Attribute("id").Value.Equals(friendUser.Attribute("id").Value))
                    {
                        count++;
                        Console.Write(friendUser.Attribute("id").Value + "- ");
                        friendID.Add(friendUser.Attribute("id").Value);
                        Console.WriteLine(friendUser.Element("avatar").Element("alias").Value);
                    }
                }
            }
            if(count == 0)
            {
                Console.WriteLine("No Added Friends!\n");
            }
            else
            {
                Console.WriteLine("Visit Someone? (Press 0 for No)");
                string strID = Console.ReadLine();
                int idVisit = 0;
                bool isHisFriend = false;
                if(int.TryParse(strID, out idVisit))
                {
                    for(int i = 0; i < friendID.Count; i++)
                    {
                        if(friendID[i].ToString() == idVisit.ToString())
                        {
                            isHisFriend = true;
                            foreach(var visitor in xElement.Descendants("client"))
                            {
                                if(visitor.Attribute("id").Value == idVisit.ToString())
                                {
                                    Console.WriteLine("Welcome to " + visitor.Element("avatar").Element("alias").Value + "'s Garden!!");
                                    DisplayVisuallyMyGarden(visitor);
                                    Console.WriteLine();

                                    string strBack = "";
                                    while(strBack != "0")
                                    {
                                        Console.Write("Press 0 to get Back\t> ");
                                        strBack = Console.ReadLine();
                                        Console.WriteLine();
                                    }
                                }
                            }
                        }
                    }
                    if(!isHisFriend)
                    {
                        Console.WriteLine("This person is not your friend!\n");
                    }
                }
                else
                {
                    Console.WriteLine("Stop Playing with Characters!");
                }
            }

        }
        
        public static void Move(XElement user)
        {
            int length = getUserGardenSize(user);
            char[,] garden = getGarden(user);
            short result = 0;

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    if (garden[i, j] != '.')
                        result = 1;
                }
            }

            switch (result)
            {
                case 0:
                    Console.WriteLine("You haven't planted anything! Can't move anything!!");
                    break;
                case 1:
                        int x = -1;
                        int y = -1;
                        string xstr, ystr;
                        while (x != 0 && y != 0)
                        {
                            Console.WriteLine("What to move?\t(Press 0 0 to finish)");
                            Console.Write("Horizontal > ");
                            xstr = Console.ReadLine();
                            Console.Write("Vertical > ");
                            ystr = Console.ReadLine();
                            if (xstr == "0" && ystr == "0")
                                break;
                            if (int.TryParse(xstr, out x))
                            {
                                if (int.TryParse(ystr, out y))
                                {
                                    if (x > 0 && x <= length)
                                    {
                                        if (y > 0 && y <= length)
                                        {
                                            if (garden[x - 1, y - 1] != '.')
                                            {

                                            foreach (var planted in user.Descendants("pickedItem"))
                                            {
                                                if (planted.Element("x").Value.Equals(xstr))
                                                {
                                                    if (planted.Element("y").Value.Equals(ystr))
                                                    {
                                                        Console.WriteLine("Where to move?\t(Press 0 0 to return it into the warehouse)");
                                                        Console.Write("Horizontal > ");
                                                        xstr = Console.ReadLine();
                                                        Console.Write("Vertical > ");
                                                        ystr = Console.ReadLine();
                                                        if (xstr == "0" && ystr == "0")
                                                        {

                                                            if (garden[x - 1, y - 1] == 'D' || garden[x - 1, y - 1] == 'x')
                                                            {
                                                                if (garden[x - 1, y - 1] == 'D')
                                                                    Console.WriteLine("Harvest item before returning it please");
                                                                else
                                                                    Console.WriteLine("Rotten Item! Please Unharvest it! Can't be returned");
                                                            }
                                                            else
                                                            {
                                                                if (int.Parse(planted.Element("idPickedItem").Value) >= 100 && int.Parse(planted.Element("idPickedItem").Value) < 200)
                                                                {
                                                                    Console.WriteLine("Can't return seeds, wait till it's matured or unharvest it");
                                                                }

                                                                else
                                                                {
                                                                    int indexToReturn = planted.ElementsBeforeSelf().Count();
                                                                    int idElt = int.Parse(planted.Element("idPickedItem").Value);

                                                                    bool foundItemInWarehouse = false;
                                                                    foreach (var checkIfExistItem in user.Descendants("myItem"))
                                                                    {
                                                                        if (checkIfExistItem.Element("idItem").Value == idElt.ToString())
                                                                        {
                                                                            foundItemInWarehouse = true;
                                                                            int oldQtt = int.Parse(checkIfExistItem.Element("quantityItem").Value);
                                                                            oldQtt++;
                                                                            checkIfExistItem.SetElementValue("quantityItem", oldQtt);
                                                                            xElement.Save(path);
                                                                        }
                                                                    }
                                                                    if (!foundItemInWarehouse)
                                                                    {
                                                                        string name = getName(idElt);
                                                                        int prix = getPrice(idElt);

                                                                        user.Element("Warehouse").Add(
                                                                            new XElement("myItem",
                                                                            new XElement("idItem", idElt),
                                                                            new XElement("nameItem", name),
                                                                            new XElement("quantityItem", "1"),
                                                                            new XElement("priceItem", new XAttribute("CurrencyUnit", "CountryCoin"), prix)
                                                                            ));
                                                                        xElement.Save(path);
                                                                    }

                                                                    user.Descendants("pickedItem").ElementAt(indexToReturn).Remove();
                                                                    xElement.Save(path);
                                                                }
                                                                break;
                                                            }
                                                        }
                                                            if (int.TryParse(xstr, out x))
                                                            {
                                                                if (int.TryParse(ystr, out y))
                                                                {
                                                                    if (x > 0 && x <= length)
                                                                    {
                                                                        if (y > 0 && y <= length)
                                                                        {
                                                                            if (garden[x - 1, y - 1] == '.')
                                                                            {
                                                                                planted.SetElementValue("x", x);
                                                                                planted.SetElementValue("y", y);
                                                                                xElement.Save(path);
                                                                            }
                                                                            else
                                                                            {
                                                                                Console.WriteLine("Occuped Place!!");
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            Console.WriteLine("Vertical out of bounds!");
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        Console.WriteLine("Horizontal out of bounds!");
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("Wrong Vertical Input - Stop Playing with characters");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Console.WriteLine("Wrong Horizontal Input - Stop Playing with characters");
                                                            }
                                                        }
                                                    }
                                            }
                                                
                                            }
                                            else
                                            {
                                                Console.WriteLine("No item at ( " + x + " , " + y + " )");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Vertical out of Bound!!");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Horizontal out of Bound!!");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Wrong Vertical Input - Stop Playing with characters");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Wrong Horizontal Input - Stop Playing with characters");
                            }
                        }
                        
                    
                    break;
            }

        }


        public static string getName(int idElt)
        {
            if (idElt >= 100)
            {
                if (idElt - 100 < 100)
                {
                    return incGetName(idElt, "Seed");
                }
                else
                {
                    if (idElt - 100 < 200)
                    {
                        return incGetName(idElt, "Tree");
                    }
                    else
                    {
                        if (idElt - 100 < 300)
                        {
                            return incGetName(idElt, "Pet");
                        }
                        else
                        {
                            if (idElt - 100 < 400)
                            {
                                return incGetName(idElt, "Company");
                            }
                            else
                            {
                                return incGetName(idElt, "Gadget");
                            }
                        }
                    }
                }
            }
            return incGetName(idElt, "specialItem");
        }

        public static string incGetName(int idElt, string type)
        {
                foreach (var nameToGet in xElement.Descendants(type))
                {
                    if (nameToGet.Attribute(type+"_id").Value == idElt.ToString())
                        return nameToGet.Element(type+"_Data").Element("name").Value;
                }
            return "";
         
        }
        public static void Water(XElement user)
        {
            int length = getUserGardenSize(user);
            char[,] garden = getGarden(user);
            short result = 0;

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    if (result!= 1 && garden[i, j] != '.')
                        result = 2;
                    if (garden[i, j] == 'W')
                        result = 1;
                }
            }

            switch (result)
            {
                case 0:
                    Console.WriteLine("You haven't planted anything!");
                    break;
                case 2:
                    Console.WriteLine("You have already Watered everything");
                    break;
                case 1:
                    int x = -1;
                    int y = -1;
                    string xstr, ystr;
                    while(x!=0 && y!=0)
                    {
                        Console.WriteLine("Where to water?\t(Press 0 0 to finish)");
                        Console.Write("Horizontal > ");
                        xstr = Console.ReadLine();
                        Console.Write("Vertical > ");
                        ystr = Console.ReadLine();
                        if (xstr == "0" && ystr == "0")
                            break;
                        if(int.TryParse(xstr, out x))
                        {
                            if(int.TryParse(ystr, out y))
                            {
                                if(x > 0 && x <= length)
                                {
                                    if(y > 0 && y <= length)
                                    {
                                        if (garden[x - 1, y - 1] == 'W')
                                        {

                                            foreach (var planted in user.Descendants("pickedItem"))
                                            {
                                                if (planted.Element("x").Value.Equals(xstr))
                                                {
                                                    if(planted.Element("y").Value.Equals(ystr))
                                                    {
                                                        int neededFarmerEnergy = Need(planted.Element("idPickedItem").Value, "wateringEnergy");
                                                        int availableFarmerEnergy = FarmerEnergy(user);
                                                        int currWater = currentWater(user);
                                                        int neededWater = Need(planted.Element("idPickedItem").Value, "waterCard");
                                                        if (availableFarmerEnergy >= neededFarmerEnergy)
                                                        {
                                                            if (currWater >= neededWater)
                                                            {
                                                                int temp = int.Parse(planted.Element("RessourceDone").Value);

                                                                planted.SetElementValue("RessourceDone", ++temp);
                                                                planted.SetElementValue("statusItem", "In Progress");
                                                                planted.SetElementValue("dateIntro", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt"));
                                                                //bzid XP w bna2eslo energy
                                                                user.Element("GameSettings").SetElementValue("Energy", availableFarmerEnergy - neededFarmerEnergy);
                                                                user.Element("GameSettings").SetElementValue("XP", currentXP(user) + gainXP(planted.Element("idPickedItem").Value, "water"));
                                                                user.Element("GameSettings").SetElementValue("Water", currWater - neededWater);

                                                                xElement.Save(path);
                                                            }
                                                            else
                                                            {
                                                                Console.WriteLine("No Water!!");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("No energy!");
                                                        }
                                                    }
                                                }
                                            }

                                        }
                                        else
                                        {
                                            Console.WriteLine("No Water Need!");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Vertical out of Bound!!");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Horizontal out of Bound!!");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Wrong Vertical Input - Stop Playing with characters");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Wrong Horizontal Input - Stop Playing with characters");
                        }
                    }

                    break;
            }
                
        }

        public static int FarmerEnergy(XElement user)
        {
            return int.Parse(user.Element("GameSettings").Element("Energy").Value);
        }
        public static int Need (string id , string operation)
        {
            if(int.Parse(id)>=100 && int.Parse(id) < 200)
            {
                foreach(var seed in xElement.Descendants("Seed"))
                {
                    if(seed.Attribute("Seed_id").Value == id)
                    {
                        if(operation == "wateringEnergy")
                            return int.Parse(seed.Element("Cost").Element("Other_Costs").Element("WateringCost").Value);
                        if(operation == "waterCard")
                            return int.Parse(seed.Element("Cost").Element("Other_Costs").Element("WaterCost").Value);
                        if (operation == "harvestEnergy")
                            return int.Parse(seed.Element("Cost").Element("Other_Costs").Element("HarvestCost").Value);
                        if (operation == "unharvestEnergy")
                            return int.Parse(seed.Element("Cost").Element("Other_Costs").Element("UnHarvestSeedCost").Value);
                    }
                }
            }
            return 0;
        }

        public static void Unharvest(XElement user)
        {
            int length = getUserGardenSize(user);
            char[,] garden = getGarden(user);
            short result = 0;

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    if (result != 1 && garden[i, j] != '.')
                        result = 2;
                    if (garden[i, j] == 'x')
                        result = 1;
                }
            }

            switch (result)
            {
                case 0:
                    Console.WriteLine("You haven't planted anytthing!!");
                    break;
                case 2:
                    Console.WriteLine("Nothing is rotten for the moment!!");
                    break;
                case 1:
                    int x = -1;
                    int y = -1;
                    string xstr, ystr;
                    while (x != 0 && y != 0)
                    {
                        Console.WriteLine("What to unharvest?\t(Press 0 0 to finish)");
                        Console.Write("Horizontal > ");
                        xstr = Console.ReadLine();
                        Console.Write("Vertical > ");
                        ystr = Console.ReadLine();
                        if (xstr == "0" && ystr == "0")
                            break;
                        if (int.TryParse(xstr, out x))
                        {
                            if (int.TryParse(ystr, out y))
                            {
                                if (x > 0 && x <= length)
                                {
                                    if (y > 0 && y <= length)
                                    {
                                        if (garden[x - 1, y - 1] == 'x')
                                        {
                                            int index = -1;

                                            foreach (var planted in user.Descendants("pickedItem"))
                                            {
                                                if (planted.Element("x").Value.Equals(xstr))
                                                {
                                                    if (planted.Element("y").Value.Equals(ystr))
                                                    {
                                                        int neededFarmerEnergy = Need(planted.Element("idPickedItem").Value, "unharvestEnergy");
                                                        int availableFarmerEnergy = FarmerEnergy(user);
                                                        if (availableFarmerEnergy >= neededFarmerEnergy)
                                                        {

                                                            index = planted.ElementsBeforeSelf().Count();
                                                            //to delete later 
                                                            
                                                            
                                                            //bna2eslo energy
                                                            user.Element("GameSettings").SetElementValue("Energy", availableFarmerEnergy - neededFarmerEnergy);
                                                            
                                                            xElement.Save(path);
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("No energy!");
                                                        }
                                                    }
                                                }
                                            }
                                            if (index != -1)
                                            {
                                                user.Descendants("pickedItem").ElementAt(index).Remove();
                                                xElement.Save(path);
                                            }

                                        }
                                        else
                                        {
                                            Console.WriteLine("It's not rotten!!");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Vertical out of Bound!!");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Horizontal out of Bound!!");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Wrong Vertical Input - Stop Playing with characters");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Wrong Horizontal Input - Stop Playing with characters");
                        }
                    }

                    break;

            }
        }


        public static void Harvest(XElement user)
        {
            int length = getUserGardenSize(user);
            char[,] garden = getGarden(user);
            short result = 0;

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    if (result != 1 && garden[i, j] != '.')
                        result = 2;
                    if (garden[i, j] == 'D')
                        result = 1;
                }
            }

            switch(result)
            {
                case 0:
                    Console.WriteLine("You haven't planted anytthing!!");
                    break;
                case 2:
                    Console.WriteLine("It's not the season yet!!");
                    break;
                case 1:
                    int x = -1;
                    int y = -1;
                    string xstr, ystr;
                    while (x != 0 && y != 0)
                    {
                        Console.WriteLine("What to harvest?\t(Press 0 0 to finish)");
                        Console.Write("Horizontal > ");
                        xstr = Console.ReadLine();
                        Console.Write("Vertical > ");
                        ystr = Console.ReadLine();
                        if (xstr == "0" && ystr == "0")
                            break;
                        if (int.TryParse(xstr, out x))
                        {
                            if (int.TryParse(ystr, out y))
                            {
                                if (x > 0 && x <= length)
                                {
                                    if (y > 0 && y <= length)
                                    {
                                        if (garden[x - 1, y - 1] == 'D')
                                        {
                                            int index = -1;

                                            foreach (var planted in user.Descendants("pickedItem"))
                                            {
                                                if (planted.Element("x").Value.Equals(xstr))
                                                {
                                                    if (planted.Element("y").Value.Equals(ystr))
                                                    {
                                                        int neededFarmerEnergy = Need(planted.Element("idPickedItem").Value, "harvestEnergy");
                                                        int availableFarmerEnergy = FarmerEnergy(user);
                                                        if (availableFarmerEnergy >= neededFarmerEnergy)
                                                        {

                                                            index = planted.ElementsBeforeSelf().Count(); 

                                                            int newID = 1000 + int.Parse(planted.Element("idPickedItem").Value);
                                                            int productionQuantity = 0;
                                                            int newQuantity, baseQuantity = 0;
                                                            int productionPrice = 0;
                                                            string nameProd = "";
                                                            if(int.Parse(planted.Element("idPickedItem").Value) - 100 < 100)
                                                                foreach(var seed in xElement.Descendants("Seed"))
                                                                {
                                                                    if(seed.Attribute("Seed_id").Value == planted.Element("idPickedItem").Value)
                                                                    {
                                                                        productionQuantity = int.Parse(seed.Element("Incomes").Element("HarvestIncome").Value);
                                                                        productionPrice = int.Parse(seed.Element("Incomes").Element("MoneyIncome").Value);
                                                                        nameProd = seed.Element("Seed_Data").Element("name").Value;
                                                                    }
                                                                }

                                                            foreach(var product in user.Descendants("myItem"))
                                                            { 
                                                                if(int.Parse(product.Element("idItem").Value) == newID)
                                                                {
                                                                    baseQuantity = int.Parse(product.Element("quantityItem").Value);
                                                                }
                                                            }
                                                            newQuantity = baseQuantity + productionQuantity;

                                                            if (baseQuantity == 0)
                                                            {
                                                                user.Element("Warehouse").Add(new XElement(
                                                                    new XElement("myItem",
                                                                    new XElement("idItem", newID),
                                                                    new XElement("nameItem", nameProd + " Ready"),
                                                                    new XElement("quantityItem", newQuantity),
                                                                    new XElement("priceItem", new XAttribute("CurrencyUnit", "CountryCoin"), productionPrice))));
                                                            }
                                                            else
                                                            {
                                                                foreach(var item in user.Descendants("myItem"))
                                                                {
                                                                    if(int.Parse(item.Element("idItem").Value) == newID)
                                                                    {
                                                                        item.SetElementValue("quantityItem", newQuantity);
                                                                    }
                                                                }
                                                            }
                                                            
                                                                //bzid XP w bna2eslo energy
                                                                user.Element("GameSettings").SetElementValue("Energy", availableFarmerEnergy - neededFarmerEnergy);
                                                                user.Element("GameSettings").SetElementValue("XP", currentXP(user) + gainXP(planted.Element("idPickedItem").Value, "harvest"));
                                                             
                                                                xElement.Save(path);
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("No energy!");
                                                        }
                                                    }
                                                }
                                            }
                                            if (index != -1)
                                            {
                                                user.Descendants("pickedItem").ElementAt(index).Remove();
                                                xElement.Save(path);
                                            }

                                        }
                                        else
                                        {
                                            Console.WriteLine("It's not the season!!");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Vertical out of Bound!!");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Horizontal out of Bound!!");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Wrong Vertical Input - Stop Playing with characters");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Wrong Horizontal Input - Stop Playing with characters");
                        }
                    }

                    break;

            }
        }


        public static int currentXP(XElement user)
        {
            return int.Parse(user.Element("GameSettings").Element("XP").Value);
        }

        public static int gainXP(string id, string operation)
        {
            if (int.Parse(id) >= 100 && int.Parse(id) < 200)
            {
                foreach (var seed in xElement.Descendants("Seed"))
                {
                    if (seed.Attribute("Seed_id").Value == id)
                    {
                        if (operation == "water")
                            return int.Parse(seed.Element("Incomes").Element("XP_Gain").Element("watering").Value);
                        if(operation == "harvest")
                            return int.Parse(seed.Element("Incomes").Element("XP_Gain").Element("harving").Value);

                    }
                }
            }
            return 0;
        }

        public static int currentWater(XElement user)
        {
            return int.Parse(user.Element("GameSettings").Element("Water").Value);
        }

        public static void GetSystemUpdate()
        {
            DateTime LastEnergyUpdate = DateTime.ParseExact(xElement.Element("GameRules").Element("GameClock").Element("lastEnergyUpdate").Value, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            int EnergyUpdateInterval = 0;
            if (xElement.Element("GameRules").Element("GameClock").Element("refillFarmerEnergy").Attribute("TimeUnit").Value == "minute")
                EnergyUpdateInterval = int.Parse(xElement.Element("GameRules").Element("GameClock").Element("refillFarmerEnergy").Value) * 60;

            DateTime LastWaterUpdate = DateTime.ParseExact(xElement.Element("GameRules").Element("GameClock").Element("lastWaterUpdate").Value, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            int RefillWaterInterval = 0;
            if (xElement.Element("GameRules").Element("GameClock").Element("refillWater").Attribute("TimeUnit").Value == "minute")
                RefillWaterInterval = int.Parse(xElement.Element("GameRules").Element("GameClock").Element("refillWater").Value) * 60;

            TimeSpan elapsedSpanEnergy = new TimeSpan((long)(DateTime.Now - LastEnergyUpdate).Ticks);
            TimeSpan elapsedSpanWater = new TimeSpan((long)(DateTime.Now - LastWaterUpdate).Ticks);


            if (elapsedSpanEnergy.TotalSeconds > EnergyUpdateInterval)
            {
                int addValue = (int)elapsedSpanEnergy.TotalSeconds / (int)EnergyUpdateInterval;
                foreach (var client in xElement.Descendants("client"))
                {
                    int maxEnergy = 0;
                    string level = client.Element("GameSettings").Element("Level").Value;
                    foreach (var rule in xElement.Descendants("LevelRule"))
                    {
                        if(rule.Attribute("id").Value == level)
                        {
                            maxEnergy = int.Parse(rule.Element("FarmerEnergyMax").Value);
                        }
                    }
                    client.Element("GameSettings").SetElementValue("Energy", int.Parse(client.Element("GameSettings").Element("Energy").Value) + addValue);
                    if (int.Parse(client.Element("GameSettings").Element("Energy").Value) > maxEnergy)
                    {
                        client.Element("GameSettings").SetElementValue("Energy", maxEnergy);
                    }
                }
                xElement.Element("GameRules").Element("GameClock").SetElementValue("lastEnergyUpdate", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt"));

            }
            if (elapsedSpanWater.TotalSeconds > RefillWaterInterval)
            {
                int addValue = (int)elapsedSpanWater.TotalSeconds / (int)RefillWaterInterval;
                foreach (var client in xElement.Descendants("client"))
                {

                    int maxWater = 0;
                    string level = client.Element("GameSettings").Element("Level").Value;
                    foreach (var rule in xElement.Descendants("LevelRule"))
                    {
                        if (rule.Attribute("id").Value == level)
                        {
                            maxWater = int.Parse(rule.Element("MaximumWaterContainer").Value);
                        }
                    }
                    client.Element("GameSettings").SetElementValue("Water", int.Parse(client.Element("GameSettings").Element("Water").Value) + addValue);
                    if(int.Parse(client.Element("GameSettings").Element("Water").Value) > maxWater)
                    {
                        client.Element("GameSettings").SetElementValue("Water", maxWater);
                    }
                }
                xElement.Element("GameRules").Element("GameClock").SetElementValue("lastWaterUpdate", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt"));
            }
            xElement.Save(path);
        }

        public static void UpdateUserLevel(XElement user)
        {
            int userXP = int.Parse(user.Element("GameSettings").Element("XP").Value);
            int curLevel = int.Parse(user.Element("GameSettings").Element("Level").Value);
            int tempLevel = 1;
            int maxWater = 0, maxEnergy = 0;

            foreach (var levelRule in xElement.Descendants("LevelRule"))
            {
                if (userXP <= int.Parse(levelRule.Element("levelXPMax").Value))
                {
                    tempLevel = int.Parse(levelRule.Attribute("id").Value);
                    maxWater = int.Parse(levelRule.Element("MaximumWaterContainer").Value);
                    maxEnergy = int.Parse(levelRule.Element("FarmerEnergyMax").Value);
                    break;
                }
            }

            if (tempLevel > curLevel)
            {
                Console.WriteLine("\nCONGRATS!! LEVEL UP!!!\n");
                user.Element("GameSettings").SetElementValue("Water", maxWater);
                user.Element("GameSettings").SetElementValue("Energy", maxEnergy);
                user.Element("GameSettings").SetElementValue("Level", tempLevel);
                xElement.Save(path);
            }
        }

        public static void getProductInfo(int id)
        {
            Console.WriteLine("Time To Mature".PadRight(20) + "Time To Rotten".PadRight(20));
            //18 23 34 8 24 37 - 15

            if (id >= 100 && id < 200)
            {
                foreach(var seed in xElement.Descendants("Seed"))
                {
                    if (int.Parse(seed.Attribute("Seed_id").Value) == id)
                    {
                        tempGetInfo(id, "Seed");
                        
                    }

                }
            }
            else
            {
                if(id>= 200 && id < 300)
                {
                    foreach (var tree in xElement.Descendants("Tree"))
                    {
                        if (int.Parse(tree.Attribute("Tree_id").Value) == id)
                           tempGetInfo(id, "Tree");
                        
                    }
                }
                else
                {
                    if (id >= 300 && id < 400)
                    {
                        foreach (var pet in xElement.Descendants("Pet"))
                        {
                            if (int.Parse(pet.Attribute("Pet_id").Value) == id)
                               tempGetInfo(id, "Tree");    
                        }
                    }
                    else
                    {
                        Console.WriteLine("Information Not Available yet!");
                    }
                }
            }
            Console.WriteLine();
        }

        public static void tempGetInfo(int id, string type)
        {
            foreach(var elt in xElement.Descendants(type))
            {
                if(id == int.Parse(elt.Attribute(type+"_id").Value))
                {
                    if(int.Parse((elt.Element(type + "_Data").Element("MaturityTime").Value)) == 1)
                        Console.Write((elt.Element(type + "_Data").Element("MaturityTime").Value + " " + elt.Element(type + "_Data").Element("MaturityTime").Attribute("TimeUnit").Value).PadRight(20)) ;
                    else
                        Console.Write((elt.Element(type + "_Data").Element("MaturityTime").Value + " " + elt.Element(type + "_Data").Element("MaturityTime").Attribute("TimeUnit").Value + "s").PadRight(20));

                    if(int.Parse(elt.Element(type + "_Data").Element("OverMaturityTime").Value) == 1)
                        Console.WriteLine((elt.Element(type + "_Data").Element("OverMaturityTime").Value + " " + elt.Element(type + "_Data").Element("OverMaturityTime").Attribute("TimeUnit").Value).PadRight(20)) ;
                    else
                        Console.WriteLine((elt.Element(type + "_Data").Element("OverMaturityTime").Value + " " + elt.Element(type + "_Data").Element("OverMaturityTime").Attribute("TimeUnit").Value + "s").PadRight(20));
                }
            }
        }

        public static void SellSection()
        {
            Console.WriteLine("1- Check Warehouse");
            Console.WriteLine("2- Sell a Product");
            Console.WriteLine("0- Back");
        }

        public static void SellProduct(XElement user)
        {

            int option = -1;

            string line1;
            while (option != 0)
            {
                SellSection();
                Console.Write("\nWhat to do? > ");
                line1 = Console.ReadLine();
                if (int.TryParse(line1, out option))
                {
                    switch (option)
                    {
                        case 1:
                            Utilities.checkWarehouse(user);
                            break;
                        case 2:
                            Utilities.SellFromWarehouse(user);
                            break;
                        case 0:
                            break;
                        default:
                            Console.WriteLine("Invalid Operation!");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Stop Playing With Characters!!");
                }
            }

        }

        public static void SellFromWarehouse(XElement user)
        {
            int option = -1;

            Console.WriteLine("What to sell?\t(Press 0 to get back)");
            while(option !=0)
            {
                int indexToDelete = -1;

                bool found = false;
                Console.Write("\t> ");
                string str = Console.ReadLine();
                if (str == "0")
                    break;
                if(int.TryParse(str, out option))
                {
                    foreach (var item in user.Descendants("myItem"))
                    {
                        if(int.Parse(item.Element("idItem").Value) == option)
                        {
                            found = true;
                            int price = 0;
                        
                            if(option < 100)
                            {
                                foreach(var elt in xElement.Descendants("specialItem"))
                                {
                                    if(option == int.Parse(elt.Attribute("specialItem_id").Value))
                                    {
                                        price = int.Parse(elt.Element("Cost").Element("price").Element("finalPrice").Value);
                                    }
                                }
                            }
                            else
                            {
                                if (option < 200)
                                {
                                    foreach (var elt in xElement.Descendants("Seed"))
                                    {
                                        if (option == int.Parse(elt.Attribute("Seed_id").Value))
                                        {
                                            price = int.Parse(elt.Element("Cost").Element("price").Element("finalPrice").Value);
                                        }
                                    }
                                }
                                else
                                {
                                    if (option < 300)
                                    {
                                        foreach (var elt in xElement.Descendants("Tree"))
                                        {
                                            if (option == int.Parse(elt.Attribute("Tree_id").Value))
                                            {
                                                price = int.Parse(elt.Element("Tree").Element("price").Element("finalPrice").Value);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (option < 400)
                                        {
                                            foreach (var elt in xElement.Descendants("Pet"))
                                            {
                                                if (option == int.Parse(elt.Attribute("Pet_id").Value))
                                                {
                                                    price = int.Parse(elt.Element("Cost").Element("price").Element("finalPrice").Value);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (option < 500)
                                            {
                                                foreach (var elt in xElement.Descendants("Company"))
                                                {
                                                    if (option == int.Parse(elt.Attribute("Company_id").Value))
                                                    {
                                                        price = int.Parse(elt.Element("Cost").Element("price").Element("finalPrice").Value);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (option < 600)
                                                {
                                                    foreach (var elt in xElement.Descendants("Gadget"))
                                                    {
                                                        if (option == int.Parse(elt.Attribute("Gadget_id").Value))
                                                        {
                                                            price = int.Parse(elt.Element("Cost").Element("price").Element("finalPrice").Value);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if(option > 1000)
                                                    {
                                                        option %= 1000;
                                                        if(option >= 100 && option < 200)
                                                        {
                                                            foreach (var elt in xElement.Descendants("Seed"))
                                                                if(int.Parse(elt.Attribute("Seed_id").Value) == option)
                                                                    price = int.Parse(elt.Element("Incomes").Element("MoneyIncome").Value);
                                                        }
                                                            
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if(int.Parse(item.Element("quantityItem").Value) == 1)
                            {
                                indexToDelete = item.ElementsBeforeSelf().Count();

                            }
                            else
                            {
                                item.SetElementValue("quantityItem", int.Parse(item.Element("quantityItem").Value) - 1);
                               }
                            user.Element("GameSettings").SetElementValue("Coins", int.Parse(user.Element("GameSettings").Element("Coins").Value)+price);
                            Console.WriteLine("Succesfully sold! New Balance = " + user.Element("GameSettings").Element("Coins").Value + " " + user.Element("GameSettings").Element("Coins").Attribute("CurrencyUnit").Value);

                        }
                    }
                    if(indexToDelete != -1)
                    {
                        user.Descendants("myItem").ElementAt(indexToDelete).Remove();
                    }
                    if (!found)
                        Console.WriteLine("Item not owned!");
                    else
                        xElement.Save(path);
                    
                }
            }
        }

        public static int getPrice(int id)
        {
            if (id< 100)
            {
                foreach (var item in xElement.Descendants("specialItem"))
                {
                    if(item.Attribute("specialItem_id").Value == id.ToString())
                    {
                        return int.Parse(item.Element("Cost").Element("price").Element("finalPrice").Value);
                    }
                }
            }
            else
            {
                if(id < 200)
                {
                    foreach (var item in xElement.Descendants("Seed"))
                    {
                        if (item.Attribute("Seed_id").Value == id.ToString())
                        {
                            return int.Parse(item.Element("Cost").Element("price").Element("finalPrice").Value);
                        }
                    }
                }
                else
                {
                    if(id < 300)
                    {
                        foreach (var item in xElement.Descendants("Tree"))
                        {
                            if (item.Attribute("Tree_id").Value == id.ToString())
                            {
                                return int.Parse(item.Element("Cost").Element("price").Element("finalPrice").Value);
                            }
                        }
                    }
                    else
                    {
                        if(id < 400)
                        {
                            foreach (var item in xElement.Descendants("Pet"))
                            {
                                if (item.Attribute("Pet_id").Value == id.ToString())
                                {
                                    return int.Parse(item.Element("Cost").Element("price").Element("finalPrice").Value);
                                }
                            }
                        }
                        else
                        {
                            if (id < 500)
                            {
                                foreach (var item in xElement.Descendants("Company"))
                                {
                                    if (item.Attribute("Company_id").Value == id.ToString())
                                    {
                                        return int.Parse(item.Element("Cost").Element("price").Element("finalPrice").Value);
                                    }
                                }
                            }
                            else
                            {
                                if(id < 600)
                                {
                                    foreach (var item in xElement.Descendants("Gadget"))
                                    {
                                        if (item.Attribute("Gadget_id").Value == id.ToString())
                                        {
                                            return int.Parse(item.Element("Cost").Element("price").Element("finalPrice").Value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }


        public static string setPass()
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

        //FINALLYY
        public static void AddNewAccount()
        {
            string[] data = new string[9];
            for(int i = 0; i < 9; i++)
            {
                data[i] = "";
            }

            Console.Write("First Name:\t");
            data[0] = Console.ReadLine();
            Console.Write("Last Name:\t");
            data[1] = Console.ReadLine();


            bool found = true;
            int times = 0;
            while (found)
            {
                if (times > 0)
                {
                    Console.WriteLine("Email already used!");
                }

                found = false;
                Console.Write("Email:\t");
                data[2] = Console.ReadLine();
                foreach (var item in xElement.Descendants("email"))
                {
                    if (item.Value == data[2])
                        found = true;
                }
                times++;
            }


            found = true;
            times = 0;
            while (found)
            {
                if(times > 0)
                {
                    Console.WriteLine("Username already exists!");
                }

                found = false;
                Console.Write("Username:\t");
                data[3] = Console.ReadLine();
                foreach(var item in xElement.Descendants("username"))
                {
                    if (item.Value == data[3])
                        found = true;
                }
                times++;
            }


            times = 0;
            while (data[5] != data[4] || (data[5] == "" && data[4] == ""))
            {
                if(times > 0)
                {
                    Console.WriteLine("Wrong Validation!");
                }
                Console.Write("Password:\t\t");
                data[4] = setPass();
                Console.Write("Validate Password:\t");
                data[5] = setPass();
                times++;
            }
            Console.WriteLine("Validation Done!");


            found = false;
            string code = "";
            times = 0;
            while (!found)
            {
                if(times > 0)
                {
                    Console.WriteLine("Invalid Country");
                }
                found = false;
                Console.Write("Country:\t");
                data[6] = Console.ReadLine();
                foreach (var item in xElement.Descendants("item"))
                {
                    if (item.Attribute("name").Value == "code")
                        code = item.Value;
                    if (item.Attribute("name").Value == "country")
                        if (item.Value.ToLower() == data[6].ToLower())
                        {
                            found = true;
                            break;
                        }
                }
                times++;
            }


            Console.Write("Phone:\t" + code + " ");
            data[7] = Console.ReadLine();


            found = true;
            times = 0;
            while (found)
            {
                if(times > 0)
                {
                    Console.WriteLine("Alias already exists!");
                }
                found = false;
                Console.Write("Alias:\t");
                data[8] = Console.ReadLine();
                foreach (var item in xElement.Descendants("alias"))
                {
                    if (item.Value == data[8])
                        found = true;
                }
                times++;
            }



            int newID = 0;
            foreach(var elt in xElement.Descendants("client"))
            {
                if (int.Parse(elt.Attribute("id").Value) > newID)
                    newID = int.Parse(elt.Attribute("id").Value);
            }
            newID++;

            xElement.Element("clients").Add(new XElement("client",
                new XAttribute("id", newID),
                new XElement("bio",
                    new XElement("nom", data[1]),
                    new XElement("prenom", data[0]),
                    new XElement("email", data[2]),
                    new XElement("username", data[3]),
                    new XElement("mot_de_passe", data[4]),
                    new XElement("telephone",
                        new XElement("code", code),
                        new XElement("telephone", data[7])),
                    new XElement("mes_addresses",
                        new XElement("addresse",
                            new XElement("pays", data[6]),
                            new XElement("nomComplet", data[0] + " " + data[1]),
                            new XElement("route",
                                new XElement("numero", ""),
                                new XElement("apartement", ""),
                                new XElement("boitePostal", ""),
                                new XElement("autre", "")),
                            new XElement("village", ""),
                            new XElement("ville", ""),
                            new XElement("etat", ""),
                            new XElement("zip", ""),
                            new XElement("telephone",
                                new XElement("code", code),
                                new XElement("phone", data[7])))),
                            new XElement("photoProfile", new XAttribute("src", ""))),
                new XElement("paymentMethod",
                    new XElement("visa", "")),
                new XElement("avatar",
                    new XElement("alias", data[8]),
                    new XElement("sexe", ""),
                    new XElement("body",
                        new XElement("forme", new XAttribute("id", "")),
                        new XElement("skinColor", new XAttribute("id", "")),
                        new XElement("yeux", new XAttribute("id", "")),
                        new XElement("styleCheveux", new XAttribute("id", ""))),
                    new XElement("outfit",
                        new XElement("chemise", new XAttribute("id", ""), new XAttribute("couleur", "")),
                        new XElement("pantalon", new XAttribute("id", ""), new XAttribute("couleur", "")),
                        new XElement("chaussure", new XAttribute("id", ""), new XAttribute("couleur", ""))),
                    new XElement("armoire",
                        new XElement("etageChemise", ""),
                        new XElement("etagePantalon", ""),
                        new XElement("etageChaussure", "")),
                    new XElement("accessoire",
                        new XElement("collier", new XAttribute("id", "")),
                        new XElement("sacAdos", new XAttribute("id", "")),
                        new XElement("montre", new XAttribute("id", ""))),
                    new XElement("armoireAutre",
                        new XElement("etageCollier", ""),
                        new XElement("etageSacAdos", ""),
                        new XElement("etageMontre", ""))),
                new XElement("GameSettings",
                    new XElement("Level", "1"),
                    new XElement("XP", "0"),
                    new XElement("Coins", new XAttribute("CurrencyUnit", "CountryCoin"), "500"),
                    new XElement("Money", new XAttribute("CurrencyUnit", "Dollar"), "0"),
                    new XElement("Energy", "15"),
                    new XElement("Water", "12")),
                new XElement("Garden",
                    new XElement("usedPlace", "")),
                new XElement("Warehouse",
                    new XElement("myItem",
                        new XElement("idItem", "1"),
                        new XElement("nameItem", "Welcome_Statue"),
                        new XElement("quantityItem", "1"),
                        new XElement("priceItem", new XAttribute("CurrencyUnit", "CountryCoin"), "0"))),
                new XElement("coupons", ""),
                new XElement("mailBox", ""),
                new XElement("friendList", "")));
            
             xElement.Save(path);
            Console.WriteLine("Account Succesfully Created!");
        }


        

        #endregion 
    }
}
