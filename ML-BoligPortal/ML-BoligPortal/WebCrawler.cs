using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;

namespace ML_BoligPortal
{
    public class WebCrawler
    {
        private readonly HttpClient _httpClient;

        public WebCrawler(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<RawProperty>> GatherData()
        {
            var rawProps = new List<RawProperty>();

            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < 741; i++)
            {
                Console.WriteLine($"Page - {i}");

                await this.GatherDataInternal(rawProps, i == 0 ? i : i * 18);
            }

            Console.WriteLine("Website scraped!");
            Console.WriteLine(sw.Elapsed);

            return rawProps;
        }

        private async Task GatherDataInternal(List<RawProperty> properties, int page)
        {
            try
            {
                var baseUrl = "https://www.boligportal.dk/en/rental-properties/";

                if (page > 0)
                {
                    baseUrl = baseUrl + "?offset=" + page;
                }

                var parser = new HtmlParser();
                var response = await _httpClient.GetAsync(baseUrl);
                response.EnsureSuccessStatusCode();
                var html = await response.Content.ReadAsStringAsync();
                var document = await parser.ParseDocumentAsync(html);

                var flexBox = document.QuerySelectorAll(".css-aa1903-Flex-Flex")[1];

                // no more pages
                if (flexBox is null)
                {
                    return;
                }

                foreach (var element in flexBox.Children.Skip(1)) // all elements on the page (18 per page)
                {
                    var singleElementHref = element.QuerySelector("a").Attributes["href"]?.Value;
                    var clickElementResponse = await _httpClient.GetAsync("https://www.boligportal.dk" + singleElementHref);
                    clickElementResponse.EnsureSuccessStatusCode();
                    var clickElementResponseHtml = await clickElementResponse.Content.ReadAsStringAsync();
                    var clickElementDocument = await parser.ParseDocumentAsync(clickElementResponseHtml);
                    var property = new RawProperty();

                    var propertyData = clickElementDocument.QuerySelectorAll(".css-1e9ga0w-Flex-Flex");

                    foreach (var propertyBox in propertyData) // 2 boxes, 1 for property info and 1 for property rental.
                    {
                        foreach (var child in propertyBox.Children.Skip(1)) // Each box has children with key-value pair
                        {
                            var propType = child.QuerySelector(".css-1ymxg01-Text-Text")?.TextContent;
                            var propValue = child.QuerySelector(".css-194pvlz-Text-Text")?.TextContent;

                            // about property
                            if (propType == "Property type")
                            {
                                property.PropertyType = propValue;
                            }
                            else if (propType == "Size")
                            {
                                var realValue = propValue.Split(' ')[0];
                                var canParse = int.TryParse(realValue, out int r);

                                if (!canParse)
                                {
                                    r = ParseWeirdFloatNumbersToIntegers(realValue);
                                }

                                property.Size = r;
                            }
                            else if (propType == "Rooms")
                            {
                                var canParse = int.TryParse(propValue, out int r);

                                if (!canParse)
                                {
                                    r = ParseWeirdFloatNumbersToIntegers(propValue);
                                }

                                property.Rooms = r;
                            }
                            else if (propType == "Floor")
                            {
                                var value = string.Empty;

                                if (propValue == "Ground floor" || propValue == "-")
                                {
                                    value = "0";
                                }
                                else if (propValue == "Basement")
                                {
                                    value = "-1";
                                }
                                else
                                {
                                    value = propValue.Substring(0, 2);
                                    var canParse = int.TryParse(value, out int r);

                                    if (!canParse)
                                    {
                                        value = propValue.Substring(0, 1);
                                    }
                                }

                                property.Floor = int.Parse(value);
                            }
                            else if (propType == "Furnished")
                            {
                                property.Furnished = ParseYesNoToBoolean(propValue);
                            }
                            else if (propType == "Shareable")
                            {
                                property.Shareable = ParseYesNoToBoolean(propValue);
                            }
                            else if (propType == "Pets allowed")
                            {
                                property.PetsAllowed = ParseYesNoToBoolean(propValue);
                            }
                            else if (propType == "Elevator")
                            {
                                property.Elevator = ParseYesNoToBoolean(propValue);
                            }
                            else if (propType == "Senior friendly")
                            {
                                property.SeniorFriendly = ParseYesNoToBoolean(propValue);
                            }
                            else if (propType == "Student only")
                            {
                                property.StudentOnly = ParseYesNoToBoolean(propValue);
                            }
                            else if (propType == "Balcony")
                            {
                                property.Balcony = ParseYesNoToBoolean(propValue);
                            }
                            else if (propType == "Parking")
                            {
                                property.Parking = ParseYesNoToBoolean(propValue);
                            }
                            // about rental
                            else if (propType == "Rental period")
                            {
                                property.RentalPeriod = propValue;
                            }
                            else if (propType == "Available from")
                            {
                                property.AvailableFrom = propValue;
                            }
                            else if (propType == "Monthly net rent")
                            {
                                property.MonthlyNetRent = ParseDanishNumbersToInt(propValue);
                            }
                            else if (propType == "Utilities")
                            {
                                property.Utilities = ParseDanishNumbersToInt(propValue);
                            }
                            else if (propType == "Deposit")
                            {
                                property.Deposit = ParseDanishNumbersToInt(propValue);
                            }
                            else if (propType == "Prepaid rent")
                            {
                                property.PrepaidRent = ParseDanishNumbersToInt(propValue);
                            }
                            else if (propType == "Move-in price")
                            {
                                property.MoveInPrice = ParseDanishNumbersToInt(propValue);
                            }
                        }
                    }

                    var properyDistrictSelector = clickElementDocument.QuerySelectorAll(".css-76suba-Text-Text")[1]?.TextContent;
                    var districtText = ParseDistrictInput(properyDistrictSelector);
                    property.District = districtText;

                    properties.Add(property);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong!");
                Console.WriteLine(ex);
                Console.WriteLine(ex.Message);
                Console.WriteLine("Current properties: " + properties.Count);
            }
        }

        private static string ParseDistrictInput(string input)
        {
            var splitInput = input.Split(new [] { ',', '-' });
            return splitInput[0].Trim() + "," + splitInput[1].Trim();
        }

        private static bool ParseYesNoToBoolean(string input)
        {
            return input == "Yes";
        }

        private static int ParseDanishNumbersToInt(string input)
        {
            var split = input.Split('-', ',');
            return int.Parse(split[0].Replace(".", string.Empty));
        }

        private static int ParseWeirdFloatNumbersToIntegers(string input)
        {
            var parseDouble = double.Parse(input);
            return int.Parse(Math.Ceiling(parseDouble).ToString());
        }
    }
}
