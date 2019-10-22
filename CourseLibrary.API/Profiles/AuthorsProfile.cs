using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;

namespace CourseLibrary.API.Profiles
{
    public class AuthorsProfile : Profile
    {
        public AuthorsProfile()
        {
            CreateMap<Author, AuthorDto>()
                .ForMember(
                    destination => destination.Name,
                    options => options.MapFrom(source => $"{source.FirstName} {source.LastName}"))
                .ForMember(
                    destination => destination.Age,
                    options => options.MapFrom(source => source.DateOfBirth.GetCurrentAge(source.DateOfDeath)));

            CreateMap<AuthorForCreationDto, Author>();
            CreateMap<AuthorForCreationWithDateOfDeath, Author>();
            CreateMap<Author, AuthorFullDto>();
        }
    }
}