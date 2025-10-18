using HRMarket.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.Core.LocationElements;

[Route("api/location-elements")]
public class LocationElementsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpPost("counties")]
    public IActionResult CreateCounty(IFormFile file, string country)
    {
        //Take a CSV file and a country name, create the country if it doesn't exist, then create the counties from the CSV file
        //Return 400 if the file is not a CSV file
        if (file.ContentType != "text/csv")
        {
            return BadRequest("File must be a CSV file");
        }

        var existingCountry = dbContext.Countries.FirstOrDefault(c => c.Name == country);
        if (existingCountry == null)
        {
            existingCountry = new Entities.LocationElements.Country { Name = country };
            dbContext.Countries.Add(existingCountry);
            dbContext.SaveChanges();
        }

        using var reader = new StreamReader(file.OpenReadStream());
        var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        };
        using var csv = new CsvHelper.CsvReader(reader, config);
        var records = csv.GetRecords<CountyCsvRecord>().ToList();

        foreach (var record in records.Where(record => !dbContext.Counties.Any(c => c.Name == record.Denj)))
        {
            dbContext.Counties.Add(new Entities.LocationElements.County
            {
                Name = record.Denj,
                CountryId = existingCountry.Id
            });
        }

        dbContext.SaveChanges();
        return Ok();
    }

    [HttpGet("countries")]
    public IActionResult GetCountries()
    {
        var countries = dbContext.Countries
            .Select(c => new { c.Id, c.Name })
            .ToList();
        return Ok(countries);
    }

    [HttpGet("{countryId:int}/counties")]
    public IActionResult GetLocationElements(int countryId)
    {
        var counties = dbContext.Counties
            .Where(c => c.CountryId == countryId)
            .Select(c => new { c.Id, c.Name })
            .ToList();
        return Ok(counties);
    }


    private class CountyCsvRecord
    {
        public string Denj { get; set; } = null!;
    }
}