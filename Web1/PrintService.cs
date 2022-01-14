using System;

namespace Web1
{
    public class PrintService
    {
        private int num;

        public PrintService()
        {
            this.num = new Random().Next();
        }

        public void SetNum(int num)
        {
            this.num = num;
        }

        public int GetNum()
        {
            return num;
        }
    }
}
