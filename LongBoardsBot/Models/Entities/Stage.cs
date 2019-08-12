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
        GettingShouldDeliverToHomeOrNot = 8,
        GettingHomeAdress = 9,
        ProcessingWantsToComment = 10,
        TypingComment = 11,
        ReceivingIsLivingInKharkivOrNot = 12,
        ReceivingDoesWantToTypeStatistics = 13,
        ReceivingMenuItem = 14,
        ReceivingDateOfVisit = 15
    }
}
