using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace NBPCurrencyReader
{
    public class Value
    {
        private readonly string _currency;
        private readonly string _startDate;
        private readonly string _endDate;
        public IList<double> PurchasePriceList { get; private set; }
        public IList<double> SellingPriceList { get; private set; }
        private IList<string> _dateList;
        private IEnumerable<string> _requiredDateList;
        public double AverageRateOfPurchasePrices { get; set; }
        public double StandardDeviationOfSellingPrices { get; set; }

        public Value(string currency, string startDate, string endDate)
        {
            _currency = currency;
            _startDate = @"c\w{4}" + DateToString(startDate);
            _endDate = @"c\w{4}" + DateToString(endDate);
            PurchasePriceList = new List<double>();
            SellingPriceList = new List<double>();
            _dateList = new List<string>();
            AverageRateOfPurchasePrices = 0;
            StandardDeviationOfSellingPrices = 0;
        }

        public void AddToPurchasePriceList(double item)
        {
            PurchasePriceList.Add(item);
        }

        public void AddToSellingPriceList(double item)
        {
            SellingPriceList.Add(item);
        }

        public void AddToDateList(string date)
        {
            _dateList.Add(date);
        }

        public void DownloadPurchaseAndSellingPrice()
        {
            foreach (var item in _dateList)
            {
                string urlToFirstDate = "http://www.nbp.pl/kursy/xml/" + item + ".xml";

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Ignore;

                using (XmlReader reader = XmlReader.Create(urlToFirstDate, settings))
                {
                    reader.ReadToFollowing("kod_waluty");
                    while (_currency != reader.ReadElementContentAsString())
                    {
                        reader.ReadToFollowing("kod_waluty");
                    }
                    reader.ReadToFollowing("kurs_kupna");
                    AddToPurchasePriceList(double.Parse(reader.ReadElementContentAsString()));
                    reader.ReadToFollowing("kurs_sprzedazy");
                    AddToSellingPriceList(double.Parse(reader.ReadElementContentAsString()));
                }
            }
        }

        private string DateToString(string date)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(date[6]);
            builder.Append(date[7]);
            builder.Append(date[3]);
            builder.Append(date[4]);
            builder.Append(date[0]);
            builder.Append(date[1]);
            return builder.ToString();
        }

        public void ParseRequiredNameFiles()
        {
            bool inAMiddle = false;

            foreach (var date in _requiredDateList)
            {
                if (Regex.IsMatch(date, _startDate) && inAMiddle == false)
                {
                    inAMiddle = true;
                }
                if (inAMiddle)
                {
                    AddToDateList(date);
                }
                if (Regex.IsMatch(date, _endDate))
                {
                    inAMiddle = false;
                    break;
                }
            }
        }

        public void DownloadAllNameFiles()
        {
            var wc = new WebClient();
            string data = wc.DownloadString("http://www.nbp.pl/kursy/xml/dir.txt");
            string[] dataStrings = Regex.Split(data, "\r\n");
            _requiredDateList = dataStrings.ToList();
            _requiredDateList = _requiredDateList.Where(x => Regex.IsMatch(x, @"c\w{4}" + @"\d{6}")); //nazwy plików tylko"c"
        }

        public double CalculateAverageRateOfPurchasePrices()
        {
            return PurchasePriceList.Average();
        }

        public double CalculateStandardDeviationOfSellingPrices()
        {
            double average = SellingPriceList.Average();
            double sumOfSquaresOfDifferences = SellingPriceList.Select(x => (x - average) * (x - average)).Sum();
            return Math.Sqrt(sumOfSquaresOfDifferences / SellingPriceList.Count);
        }
    }
}