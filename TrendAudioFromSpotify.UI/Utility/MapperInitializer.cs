using AutoMapper;
using TrendAudioFromSpotify.Data.Model;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Utility
{
    public class MyMapper : Mapper
    {
        public MyMapper(IConfigurationProvider configurationProvider) : base(configurationProvider)
        {

        }
    }

    public class MyConfig : MapperConfiguration
    {
        public MyConfig() : base(cfg =>
                            {
                                cfg.CreateMap<Audio, AudioDto>();
                                cfg.CreateMap<Playlist, PlaylistDto>();
                                cfg.CreateMap<Group, GroupDto>();
                                cfg.CreateMap<MonitoringItem, MonitoringItemDto>().ForMember(x => x.Trends, opt => opt.Ignore());
                                cfg.CreateMap<Schedule, ScheduleDto>();

                                cfg.CreateMap<GroupDto, Group>();
                                cfg.CreateMap<PlaylistDto, Playlist>();
                                cfg.CreateMap<AudioDto, Audio>();
                                cfg.CreateMap<MonitoringItemDto, MonitoringItem>();
                                cfg.CreateMap<ScheduleDto, Schedule>();
                            })
        {

        }
    }
}
