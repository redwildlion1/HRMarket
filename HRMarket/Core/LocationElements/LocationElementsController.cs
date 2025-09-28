using HRMarket.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.Core.LocationElements;

public class LocationElementsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpPost("api/location-elements/counties")]
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
        using var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);
        var records = csv.GetRecords<CountyCsvRecord>().ToList();
        foreach (var record in records.Where(record => !dbContext.Counties.Any(c => c.Name == record.DENJ)))
        {
            dbContext.Counties.Add(new Entities.LocationElements.County
            {
                Name = record.DENJ,
                CountryId = existingCountry.Id
            });
        }
        dbContext.SaveChanges();
        return Ok();
    }
    
    private class CountyCsvRecord
    {
        public string DENJ { get; set; } = null!;
    }
}