using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LongBoardsBot.Models.Database
{
    public class User : Entity
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public IEnumerable<LongBoard> BoughtLongBoards { get; set; }
    }
}
