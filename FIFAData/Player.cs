using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFAData
{
    public class Player
    {
        public string Name { get; set; }

        public string Face { get; set; }

        public decimal OverallScore { get; set; }

        public bool IsNew { get; set; }

        public bool IsPlayerBanned { get; set; }

        public bool SomeOtherFakeCondition { get; set; }

        public static List<Player> Participants
        {
            get
            {
                return new List<Player>()
                {
                    { new Player { Name = "Dave M", Face = ":dave:", OverallScore = 10.43m } },
                    { new Player { Name = "Dom", Face = ":dom:" , OverallScore = 62.89m} },
                    { new Player { Name = "Matt N", Face = ":matt:" , OverallScore = 51.53m} },
                    { new Player { Name = "Liam", Face = ":liam:" , OverallScore = 74.50m} },
                    { new Player { Name = "James", Face = ":james:" , OverallScore = 15.38m} },
                    { new Player { Name = "Louie", Face = ":louie:" , OverallScore = 45.09m} },
                    { new Player { Name = "Matt W", Face = ":mattw:" , OverallScore = 64.21m} },
                    { new Player { Name = "Tristan", Face = ":tristan:" , OverallScore = 38.78m} },
                    { new Player { Name = "Ash", Face = ":ash:" , OverallScore = 61.97m} },
                    { new Player { Name = "Craig", Face = ":craig:" , OverallScore = 57.26m} },
                    { new Player { Name = "Dave B", Face = ":daveb:" , OverallScore = 15.38m } },
                    { new Player { Name = "Neil", Face = ":neil:" , OverallScore = 0, } },
                    { new Player { Name = "Luke", Face = ":luke:" , OverallScore = 23.08m, } },
                    { new Player { Name = "Moggmeister", Face = ":mogg:", OverallScore = 0 , IsNew = true} },
                    { new Player { Name = "Jakub", Face = ":jakub:", OverallScore = 0 , IsNew = true} },
                };
            }
        }

    }
}
