using System.Data;

namespace GFDA
{
    public partial class Doll
    {
        private string SetCountry(DataRow dr)
        {
            return ((string)dr["Country"]) switch
            {
                "Australia" => ETC.Resources.GetString(Resource.String.Country_Australia),
                "Austria" => ETC.Resources.GetString(Resource.String.Country_Austria),
                "Belgium" => ETC.Resources.GetString(Resource.String.Country_Belgium),
                "BlazeBlue" => ETC.Resources.GetString(Resource.String.Country_BlazeBlue),
                "Brazil" => ETC.Resources.GetString(Resource.String.Country_Brazil),
                "Bulgaria" => ETC.Resources.GetString(Resource.String.Country_Bulgaria),
                "China" => ETC.Resources.GetString(Resource.String.Country_China),
                "Croatia" => ETC.Resources.GetString(Resource.String.Country_Croatia),
                "Czech" => ETC.Resources.GetString(Resource.String.Country_Czech),
                "Czechoslovakia" => ETC.Resources.GetString(Resource.String.Country_Czechoslovakia),
                "DJMAX RESPECT" => ETC.Resources.GetString(Resource.String.Country_DJMAXRESPECT),
                "Finland" => ETC.Resources.GetString(Resource.String.Country_Finland),
                "France" => ETC.Resources.GetString(Resource.String.Country_France),
                "Germany" => ETC.Resources.GetString(Resource.String.Country_Germany),
                "Guilty Gear" => ETC.Resources.GetString(Resource.String.Country_GuiltyGear),
                "Honkai 2" => ETC.Resources.GetString(Resource.String.Country_Honkai2),
                "Hungary" => ETC.Resources.GetString(Resource.String.Country_Hungary),
                "Israel" => ETC.Resources.GetString(Resource.String.Country_Israel),
                "Italy" => ETC.Resources.GetString(Resource.String.Country_Italy),
                "Japan" => ETC.Resources.GetString(Resource.String.Country_Japan),
                "Poland" => ETC.Resources.GetString(Resource.String.Country_Poland),
                "Republic of Korea" => ETC.Resources.GetString(Resource.String.Country_RepublicOfKorea),
                "Republic of South Africa" => ETC.Resources.GetString(Resource.String.Country_RepublicOfSouthAfrica),
                "Russia" => ETC.Resources.GetString(Resource.String.Country_Russia),
                "Serbia" => ETC.Resources.GetString(Resource.String.Country_Serbia),
                "Singapore" => ETC.Resources.GetString(Resource.String.Country_Singapore),
                "Soviet Union" => ETC.Resources.GetString(Resource.String.Country_SovietUnion),
                "Spain" => ETC.Resources.GetString(Resource.String.Country_Spain),
                "Sweden" => ETC.Resources.GetString(Resource.String.Country_Sweden),
                "Switzerland" => ETC.Resources.GetString(Resource.String.Country_Switzerland),
                "Taiwan" => ETC.Resources.GetString(Resource.String.Country_Taiwan),
                "United Kingdom" => ETC.Resources.GetString(Resource.String.Country_UnitedKingdom),
                "United States of America" => ETC.Resources.GetString(Resource.String.Country_UnitedStatesOfAmerica),
                _ => "Unknown",
            };
        }

    }
}