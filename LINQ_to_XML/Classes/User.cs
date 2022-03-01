using System.Collections.Generic;
using System.Xml.Serialization;

namespace LINQ_to_XML
{
    [XmlRoot]
    public class Users
    {
        [XmlElement]
        public List<User> User { get; set; }
    }
    public class User
    {
        [XmlAttribute]
        public int Id { get; set; }
        
        [XmlElement]
        public string Fullname { get; set; }
       
        [XmlElement]
        public string Likes { get; set; }
      
        [XmlElement]
        public string TotalPosts { get; set; }
        
        [XmlElement]
        public string TotalFollowing { get; set; }
        
        [XmlElement]
        public string TotalFollowers { get; set; }
     
        [XmlElement]
        public string ImageURL { get; set; }
      
        [XmlElement]
        public string City { get; set; }
    }
}
