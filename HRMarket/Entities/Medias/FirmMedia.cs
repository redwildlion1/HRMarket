using HRMarket.Configuration.Types;
using HRMarket.Entities.Firms;

namespace HRMarket.Entities.Medias;

public class FirmMedia
{
    public Guid Id { get; set; }
    public Guid FirmId { get; set; }
    public Firm Firm { get; set; } = null!;

    public Guid MediaId { get; set; }
    public Media Media { get; set; } = null!;

    public FirmMediaType FirmMediaType { get; set; }
}