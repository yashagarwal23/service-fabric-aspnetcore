using System;

namespace Web1
{
    public interface IService
    {
        string Print();
    }

    public class PrintService : IService
    {
        int num;

        public PrintService()
        {
            Console.WriteLine("constructed");
            num = new Random().Next();
        }

        public string Print()
        {
            return num.ToString();
        }
    }
}
