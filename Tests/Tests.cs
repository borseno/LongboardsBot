using LongBoardsBot.Controllers;
using LongBoardsBot.Helpers;
using Xunit;

namespace Tests
{
    public class Tests
    {
        [Theory]
        [InlineData("blablabla", "blablablaMessage")]
        [InlineData("33", "33Message")]
        [InlineData("", "Message")]
        [InlineData("ahhha", "ahhha" + "Message")]
        public void AddControllerName_ShouldAddMessageToGivenString
            (string initValue, string expected)
        {
            string result = initValue.AddControllerName(new MessageController());

            Assert.Equal(expected, result);
        }
    }
}
