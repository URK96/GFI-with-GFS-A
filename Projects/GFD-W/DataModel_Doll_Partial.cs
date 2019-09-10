using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Net;

namespace GFD_W
{
    public partial class Doll
    {
        private string SetCountry(ref DataRow dr)
        {
            /*switch ((string)dr["Country"])
            {
                case "Australia":
                    return ETC.Resources.GetString(Resource.String.Country_Australia);
                case "Austria":
                    return ETC.Resources.GetString(Resource.String.Country_Austria);
                case "Belgium":
                    return ETC.Resources.GetString(Resource.String.Country_Belgium);
                case "BlazeBlue":
                    return ETC.Resources.GetString(Resource.String.Country_BlazeBlue);
                case "Brazil":
                    return ETC.Resources.GetString(Resource.String.Country_Brazil);
                case "Bulgaria":
                    return ETC.Resources.GetString(Resource.String.Country_Bulgaria);
                case "China":
                    return ETC.Resources.GetString(Resource.String.Country_China);
                case "Croatia":
                    return ETC.Resources.GetString(Resource.String.Country_Croatia);
                case "Czech":
                    return ETC.Resources.GetString(Resource.String.Country_Czech);
                case "Czechoslovakia":
                    return ETC.Resources.GetString(Resource.String.Country_Czechoslovakia);
                case "DJMAX RESPECT":
                    return ETC.Resources.GetString(Resource.String.Country_DJMAXRESPECT);
                case "Finland":
                    return ETC.Resources.GetString(Resource.String.Country_Finland);
                case "France":
                    return ETC.Resources.GetString(Resource.String.Country_France);
                case "Germany":
                    return ETC.Resources.GetString(Resource.String.Country_Germany);
                case "Guilty Gear":
                    return ETC.Resources.GetString(Resource.String.Country_GuiltyGear);
                case "Honkai 2":
                    return ETC.Resources.GetString(Resource.String.Country_Honkai2);
                case "Hungary":
                    return ETC.Resources.GetString(Resource.String.Country_Hungary);
                case "Israel":
                    return ETC.Resources.GetString(Resource.String.Country_Israel);
                case "Italy":
                    return ETC.Resources.GetString(Resource.String.Country_Italy);
                case "Japan":
                    return ETC.Resources.GetString(Resource.String.Country_Japan);
                case "Poland":
                    return ETC.Resources.GetString(Resource.String.Country_Poland);
                case "Republic of Korea":
                    return ETC.Resources.GetString(Resource.String.Country_RepublicOfKorea);
                case "Republic of South Africa":
                    return ETC.Resources.GetString(Resource.String.Country_RepublicOfSouthAfrica);
                case "Russia":
                    return ETC.Resources.GetString(Resource.String.Country_Russia);
                case "Serbia":
                    return ETC.Resources.GetString(Resource.String.Country_Serbia);
                case "Singapore":
                    return ETC.Resources.GetString(Resource.String.Country_Singapore);
                case "Soviet Union":
                    return ETC.Resources.GetString(Resource.String.Country_SovietUnion);
                case "Spain":
                    return ETC.Resources.GetString(Resource.String.Country_Spain);
                case "Sweden":
                    return ETC.Resources.GetString(Resource.String.Country_Sweden);
                case "Switzerland":
                    return ETC.Resources.GetString(Resource.String.Country_Switzerland);
                case "Taiwan":
                    return ETC.Resources.GetString(Resource.String.Country_Taiwan);
                case "United Kingdom":
                    return ETC.Resources.GetString(Resource.String.Country_UnitedKingdom);
                case "United States of America":
                    return ETC.Resources.GetString(Resource.String.Country_UnitedStatesOfAmerica);
                default:
                    return "Unknown";
            }*/

            return null;
        }

    }
}