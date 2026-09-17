using AutoMapper;
using Golbet.DTOs;
using Golbet.Entities;

namespace Golbet.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Match, MatchDto>()
            .ForMember(dest => dest.HomeTeamName,
                opt => opt.MapFrom(src => src.HomeTeam.Name))
            .ForMember(dest => dest.HomeTeamCrestUrl,
                opt => opt.MapFrom(src => src.HomeTeam.CrestUrl))
            .ForMember(dest => dest.AwayTeamName,
                opt => opt.MapFrom(src => src.AwayTeam.Name))
            .ForMember(dest => dest.AwayTeamCrestUrl,
                opt => opt.MapFrom(src => src.AwayTeam.CrestUrl));

        CreateMap<Match, MatchDetailDto>()
            .ForMember(dto => dto.TotalBets,
                options => options.MapFrom(match => match.Bets.Count));
    }
}