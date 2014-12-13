using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Rad
{
    class XMLParserClass
    {
        private XElement XMLDocElements;
        
        public XMLParserClass(string XMLDocumentName)
        {
            try
            {
                XMLDocElements = XElement.Load(XMLDocumentName);
            }
            catch(Exception e)
            {
                
            }
            
        }

        public void ReadCitiesInProvince(string ProvinceName, List<string> CityNameList)
        {
            if(XMLDocElements != null)
            {
                IEnumerable<XElement> ProvinceNode =
                    from EnumProv in XMLDocElements.Elements("Province")
                    where (string)EnumProv.Attribute("Name") == ProvinceName
                    select EnumProv;

                IEnumerable<XElement> Cities =
                    from EnumCity in ProvinceNode.Descendants()
                    select EnumCity;

                foreach (XElement EnumCity in Cities)
                {

                    string CityName = EnumCity.Value;
                    CityNameList.Add(CityName);
                    CityNameList.Sort();
                }
            }
            
        }

        public string GetCityCode(string CityName)
        {
            XElement CityCode;
            
            if (XMLDocElements != null)
            {
                IEnumerable<XElement> CityNode =
                    from EnumProv in XMLDocElements.Elements("City")
                    where (string)EnumProv.Attribute("Name") == CityName
                    select EnumProv;

                CityCode = CityNode.First();
                return CityCode.Value.ToString();
            }

            return "Failed";
        }
    }
}
