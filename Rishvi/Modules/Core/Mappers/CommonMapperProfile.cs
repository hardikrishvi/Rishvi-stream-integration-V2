using System;
using AutoMapper;
using Rishvi.Modules.Core.DTOs;

namespace Rishvi.Modules.Core.Mappers
{
    public class CommonMapperProfile : Profile
    {
        public CommonMapperProfile()
        {
            CreateMap<DateTime, DateTimeDto>().ConvertUsing<DateTimeToDtoConverter>();
            CreateMap<DateTimeDto, DateTime>().ConvertUsing<DtoToDateTimeConverter>();

            // For Nullable DateTime
            CreateMap<DateTime?, DateTimeDto>().ConvertUsing<NullableDateTimeToDtoConverter>();
            
        }
    }

    public class DtoToDateTimeConverter : ITypeConverter<DateTimeDto, DateTime>
    {
        public DateTime Convert(DateTimeDto source, DateTime destination, ResolutionContext context)
        {
            return source == null ? DateTime.MinValue : new DateTime(source.Year, source.Month, source.Day);
        }
    }

    public class DateTimeToDtoConverter : ITypeConverter<DateTime, DateTimeDto>
    {
        public DateTimeDto Convert(DateTime source, DateTimeDto destination, ResolutionContext context)
        {
            return source == DateTime.MinValue ? null : new DateTimeDto(source.Year, source.Month, source.Day);
        }
    }

    public class NullableDateTimeToDtoConverter : ITypeConverter<DateTime?, DateTimeDto>
    {
        public DateTimeDto Convert(DateTime? source, DateTimeDto destination, ResolutionContext context)
        {
            return source == null || source == DateTime.MinValue
                ? null
                : new DateTimeDto(source.Value.Year, source.Value.Month, source.Value.Day);
        }
    }
}