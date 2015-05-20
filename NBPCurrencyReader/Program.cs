using System;

namespace NBPCurrencyReader
{
    class Program
    {
        static void Main(string[] args)
        {
            string start;
            string end;
            string currency;

            //Take data from user
            Console.WriteLine("NBP Currency Reader v1.0");
            Console.WriteLine("Enter currency(USD, EUR, CHF, GBP):");
            currency = Console.ReadLine();
            Console.WriteLine("Enter start date(dd-mm-rr):");
            start = Console.ReadLine();
            Console.WriteLine("Enter end date(dd-mm-rr):");
            end = Console.ReadLine();

            //Create major object
            var value = new Value(currency, start, end);

            value.DownloadAllNameFiles();
            value.ParseRequiredNameFiles();
            value.DownloadPurchaseAndSellingPrice();

            Console.WriteLine("Average: {0:N4}", value.CalculateAverageRateOfPurchasePrices());
            Console.WriteLine("Standard deviation: {0:N4}", value.CalculateStandardDeviationOfSellingPrices());
            Console.Read();
        }
    }
}
