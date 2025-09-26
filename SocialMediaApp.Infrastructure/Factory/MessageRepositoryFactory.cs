using Microsoft.Extensions.DependencyInjection;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Core.Interface.Factory;
using SocialMediaApp.Infrastructure.Repository.MessageRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Infrastructure.Factory
{
    public class MessageRepositoryFactory : IMessageRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageRepositoryFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        IMessageRepository IMessageRepositoryFactory.CreateRepository(string repositoryType)
        {
            return repositoryType switch
            {
                "TWO" => _serviceProvider.GetRequiredService<TwoSomeChatMessageRepository>(),
                "GROUP" => _serviceProvider.GetRequiredService<GroupChatMessageRepository>(),
                _ => null
            };
        }
    }
}
