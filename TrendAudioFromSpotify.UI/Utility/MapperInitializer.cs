﻿using AutoMapper;
using AutoMapper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

                                cfg.CreateMap<GroupDto, Group>();
                                cfg.CreateMap<PlaylistDto, Playlist>();
                                cfg.CreateMap<AudioDto, Audio>();
                            })
        {

        }
    }
}