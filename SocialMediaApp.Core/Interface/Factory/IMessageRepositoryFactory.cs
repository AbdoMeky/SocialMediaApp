using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Interface.Factory
{
    public interface IMessageRepositoryFactory
    {
        IMessageRepository CreateRepository(string repositoryType);

    }
}
