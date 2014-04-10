using ClientConsole.Tests;

namespace ClientConsole
{
    internal class SampleApp
    {
        private static void Main(string[] args)
        {
            //new TransactionsTests().run();

            new FreezeTests().Run();

            //new TeacherTests().run();
        }
    }
}