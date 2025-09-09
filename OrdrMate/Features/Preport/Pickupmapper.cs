using AutoMapper;

namespace OrdrMate.Features.Preport;

public class PickupReportProfile : Profile
{
    public PickupReportProfile()
    {
        CreateMap<PickupReportDto, PickupReport>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ReportedTime, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<PickupReport, PickupReportDto>();
    }
}
