using LongBoardsBot.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LongBoardsBot.Models.Entities.StatisticsStage;

namespace LongBoardsBot.Helpers
{
    public static class StatisticsStageHelper
    {  
        public static StatisticsStage GetNext(this StatisticsStage current)
        {
            switch (current)
            {
                case None:
                    return None;
                case Age:
                    return WorkingOrStudying;
                case WorkingOrStudying:
                    return Profession;
                case Profession:
                    return Hobby;
                case Hobby:
                    return None;
                default:
                    return None;
            }
        }
    }
}
