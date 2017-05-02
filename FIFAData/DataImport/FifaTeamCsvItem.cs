using FIFA.Model;
using FileHelpers;

namespace FIFAData
{
    [DelimitedRecord(",")]
    public class TeamCsvItem
    {
        public string LogoUrl;

        public string TeamName;

        public string League;

        [FieldConverter(ConverterKind.Int32)]
        public int OverallRating;

        public Team ToTeam()
        {
            return new Team
            {
                LogoUrl = this.LogoUrl,
                TeamName = this.TeamName,
                League = this.League,
                OverallRating = this.OverallRating
            };
        }
    }
}
