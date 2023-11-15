// See https://aka.ms/new-console-template for more information
using System.Formats.Asn1;
using System.Globalization;
using System;
using CsvHelper;
using System.Net;
using System.Collections.Generic;

List<NamedInsuredInput> records;
var output = new List<NamedInsuredOutput>();

using (var reader = new StreamReader("../../../problem-input.csv"))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
	records = csv.GetRecords<NamedInsuredInput>().ToList();
}

var makeAndModelMap =  records.GroupBy(x => x.VehicleMake.ToLower() + "#" + x.VehicleColor.ToLower())
							  .ToDictionary(x=> x.Key, x => x.Count());
var firstNameMap = records.GroupBy(x => x.FirstName.ToLower())
						  .ToDictionary(x => x.Key, x => x.Count());

foreach (var record in records)
{
	double newPremium = double.Parse(record.CurrentMonthlyPremiumCents);


	// if their address's street number is even, add $7 to their premium. If it is odd, add $8.
	var addressNumber = double.Parse(record.AddressLine1.Split(' ')[0]);
	newPremium += addressNumber % 2 == 0 ? 700.00: 800.00;


	// if they have a .edu email address, discount the premium by 25 %; otherwise, add $5.25 to their premium
	newPremium = record.Email.ToLower().EndsWith(".edu") ? newPremium *= .75 : newPremium += 525.00;

	// if their car is their favorite color, subtract $1.25 from their premium
	if (record.FavoriteColor.ToLower() == record.VehicleColor.ToLower())
	{
		newPremium -= 125.00;
	}

	// if their last name has an uppercase or lowercase T in it, subtract $3.29 from their premium
	if (record.LastName.ToLower().Contains("t")){
		newPremium -= 329.00;
	}

	//add the first two digits of their zip code, as cents, to their premium; subtract the last two digits of their zip code, as dollars, from their premium
	newPremium += double.Parse(record.Zip.Remove(2));
	newPremium -= double.Parse(record.Zip.Remove(0,record.Zip.Length - 2)) * 100.00;

	//add the greatest digit of their car's model year to their premium, as dollars
	newPremium += double.Parse(record.VehicleYear.ToCharArray().OrderByDescending(x => x).First().ToString()) * 100.00;

	// if no other policy holder has the same make and color car, add 50% to their premium
	if (makeAndModelMap[record.VehicleMake.ToLower() + "#" + record.VehicleColor.ToLower()] == 1)
	{
		newPremium *= 1.5;
	}

	// if any other policy holder has the same first name, half the premium
	if (firstNameMap[record.FirstName.ToLower()] > 1) 
	{
		newPremium *= .5;
	}

	// round positive premiums down to the nearest cent; round negative premiums up to the nearest cent
	var outputRecord = new NamedInsuredOutput(record.CustomerID, ((int)newPremium).ToString());

	output.Add(outputRecord);
}


using (var writer = new StreamWriter("../../../problem-output.csv"))
using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
{
	csv.WriteRecords(output);
}


public record NamedInsuredOutput(string CustomerID, string NewMonthlyPremium);
public record NamedInsuredInput(string CustomerID, string FirstName, string LastName, string Email, string AddressLine1, string AddressLine2, string City, string State, string Zip, string FavoriteColor, string FavoriteQuote, string VehicleVIN, string VehicleMake, string VehicleModel, string VehicleYear, string VehicleColor, string CurrentMonthlyPremiumCents);
