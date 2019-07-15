namespace TelegramBot_SampleFunctionalities
{
    public enum Stage
    {
        AskingName = 0,
        GettingName = 1,
        GettingPhone = 2,
        ProcessingLongboardsKeyboardInput = 3,
        ProcessingBasketKeyboardInput = 4,
        AskingIfShouldContinueAddingToBasket = 5,
        ShouldRestartDialog = 6
    }
}
