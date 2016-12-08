using FileHelpers;

namespace FIFAData
{
    [DelimitedRecord(",")]
    public class FifaTeamCsvItem
    {
        [FieldConverter(ConverterKind.Decimal, ".")]
        public decimal Stars;

        public string LogoUrl;

        public string TeamName;

        public string League;

        [FieldConverter(ConverterKind.Int32)]
        public int OverallRating;

        public FifaTeam ToFifaTeam()
        {
            return new FifaTeam
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
