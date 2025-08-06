using AutoMapper;
using Matrix.Models;
using Matrix.DTOs;

namespace Matrix.Mappings
{
    /// <summary>
    /// AutoMapper 映射配置檔
    /// 定義所有 Model 與 DTO 之間的映射關係
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMappings();
        }

        private void CreateMappings()
        {
            // Person ↔ PersonDto 映射
            CreateMap<Person, PersonDto>()
                .ReverseMap();

            // Article ↔ ArticleDto 映射
            CreateMap<Article, ArticleDto>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author))
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies ?? new List<Reply>()));

            // ArticleDto ← Article (單向映射，用於讀取)
            CreateMap<CreateArticleDto, Article>()
                .ForMember(dest => dest.ArticleId, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorId, opt => opt.Ignore())
                .ForMember(dest => dest.CreateTime, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.PraiseCount, opt => opt.Ignore())
                .ForMember(dest => dest.CollectCount, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.Ignore())
                .ForMember(dest => dest.Replies, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore());

            // Reply ↔ ReplyDto 映射
            CreateMap<Reply, ReplyDto>()
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CreateTime, opt => opt.MapFrom(src => src.ReplyTime))
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User));

            CreateMap<CreateReplyDto, Reply>()
                .ForMember(dest => dest.ReplyId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.ReplyTime, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Article, opt => opt.Ignore());

            // Notification ↔ NotificationDto 映射
            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.NotificationId, opt => opt.MapFrom(src => src.NotifyId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.GetId))
                .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.SendId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsRead))
                .ForMember(dest => dest.CreateTime, opt => opt.MapFrom(src => src.SentTime))
                .ForMember(dest => dest.ReadTime, opt => opt.MapFrom(src => src.IsReadTime))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.Receiver))
                .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.Sender))
                .ForMember(dest => dest.Title, opt => opt.Ignore()) // 需要自定義邏輯
                .ForMember(dest => dest.Content, opt => opt.Ignore()); // 需要自定義邏輯

            // User ↔ UserDto 映射
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Person, opt => opt.MapFrom(src => src.Person));
            
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.Person, opt => opt.Ignore());
        }
    }
}