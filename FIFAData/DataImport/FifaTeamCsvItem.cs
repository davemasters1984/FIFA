using FIFA.Model;
using FileHelpers;

namespace FIFAData
{
    [DelimitedRecord(",")]
    public class TeamCsvItem
    {
        [FieldConverter(ConverterKind.Decimal, ".")]
        public decimal Stars;

        public string LogoUrl;

        public string TeamName;

        public string League;

        [FieldConverter(ConverterKind.Int32)]
        public int OverallRating;

        public Team ToTeam()
        {
            return new Team
            {
                Stars = this.Stars,
                LogoUrl = this.LogoUrl,
                TeamName = this.TeamName,
                League = this.League,
                OverallRating = this.OverallRating
            };
        }
    }
}
