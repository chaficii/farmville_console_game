using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LINQ_to_XML
{
    public enum SexEnum
    {
        Male,
        Female
    };

    public class Phone
    {
        [XmlAttribute]
        public String Type { get; set; }
        [XmlText]
        public String Number { get; set; }
    }

    public class Address
    {
        [XmlElement]
        public string Street { get; set; }
        [XmlElement]
        public string City { get; set; }
        [XmlElement]
        public string State { get; set; }
        [XmlElement]
        public string Zip { get; set; }
        [XmlElement]
        public string Country { get; set; }
    }

    public class Employee
    {
        [XmlElement]
        public int EmpId { get; set; }
        [XmlElement]
        public String Name { get; set; }
        [XmlElement]
        public SexEnum Sex { get; set; }
        [XmlElement]
        public List<Phone> Phones { get; set; }
        [XmlElement]
        public Address Address { get; set; }
    }

    [XmlRoot] 
    public class Employees
    {
        [XmlElement]
        public List<Employee> Employee { get; set; }
    }
}
