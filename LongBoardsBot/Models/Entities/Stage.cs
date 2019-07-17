namespace LongBoardsBot.Models.Entities
{
    /// <summary>
    /// Unordered! (starting from 7 inclusively)
    /// </summary>
    public enum Stage
    {
        AskingName = 0,
        GettingName = 1,
        GettingPhone = 2,
        WhatLongBoard = 3,
        ShouldAddLongboardToBasket = 4,
        ShouldContinueAddingToBasket = 5,
        ShouldRestartDialog = 6,
        HowManyLongboards = 7,
        GettingShouldDeliverToHomeOrNot,
        GettingHomeAdress
    }
}
