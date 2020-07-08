using System;
using System.Messaging;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                string qName = @".\private$\queueexchange2;subqueue";
                if (!MessageQueue.Exists(qName))
                {
                    using (MessageQueue t = MessageQueue.Create(qName))
                    {
                        Console.WriteLine($"Queue Created for {qName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            Console.ReadKey();

        }
    }
}
