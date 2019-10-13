using Microsoft.AspNetCore.Mvc;

namespace LongBoardsBot.Helpers
{
    public static class StringExtensions
    {
        public static string AddControllerName(this string value, ControllerBase controller)
        {
            const string BaseClassEnding = "Base";

            var derivedClassName = controller.GetType().Name;
            var baseClassName = typeof(ControllerBase).Name;
            var commonEndingForDerived = baseClassName.Remove(BaseClassEnding); 
            var controllerName = derivedClassName.Remove(commonEndingForDerived);

            return value + controllerName;
        }
    }
}